using System.ServiceModel.Syndication;
using System.Xml;
using HtmlAgilityPack;
using Nulah.RSS.Domain.Interfaces;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Core;

public class FeedReader : IFeedReader
{
	private readonly HttpClient _client;

	public FeedReader(HttpClient client)
	{
		_client = client;
	}

	/// <summary>
	/// Attempts to parse the feed located at the given location. The location can be a remote URL or locally accessible file.
	/// <para>
	/// Any title or description values will be trimmed of any whitespace
	/// </para>
	/// <para>
	/// Any failure to load the feed from the location (such as network issues or files that can't be found)
	/// will throw an exception that should be handled.
	/// </para>
	/// </summary>
	/// <param name="feedLocation">Remote URL or file path</param>
	/// <returns></returns>
	public async Task<FeedDetail> ParseFeedDetails(string? feedLocation)
	{
		return await LoadFeedDetail(feedLocation, _client);
	}

	/// <summary>
	/// Returns all <see cref="FeedItem"/> for a feed by given location. Empty or null values will throw an exception.
	/// <para>
	/// Any failure to load the feed from the location (such as network issues or files that can't be found)
	/// will throw an exception that should be handled.
	/// </para>
	/// </summary>
	/// <param name="rssLocation"></param>
	/// <returns></returns>
	public List<FeedItem> ParseFeedItems(string rssLocation)
	{
		if (string.IsNullOrWhiteSpace(rssLocation))
		{
			throw new ArgumentNullException(nameof(rssLocation));
		}

		using var reader = XmlReader.Create(rssLocation);
		var syndicationFeed = SyndicationFeed.Load(reader);

		var items = new List<FeedItem>();

		foreach (var syndicationItem in syndicationFeed.Items)
		{
			var item = new FeedItem()
			{
				Title = syndicationItem.Title?.Text ?? "TITLE MISSING",
				Url = syndicationItem.ElementExtensions.FirstOrDefault(x => x.OuterName == "url")?.GetObject<string>()
				      ?? syndicationItem.Links.FirstOrDefault(x => x.RelationshipType == "alternate")?.Uri.AbsoluteUri
				      ?? "URL MISSING",
				Summary = syndicationItem.Summary?.Text,
				Published = syndicationItem.PublishDate,
				Author = GetAuthorFromSyndicationItem(syndicationItem)
			};

			// Some feeds have their content encoded, so we attempt to retrieve it in a different way
			if (syndicationItem.ElementExtensions.FirstOrDefault(x => x.OuterNamespace == "http://purl.org/rss/1.0/modules/content/")
			    is { } encodedContent)
			{
				item.Content = encodedContent.GetObject<string>();
			}
			// If we have text content, use it
			else if (syndicationItem.Content is TextSyndicationContent { } textContent)
			{
				item.Content = textContent.Text;
			}
			// otherwise use the summary text as content as a fallback
			else
			{
				item.Content = item.Summary;
			}

			items.Add(item);
		}

		return items;
	}

	private static string? GetAuthorFromSyndicationItem(SyndicationItem syndicationItem)
	{
		// For ArsTechnica, DevTo, and atom feeds that use the http://purl.org/dc/elements/1.1/ name space (often wordpress)
		if (syndicationItem.ElementExtensions.FirstOrDefault(x => x.OuterName == "creator")?.GetObject<string>() is { } creator)
		{
			return creator;
		}

		// No authors return null
		if (syndicationItem.Authors.Count == 0)
		{
			return default;
		}

		// Single author return either the name, email or uri, in that order, for the first one that is not nill
		if (syndicationItem.Authors.Count == 1)
		{
			var author = syndicationItem.Authors.First();

			// For github atom feeds
			if (!string.IsNullOrWhiteSpace(author.Name))
			{
				return author.Name;
			}

			// For certain webtoon feeds as they only give a single <author> field which the
			// reader populates into the email property
			if (!string.IsNullOrWhiteSpace(author.Email))
			{
				return author.Email;
			}

			// Last chance fall back
			if (!string.IsNullOrWhiteSpace(author.Uri))
			{
				return author.Email;
			}
		}

		return default;
	}

	/// <summary>
	/// Retrieves the feed from the given location and returns the details if successful. Throws any exceptions otherwise.
	/// </summary>
	/// <param name="feedLocation"></param>
	/// <param name="client"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException">Thrown if <see cref="feedLocation"/> is null or empty</exception>
	private static async Task<FeedDetail> LoadFeedDetail(string? feedLocation, HttpClient client)
	{
		// Shortcut any checks done by XmlReader and fail early if we don't have a location
		if (string.IsNullOrWhiteSpace(feedLocation))
		{
			throw new ArgumentNullException(nameof(feedLocation));
		}

		using var reader = XmlReader.Create(feedLocation);
		var syndicationFeed = SyndicationFeed.Load(reader);

		// We normalise feed details with text fields to have no leading or trailing spaces
		return new FeedDetail()
		{
			Title = syndicationFeed.Title?.Text.Trim() ?? "TITLE MISSING",
			ImageUrl = syndicationFeed.ImageUrl?.AbsoluteUri,
			ImageBlob = await LoadRemoteImage(syndicationFeed.ImageUrl?.AbsoluteUri, client),
			// We attempt to load the favicon from the alternative link resolved from a feed (if it exists)
			// we do this because sometimes a feed may be located under a subdomain (eg, ArsTechnica), and attempting to
			// resolve the favicon from the html header in these cases will fail, as more often than not these locations
			// are 404 pages. We also can't just go from the base address because I cannot be bothered removing all the subdomains,
			// and a subdomain may have its own fav-icon and I'd rather go from the alternative (full page for the feed generally),
			// instead of guessing like other feed readers might.
			Favicon = await RetrieveFavicon(syndicationFeed.Links.FirstOrDefault(x => x.RelationshipType == "alternate")?.Uri, client),
			Description = syndicationFeed.Description?.Text.Trim(),
			Location = feedLocation
		};
	}

