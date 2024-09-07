using System;
using System.IO;
using System.Net.Http;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Nulah.RSS.Avalonia.ViewModels;
using Nulah.RSS.Avalonia.Views;
using Nulah.RSS.Core;
using Nulah.RSS.Data;
using Nulah.RSS.Domain.Interfaces;
using Splat;

namespace Nulah.RSS.Avalonia;

public partial class App : Application
{
	public override void Initialize()
	{
		if (!Design.IsDesignMode)
		{
			AddCommonServices();
		}

		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.MainWindow = new MainWindow()
			{
				DataContext = new MainWindowViewModel()
			};
		}

		base.OnFrameworkInitializationCompleted();
	}

	private static void AddCommonServices()
	{
		Locator.CurrentMutable.RegisterConstant<FeedContext>(new FeedContext(FeedContext.BuildOptionsFromConnectionString($"Data Source={AppContext.BaseDirectory}/data/nulah.rss.db")));
		Locator.CurrentMutable.RegisterConstant<HttpClient>(new HttpClient(new SocketsHttpHandler
		{
			PooledConnectionLifetime = TimeSpan.FromMinutes(15) // Recreate every 15 minutes
		}));
		Locator.CurrentMutable.Register<IFeedManager>(() => new FeedManager(Locator.Current.GetService<FeedContext>()!));
		Locator.CurrentMutable.Register<IFeedStorage>(() => new FeedStorage(Locator.Current.GetService<IFeedManager>()!));
		Locator.CurrentMutable.Register<IFeedReader>(() => new FeedReader(Locator.Current.GetService<HttpClient>()!));

		// Ensure the data directory exists before attempting to create a database
		Directory.CreateDirectory($"{AppContext.BaseDirectory}/data");

		var ctx = Locator.Current.GetService<FeedContext>();
		ctx?.EnsureExists();
	}
}