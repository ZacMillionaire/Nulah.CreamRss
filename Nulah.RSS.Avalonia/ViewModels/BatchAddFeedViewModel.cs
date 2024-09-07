using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ReactiveUI;

namespace Nulah.RSS.Avalonia.ViewModels;

public class BatchAddFeedViewModel : ViewModelBase
{
	private List<string> _feedUris = new();
	private string? _feedUri;

	public List<string> FeedUris
	{
		get => _feedUris;
		set => this.RaiseAndSetIfChanged(ref _feedUris, value);
	}

	public string? FeedUri
	{
		get => _feedUri;
		set => this.RaiseAndSetIfChanged(ref _feedUri, value);
	}

	public void HandleFeedUriKeyEvent(KeyEventArgs arg)
	{
		// Load and save feed details if the user hit enter. Same behaviour as if they clicked the save button
		if (arg.Key == Key.Enter)
		{
			AddFeedUriToList();
		}
	}

	/// <summary>
	/// Adds the current FeedUri to the list of feeds to save, and clears the previous value.
	/// </summary>
	private void AddFeedUriToList()
	{
		if (!string.IsNullOrWhiteSpace(FeedUri))
		{
			FeedUris.Add(FeedUri);
			this.RaisePropertyChanged(nameof(FeedUris));
			FeedUri = null;
		}
	}

	private void LoadAndSaveFeedDetails()
	{
		if (!Design.IsDesignMode)
		{
			Dispatcher.UIThread.InvokeAsync(async () =>
			{
				// try
				// {
				// 	// If we've already added this source, set and forget. Feeds are distinct by feed location
				// 	if (await _feedManager!.GetFeedDetail(_feedUri) is { } existingFeed)
				// 	{
				// 		FeedDetail = existingFeed;
				// 	}
				// 	else
				// 	{
				// 		// Otherwise parse the source and off we go
				// 		var parsedFeed = await _feedReader!.ParseFeedDetails(_feedUri);
				// 		FeedDetail = await _feedManager!.CreateFeedDetail(parsedFeed);
				// 		FeedListUpdate?.Invoke();
				// 	}
				// }
				// catch (Exception ex)
				// {
				// 	await File.AppendAllTextAsync("./app.log", ex.Message);
				// }
				// finally
				// {
				// 	await File.AppendAllTextAsync("./app.log", "done lol");
				// 	// ensure the FeedUri value is empty
				// 	FeedUri = null;
				// }
			});
		}
	}
}