	private static async Task<byte[]?> LoadRemoteImage(string? remoteImageAddress, HttpClient client)
	{
		if (!string.IsNullOrWhiteSpace(remoteImageAddress)
		    // resolveuri takes the address last instead of converting it to a uri as it should be an absolute
		    // path, either remotre or filesystem local
		    && XmlResolver.FileSystemResolver.ResolveUri(null, remoteImageAddress) is { IsFile: false })
		{
			var imageStream = await client.GetByteArrayAsync(remoteImageAddress);
			return imageStream;
		}

		// File based image addresses are always null for image blobs
		return null;
	}

	/// <summary>
	/// Attempts to retrieve the RSS icon for a feed by locating the favicon from the websites authoritive location -
	/// basically from the http(s) to the end of the .tld. and parsing the html header for 'icon' or 'shortcut icon'
	/// </summary>
	/// <param name="feedLocation"></param>
	/// <param name="client"></param>
	/// <returns></returns>
	private static async Task<byte[]?> RetrieveFavicon(string feedLocation, HttpClient client)
	{
		if (!string.IsNullOrWhiteSpace(feedLocation)
		    && XmlResolver.FileSystemResolver.ResolveUri(null, feedLocation) is { IsFile: false })
		{
			return await RetrieveFavicon(new Uri(feedLocation), client);
		}

		return null;
	}

	/// <summary>
	/// Attempts to retrieve the RSS icon for a feed by locating the favicon from the websites authoritive location -
	/// basically from the http(s) to the end of the .tld. and parsing the html header for 'icon' or 'shortcut icon'
	/// </summary>
	/// <param name="feedLocation"></param>
	/// <param name="client"></param>
	/// <returns></returns>
	private static async Task<byte[]?> RetrieveFavicon(Uri? feedLocation, HttpClient client)
	{
		// What I thought would be trivial to do I quickly realised it wasn't.
		// What the _fuck_ favicons why are you such a wide range of guessing to know what to display, no wonder webdev
		// is a mess of, "well it could be any 16 of these things and it's up to the browser to know what to do lol!".
		// At least the websites I make aren't trying to cover for 200 different standards and I can just pick one and
		// if others don't support it then lol.
		// wait shit that's probably why this method is what it is, because that's what everyone else is doing.
		if (feedLocation != null
		    && XmlResolver.FileSystemResolver.ResolveUri(feedLocation, null) is { IsFile: false })
		{
			var web = new HtmlWeb();
			// we use GetLeftPart here to get the scheme and authority of the uri, or in human words: we get the https part + everything up to the end of the tld.
			// we can't use .Path because htmlagility cannot handle such a complex task of missing the scheme fragment 🙃
			// TODO: handle cases with subdomains and strip them out
			var htmlDoc = await web.LoadFromWebAsync(feedLocation.GetLeftPart(UriPartial.Authority));
			// Find all nodes that in the head of the document with a rel of either 'icon' or 'shortcut icon', both of which can refer to the favicon source
			var faviconNodes = htmlDoc.DocumentNode.SelectNodes("/html/head/link[@rel='icon' or @rel='shortcut icon' or @rel='alternate icon' and @href]");

			// TODO: honestly this sucks, I should first find the link with icon, then if that doesn't exist (or its an svg), check each successive type
			if (faviconNodes is { Count: not 0 })
			{
				// Loop over any we found, and return the first _non_ svg icon. Special shout outs to GitHub for making me
				// learn that image/svg+xml is a valid favicon source.
				// Why are we excluding svg icons? Because there's no trivial way to display or render these in a desktop
				// application.
				// Yeah web apps can but I also don't want to store an svg as these can be significantly larger than
				// other icon formats.
				foreach (var faviconNode in faviconNodes)
				{
					// Skip over this node if it's an svg - we can't display them currently
					if (faviconNode.Attributes.Contains("type")
					    && faviconNode.Attributes["type"].Value.Contains("svg"))
					{
						continue;
					}
					// There's a risk we just skipped over a potential svg because we were counting on the link to have a
					// type attribute that isn't lying to us.
					// ArsTechnica is guilty of this, but at least in their case I know its not an svg. For others I'm
					// basically just guessing

					var favIcon = faviconNode.Attributes["href"].Value;
					// We should probably check that its an image, but eh, I'll regret not doing that when it becomes a problem.
					// Or if I come back to this TODO to check the favicon is actually an image and not something malicious
					var imageStream = await client.GetByteArrayAsync(favIcon);
					return imageStream;
				}
			}
		}

		return null;
	}
}