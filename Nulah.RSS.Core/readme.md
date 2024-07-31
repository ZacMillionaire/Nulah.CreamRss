# About

This project is responsible for the core implementation of reading and retrieving feeds.

For anything wishing to build their own frontend - this is the project to include that will provide all non-storage behaviours.

## FeedReader.cs
Used for parsing RSS feeds

## FeedStorage.cs
Should probably actually be called FeedRepository, takes a dependency on IFeedManager which will implement the actual storage
of data in a database.

# TODO

- Change `ImageUri` to `ImageResource` to better support remote resources, local resources, and data blobs
  - My goal is to retrieve image blobs from remote resources to better reduce data usage as I don't really want to make
a call to remote every time a user views detail lists
    - Not because its pointless when it could be cached, but because some people can be creepy and use it to track users
- Clean up the validation checks. They work but I'm not entirely super pumped about how they work and I may reimplement
the behaviour to be a bit closer to a dictionary implementation than what `System.ComponentModel` provides.
  - It works well enough, but it's clearly designed for a specific way to display errors in the source form
- Probably move the batch methods from the API controller into the `IFeedStorage` so the API isn't making multiple calls
that `IFeedStorage` could handle instead
- rename `IFeedStorage` into something that doesn't imply it actually saves data.