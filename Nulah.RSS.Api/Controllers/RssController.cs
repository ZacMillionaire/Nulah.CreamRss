using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Nulah.RSS.Core;
using Nulah.RSS.Domain.Interfaces;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class RssController : ControllerBase
{
	private readonly ILogger<RssController> _logger;
	private readonly IFeedReader _feedReader;

	public RssController(IFeedReader feedReader, IFeedStorage feedStorage, ILogger<RssController> logger)
	{
		_logger = logger;
		_feedReader = feedReader;
	}

	public class RssItem
	{
		public string Title { get; set; } = null!;
		public string Url { get; set; } = null!;
		public string? Summary { get; set; }
		public string? Content { get; set; }
	}

	/// <summary>
	/// Loads and attempts to parse the feed at the given url. This can either be a remote address or locally available
	/// file if available from the host.
	/// <para>
	///	This endpoint does not save the parsed feed, however the response can be used to save if required.
	/// </para>
	/// </summary>
	/// <param name="feedRequest">If null or empty, an bad request will be returned. Can be a remote address or local file resource</param>
	/// <returns></returns>
	[HttpPost]
	[Route("[action]")]
	public ActionResult<FeedDetail?> Preview([FromBody] FeedRequest feedRequest)
	{
		try
		{
			var feedDetail = _feedReader.ParseFeedDetails(feedRequest.FeedLocation);
			return Ok(feedDetail);
		}
		catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
		{
			return new BadRequestObjectResult($@"Unable to load feed from ""{feedRequest.FeedLocation}""");
		}
		catch (Exception ex) when (ex is ArgumentNullException)
		{
			return new BadRequestObjectResult("Feed location is required");
		}
		catch (Exception ex)
		{
			return new BadRequestObjectResult(ex.Message);
		}
	}

	[HttpPost]
	[Route("[action]")]
	public IEnumerable<RssItem> GetRssItems([FromBody] string url)
	{
		if (string.IsNullOrEmpty(url))
		{
			return [];
		}

		try
		{
			using var reader = XmlReader.Create(url);
			var a = SyndicationFeed.Load(reader);

			var items = new List<RssItem>();

			foreach (var syndicationItem in a.Items)
			{
				var item = new RssItem()
				{
					Title = syndicationItem.Title?.Text ?? "TITLE MISSING",
					Url = syndicationItem.ElementExtensions.FirstOrDefault(x => x.OuterName == "url")?.GetObject<string>()
					      ?? syndicationItem.Links.FirstOrDefault(x => x.RelationshipType == "alternate")?.Uri.AbsoluteUri
					      ?? "URL MISSING",
					Summary = syndicationItem.Summary?.Text,
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
		catch (Exception ex)
		{
			// TODO: handle this better by returning an error response
			return [];
		}
	}
}