using System.ServiceModel.Syndication;
using System.Xml;
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
			// TODO: also duplicate implementation of LoadRemoteImage to load a favicon into this property
			// Favicon = TODO
			Description = syndicationFeed.Description?.Text.Trim(),
			Location = feedLocation
		};
	}

	private static async Task<byte[]?> LoadRemoteImage(string? remoteImageAddress, HttpClient client)
	{
		if (!string.IsNullOrWhiteSpace(remoteImageAddress)
		    && XmlResolver.FileSystemResolver.ResolveUri(null, remoteImageAddress) is { IsFile: false })
		{
			var imageStream = await client.GetByteArrayAsync(remoteImageAddress);
			return imageStream;
		}

		// File based image addresses are always null for image blobs
		return null;
	}
}