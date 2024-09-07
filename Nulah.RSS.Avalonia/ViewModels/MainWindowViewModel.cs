using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using Nulah.RSS.Domain.Interfaces;
using Nulah.RSS.Domain.Models;
using ReactiveUI;
using Splat;

namespace Nulah.RSS.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	private readonly IFeedManager? _feedManager;
	private List<FeedDetail> _feeds = new();
	private ViewModelBase? _windowContent;
	private FeedDetail? _selectedFeedDetail;
	private bool _feedListAvailable;

	public ICommand OpenAddEditFeedWindowCommand { get; }

	public List<FeedDetail> Feeds
	{
		get => _feeds;
		set => this.RaiseAndSetIfChanged(ref _feeds, value);
	}

	public ViewModelBase WindowContent
	{
		get => _windowContent ?? new();
		set => this.RaiseAndSetIfChanged(ref _windowContent, value);
	}

	public FeedDetail? SelectedFeedDetail
	{
		get => _selectedFeedDetail;
		set => this.RaiseAndSetIfChanged(ref _selectedFeedDetail, value);
	}

	/// <summary>
	/// Indicates if the feed list is loaded and ready for interaction
	/// </summary>
	public bool FeedListAvailable
	{
		get => _feedListAvailable;
		set => this.RaiseAndSetIfChanged(ref _feedListAvailable, value);
	}

	public MainWindowViewModel(IFeedManager? feedManager = null)
	{
		_feedManager = feedManager ?? Locator.Current.GetService<IFeedManager>();

		OpenAddEditFeedWindowCommand = ReactiveCommand.Create(() => OpenAddEditFeedWindow());

		this.WhenAnyValue(x => x.SelectedFeedDetail)
			.Where(x => x != null)
			.Subscribe(OpenAddEditFeedWindow);

		// Avoid any runtime specific code such as loading feeds if we're in the editor
		if (Design.IsDesignMode) return;
		OnFeedListUpdated();
	}

	private void OpenAddEditFeedWindow(FeedDetail? feedDetail = null)
	{
		// If the incoming feedDetail is null, we've most likely clicked the add feed button (or some other event source
		// has called a similar behaviour).
		// In these instances we want to deselect any previously selected list item from the main feed list
		if (feedDetail == null)
		{
			SelectedFeedDetail = null;
		}

		if (WindowContent is not AddEditFeedViewModel addEditFeedViewModel)
		{
			WindowContent = new AddEditFeedViewModel()
			{
				// Not sure if this creates a leak the same way failing to unbind an event does, but seeing as its a property
				// reference to a method here, it _should_ clean up correctly, but I've honestly never investigated it
				// and unsure of if it'll actually be a problem.
				// I'm juust doing it this way because I really _cannot_ be fucked doing event unbinds (-=) for future
				// content models I may want to add and maintaining that sounds worse than any potential leaks that may
				// be created here.
				// I also don't intend this to be multicast and this can only be set in init
				FeedListUpdate = OnFeedListUpdated,
				FeedDetail = feedDetail
			};
		}
		else
		{
			addEditFeedViewModel.FeedDetail = feedDetail;
			// Reset the feeduri as well
			addEditFeedViewModel.FeedUri = null;
		}
	}

	private void OnFeedListUpdated()
	{
		Dispatcher.UIThread.InvokeAsync(async () =>
		{
			FeedListAvailable = false;
			Feeds = await _feedManager!.GetFeedDetails();
			FeedListAvailable = true;
		});
	}
}