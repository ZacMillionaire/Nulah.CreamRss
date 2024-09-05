using System;
using System.IO;
using Avalonia.Platform;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Avalonia.ViewModels.DesignTime;

public class AddEditFeedDesignViewModel : AddEditFeedViewModel
{
	public AddEditFeedDesignViewModel()
	{
		FeedDetail = new FeedDetail()
		{
			Description = "This is a design time feed description for an RSS feed that does not exist lol! But boy howdy is this going to be a long long message so I can do some wrapping",
			Location = "http://does.not/exist",
			Title = "Design time RSS feed",
			CreatedUtc = DateTimeOffset.Now.AddMinutes(-2),
			UpdatedUtc = DateTimeOffset.Now.AddMinutes(-1)
		};
	}
}