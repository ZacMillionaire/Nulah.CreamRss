using System;
using System.Globalization;
using System.IO;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Nulah.RSS.Avalonia.Converters;

public class ByteArrayToImageConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (targetType == typeof(IImage))
		{
			if (value is byte[] { Length: > 0 } imageBlob)
			{
				try
				{
					return new Bitmap(new MemoryStream(imageBlob));
				}
				catch (Exception ex)
				{
					// TODO: log to a file
					// Fallback if the content of imageBlob is not a valid image (eg, the image the imageBlob represents was
					// actually an SVG)
					return new Bitmap(AssetLoader.Open(new Uri("avares://Nulah.RSS.Avalonia/Assets/default_rss_icon.ico")));
				}
			}

			// Return a default icon if we have no valid value
			return new Bitmap(AssetLoader.Open(new Uri("avares://Nulah.RSS.Avalonia/Assets/default_rss_icon.ico")));
		}

		return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}