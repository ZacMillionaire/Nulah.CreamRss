using System;
using System.IO;
using Avalonia.Platform;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Avalonia.ViewModels.DesignTime;

public class AddEditFeedDesignViewModel : AddEditFeedViewModel
{
	public AddEditFeedDesignViewModel()
	{
		// FeedDetail = new FeedDetail()
		// {
		// 	Description = "This is a design time feed description for an RSS feed that does not exist lol! But boy howdy is this going to be a long long message so I can do some wrapping." +
		// 	              "Turns out we need to actually make this a lot longer to make sure that the display image correctly stays a circle and that no UI changes have caused it to" +
		// 	              "potentially stretch or not be constrained." +
		// 	              "This is also really long to make sure that the displayed image remains at the top left of the view and isn't trying to align itself with the center of the container" +
		// 	              "(unless of course it's been told to do so)",
		// 	Location = "http://does.not/exist",
		// 	Title = "Design time RSS feed",
		// 	CreatedUtc = DateTimeOffset.Now.AddMinutes(-2),
		// 	UpdatedUtc = DateTimeOffset.Now.AddMinutes(-1)
		// };
	}
}