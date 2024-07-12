using System.ServiceModel.Syndication;
using System.Xml;
using Nulah.RSS.Core.Models;

namespace Nulah.RSS.Core;

public class RssReader
{
	/// <summary>
	/// Attempts to load the feed located at the given location. The location can be a remote URL or locally accessible file.
	/// <para>
	/// Any failure to load the feed from the location (such as network issues or files that can't be found)
	/// will throw an exception that should be handled
	/// </para>
	/// </summary>
	/// <param name="rssLocation">Remote URL or file path</param>
	/// <returns></returns>
	public RssDetail LoadRssDetails(string rssLocation)
	{
		return LoadRssDetail(rssLocation);
	}

	/// <summary>
	/// Returns all RssItems for a feed by given location. Empty or null values will throw an exception
	/// <para>
	/// Any failure to load the feed from the location (such as network issues or files that can't be found)
	/// will throw an exception that should be handled.
	/// </para>
	/// </summary>
	/// <param name="rssLocation"></param>
	/// <returns></returns>
	public List<RssItem> LoaddRssItems(string rssLocation)
	{
		if (string.IsNullOrEmpty(rssLocation))
		{
			throw new ArgumentNullException(nameof(rssLocation));
		}

		using var reader = XmlReader.Create(rssLocation);
		var syndicationFeed = SyndicationFeed.Load(reader);

		var items = new List<RssItem>();

		foreach (var syndicationItem in syndicationFeed.Items)
		{
			var item = new RssItem()
			{
				Title = syndicationItem.Title?.Text ?? "TITLE MISSING",
				Url = syndicationItem.ElementExtensions.FirstOrDefault(x => x.OuterName == "url")?.GetObject<string>()
				      ?? syndicationItem.Links.FirstOrDefault(x => x.RelationshipType == "alternate")?.Uri.AbsoluteUri
				      ?? "URL MISSING",
				Summary = syndicationItem.Summary?.Text,
				Published = syndicationItem.PublishDate,
				Author = GetAuthorFromSyndicationItem(syndicationItem)
			};

			// Some rss feeds have their content encoded, so we attempt to retrieve it in a different way
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

	private string? GetAuthorFromSyndicationItem(SyndicationItem syndicationItem)
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

	private RssDetail LoadRssDetail(string rssLocation)
	{
		if (string.IsNullOrEmpty(rssLocation))
		{
			throw new ArgumentNullException(nameof(rssLocation));
		}

		using var reader = XmlReader.Create(rssLocation);
		var syndicationFeed = SyndicationFeed.Load(reader);

		return new RssDetail()
		{
			Title = syndicationFeed.Title?.Text ?? "TITLE MISSING",
			ImageUrl = syndicationFeed.ImageUrl?.AbsoluteUri,
			Description = syndicationFeed.Description?.Text
		};
	}
}