using System;
using System.Globalization;
using System.IO;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Nulah.RSS.Avalonia.Views;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
	}
}

public class ByteArrayToImageConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (targetType == typeof(IImage))
		{
			if (value is byte[] { Length: > 0 } imageBlob)
			{
				return new Bitmap(new MemoryStream(imageBlob));
			}

			// Return a default icon if we have no valid value
			return new Bitmap(AssetLoader.Open(new Uri("avares://Nulah.RSS.Avalonia/Assets/default_rss_icon.ico")));
		}

		return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}