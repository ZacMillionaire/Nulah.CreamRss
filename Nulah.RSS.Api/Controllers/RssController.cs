using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Nulah.RSS.Domain.Interfaces;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class RssController : ControllerBase
{
	private readonly ILogger<RssController> _logger;
	private readonly IFeedReader _feedReader;
	private readonly IFeedStorage _feedStorage;

	public RssController(IFeedReader feedReader, IFeedStorage feedStorage, ILogger<RssController> logger)
	{
		_logger = logger;
		_feedReader = feedReader;
		_feedStorage = feedStorage;
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

	/// <summary>
	/// Saves a given feed detail and returns the same feed with database properties populated. Feeds created this way
	/// are assumed to be valid and exist (or the <see cref="FeedDetail"/> is the result of a call to <see cref="Preview"/>)
	/// as they are being created with explicit details.
	/// <para>
	/// If <see cref="FeedDetail.Id"/> is 0 a new feed detail will be created.
	/// </para>
	/// <para>
	/// If a feed already exists by <see cref="FeedDetail.Location"/> and <see cref="FeedDetail.Id"/> is 0 or does not
	/// match the <see cref="FeedDetail.Id"/> of the previously saved feed an exception will be thrown.
	/// </para>
	/// <para>
	/// Repeated calls to this endpoint will update the given feedDetail by Id if it exists.
	/// </para>
	/// </summary>
	/// <param name="feedDetail"></param>
	/// <returns></returns>
	[HttpPost]
	[Route("[action]")]
	public async Task<ActionResult<FeedDetail?>> CreateFeedByDetail([FromBody] FeedDetail feedDetail)
	{
		var newFeed = await _feedStorage.CreateFeedDetail(feedDetail);

		return newFeed;
	}

	/// <summary>
	/// Updates a given feed detail matching on it's Id. If the location is being updated and matches another existing
	/// feed an exception will be thrown.
	/// </summary>
	/// <param name="feedDetail"></param>
	/// <returns></returns>
	[HttpPost]
	[Route("[action]")]
	public async Task<ActionResult<FeedDetail?>> UpdateFeedByDetail([FromBody] FeedDetail feedDetail)
	{
		var updatedFeed = await _feedStorage.UpdateFeedDetail(feedDetail);

		return updatedFeed;
	}

	/// <summary>
	/// First loads the feed from the given request, then saves and returns a <see cref="FeedDetail"/> with database properties populated.
	/// <para>
	/// If a feed already exists with the given location, it will be updated.
	/// </para>
	/// </summary>
	/// <param name="feedRequest"></param>
	/// <returns></returns>
	[HttpPost]
	[Route("[action]")]
	public async Task<ActionResult<FeedDetail?>> CreateOrUpdateFeed([FromBody] FeedRequest feedRequest)
	{
		try
		{
			var feedDetail = _feedReader.ParseFeedDetails(feedRequest.FeedLocation);
			// Check if we already have this feed by location, if we do we update it with the details we parsed previously
			// effectively discarding _all_ previous details of this feed if they differ.
			// TODO: This feels really gross and I might refactor this into a distinct create/update later, but it does work
			if (await _feedStorage.GetFeedByLocation(feedRequest.FeedLocation) is { } existingByLocation)
			{
				// Set the Id to the existing location for downstream implementations
				feedDetail.Id = existingByLocation.Id;
				return await _feedStorage.UpdateFeedDetail(feedDetail);
			}

			return await _feedStorage.CreateFeedDetail(feedDetail);
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

	/// <summary>
	/// Loads each given location, and if it doesn't exist, creates it, otherwise it will update it.
	/// <para>
	/// This result may contain errors but will still return 200 OK so it is up to the caller to check for any errors
	/// </para>
	/// </summary>
	/// <param name="batchRequest"></param>
	/// <returns></returns>
	[HttpPost]
	[Route("/batch/[controller]/[action]")]
	public async Task<ActionResult<BatchFeedResult>> CreateFeed([FromBody] BatchFeedRequest batchRequest)
	{
		var batchResult = new BatchFeedResult();
		if (batchRequest.Locations.Count == 0)
		{
			batchResult.Errors.Add("No rss locations given to create");
			// return to avoid a needless enumeration attempt below
			return new BadRequestObjectResult(batchResult);
		}

		foreach (var feedLocation in batchRequest.Locations)
		{
			try
			{
				var feedDetail = _feedReader.ParseFeedDetails(feedLocation);
				var createdFeed = await _feedStorage.CreateFeedDetail(feedDetail);
				batchResult.CreatedFeeds.Add(createdFeed);
			}
			catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
			{
				batchResult.Errors.Add($@"Unable to load feed from ""{feedLocation}""");
			}
			catch (Exception ex) when (ex is ArgumentNullException)
			{
				batchResult.Errors.Add("Feed location is required");
			}
			catch (Exception ex)
			{
				batchResult.Errors.Add(ex.Message);
			}
		}

		// If we have errors and no created feeds, return a 400 bad request.
		// What happens if we have _no_ errors and _no_ created feeds? Who knows but that isn't something I want to
		// try and figure out how to replicate until someone does
		if (batchResult.Errors.Count != 0 && batchResult.CreatedFeeds.Count == 0)
		{
			return new BadRequestObjectResult(batchResult);
		}

		// Otherwise if we have at least 1 created feed and 0 or more errors, return a 200 ok
		// partial success is still a success as the caller is expected to check for errors
		return Ok(batchResult);
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