using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Nulah.RSS.Avalonia.Controls;

public class ByteArrayImage : TemplatedControl
{
	public static readonly StyledProperty<byte[]?> ByteArrayProperty
		= AvaloniaProperty.Register<ByteArrayImage, byte[]?>(nameof(ByteArray));

	public byte[]? ByteArray
	{
		get => GetValue(ByteArrayProperty);
		set => SetValue(ByteArrayProperty, value);
	}
}