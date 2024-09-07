using Avalonia.Controls;
using Avalonia.Input;
using Nulah.RSS.Avalonia.ViewModels;

namespace Nulah.RSS.Avalonia.Views;

public partial class AddEditFeedView : UserControl
{
	public AddEditFeedView()
	{
		InitializeComponent();
	}

	private void HandleFeedUriKeyEvent(object? sender, KeyEventArgs e)
	{
		((AddEditFeedViewModel?)DataContext)?.HandleFeedUriKeyEvent(e);
	}
}