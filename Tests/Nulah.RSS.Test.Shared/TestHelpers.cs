﻿namespace Nulah.RSS.Test.Shared;

public sealed class TestHelpers
{
	/// <summary>
	/// Returns the contents of the image by name, located under TestFiles/SampleRssFeeds/Images.
	/// </summary>
	/// <param name="imageName"></param>
	/// <returns></returns>
	public static byte[] LoadImageResource(string imageName)
	{
		return File.ReadAllBytes($"./TestFiles/SampleRssFeeds/Images/{imageName}");
	}
}