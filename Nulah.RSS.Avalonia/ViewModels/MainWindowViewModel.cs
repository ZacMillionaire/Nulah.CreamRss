using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Platform;
using Nulah.RSS.Domain.Interfaces;
using Nulah.RSS.Domain.Models;
using ReactiveUI;
using Splat;

namespace Nulah.RSS.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
	public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static

	private List<FeedDetail> _feeds = new();

	public List<FeedDetail> Feeds
	{
		get => _feeds;
		set => this.RaiseAndSetIfChanged(ref _feeds, value);
	}

	private readonly IFeedManager? _feedManager;

	public MainWindowViewModel(IFeedManager? feedManager = null)
	{
		_feedManager = feedManager ?? Locator.Current.GetService<IFeedManager>();
	}
}