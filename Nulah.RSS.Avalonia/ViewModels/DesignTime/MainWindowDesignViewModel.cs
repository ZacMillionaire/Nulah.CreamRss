using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Avalonia.ViewModels.DesignTime;

public class MainWindowDesignViewModel : MainWindowViewModel
{
	public MainWindowDesignViewModel()
	{
		Feeds = Enumerable.Range(0, 10)
			.Select(x => new FeedDetail()
			{
				Title = $"RSS Feed Item {x} asdf asdf asdf asdfasf asf "
			})
			.ToList();
		FeedListAvailable = true;
	}
}