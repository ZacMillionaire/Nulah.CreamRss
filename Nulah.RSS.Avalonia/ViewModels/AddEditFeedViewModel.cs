using System;
using System.IO;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
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
	private bool _isLoadingFeed;
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

	public bool IsLoadingFeed
	{
		get => _isLoadingFeed;
		set => this.RaiseAndSetIfChanged(ref _isLoadingFeed, value);
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

	public void HandleFeedUriKeyEvent(KeyEventArgs arg)
	{
		// Load and save feed details if the user hit enter. Same behaviour as if they clicked the save button
		if (arg.Key == Key.Enter)
		{
			LoadAndSaveFeedDetails();
		}
	}

	private void LoadAndSaveFeedDetails()
	{
		if (!Design.IsDesignMode)
		{
			Dispatcher.UIThread.InvokeAsync(async () =>
			{
				try
				{
					IsLoadingFeed = true;
					// If we've already added this source, set and forget. Feeds are distinct by feed location
					if (await _feedManager!.GetFeedDetail(_feedUri) is { } existingFeed)
					{
						FeedDetail = existingFeed;
					}
					else
					{
						// Otherwise parse the source and off we go
						var parsedFeed = await _feedReader!.ParseFeedDetails(_feedUri);
						FeedDetail = await _feedManager!.CreateFeedDetail(parsedFeed);
						FeedListUpdate?.Invoke();
					}
				}
				catch (Exception ex)
				{
					await File.AppendAllTextAsync("./app.log", ex.Message);
				}
				finally
				{
					await File.AppendAllTextAsync("./app.log", "done lol");
					// ensure the FeedUri value is empty
					FeedUri = null;
					IsLoadingFeed = false;
				}
			});
		}
	}
}