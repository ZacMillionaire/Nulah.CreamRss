using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using Nulah.RSS.Domain.Interfaces;
using Nulah.RSS.Domain.Models;
using ReactiveUI;
using Splat;

namespace Nulah.RSS.Avalonia.ViewModels;

public class AddEditFeedViewModel : ViewModelBase
{
	private FeedDetail? _feedDetail;
	private string? _feedUri;
	public ICommand LoadAndSaveFeedDetailsCommand { get; }
	private readonly IFeedManager? _feedManager;
	private readonly IFeedReader? _feedReader;

	public FeedDetail? FeedDetail
	{
		get => _feedDetail;
		set => this.RaiseAndSetIfChanged(ref _feedDetail, value);
	}

	public string? FeedUri
	{
		get => _feedUri;
		set => this.RaiseAndSetIfChanged(ref _feedUri, value);
	}

	/// <summary>
	/// Method to be called when feeds update
	/// </summary>
	public Action? FeedListUpdate { get; init; }

	public AddEditFeedViewModel(IFeedManager? feedManager = null, IFeedReader? feedReader = null)
	{
		_feedManager = feedManager ?? Locator.Current.GetService<IFeedManager>();
		_feedReader = feedReader ?? Locator.Current.GetService<IFeedReader>();
		LoadAndSaveFeedDetailsCommand = ReactiveCommand.Create(LoadAndSaveFeedDetails);
	}

	private void LoadAndSaveFeedDetails()
	{
		if (!Design.IsDesignMode)
		{
			// This is some messy code to make ParseFeedDetails pretend its async.
			// Under the current implementation there's a messy forced synchronous .Result on something
			// that should instead be awaited, and the method change should be made to support async.
			// It's an easy update and I've validated it to work, I just don't want to update the tests for it
			// otherwise I'll never get this fucking thing finished.
			Dispatcher.UIThread.InvokeAsync(async () =>
			{
				await Task.Run(async () =>
				{
					// If we've already added this source, set and forget. Feeds are distinct by feed location
					if (await _feedManager!.GetFeedDetail(_feedUri) is { } existingFeed)
					{
						FeedDetail = existingFeed;
					}
					else
					{
						// Otherwise parse the source and off we go
						var parsedFeed = _feedReader!.ParseFeedDetails(_feedUri);
						// Naturally, of course, saving the FeedDetail is async, but we still need to keep this wrapped in
						// a Task.Run to make ParseFeedDetails behave nicely
						FeedDetail = await _feedManager!.CreateFeedDetail(parsedFeed);
						FeedListUpdate?.Invoke();
					}
				});
			});
		}
	}
}