using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using Nulah.RSS.Avalonia.Views;
using Nulah.RSS.Domain.Interfaces;
using Nulah.RSS.Domain.Models;
using ReactiveUI;
using Splat;

namespace Nulah.RSS.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	private readonly IFeedManager? _feedManager;
	private List<FeedDetail> _feeds = new();
	private Window? _owner;
	private ViewModelBase? _windowContent;

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

	public MainWindowViewModel(IFeedManager? feedManager = null, Window? owner = null)
	{
		_feedManager = feedManager ?? Locator.Current.GetService<IFeedManager>();
		_owner = owner;
		OpenAddEditFeedWindowCommand = ReactiveCommand.Create(OpenAddEditFeedWindow);

		Dispatcher.UIThread.InvokeAsync(async () =>
		{
			Feeds = await _feedManager!.GetFeedDetails();
		});
	}

	private void OpenAddEditFeedWindow()
	{
		WindowContent = new AddEditFeedViewModel();
		// if (_owner != null)
		// {
		// 	var edit = new AddEditFeedWindow();
		// 	edit.ShowDialog(_owner);
		// }
	}
}