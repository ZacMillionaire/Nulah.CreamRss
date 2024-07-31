# About

This project provides all types and interfaces used and provided by Core.

For anyone wishing to create their own implementations, this is the only project you'll need to ensure interoperability.

Why you'd do this I don't know, but the important thing is you can build whatever replacement implementation as you want
built off of this project, and theoretically everything should behave as expected.

## IFeedManager

Responsible for any data-related actions, generally concerning the storage within a database or similar. Some entity validation
may occur here to ensure data conforms to whichever storage provider is being built upon eg, fields must have a value or be a length etc
before being saved to the database.

## IFeedStorage

This is your repository, and essentially serves as a wrapper around `IFeedManager`, but enables validating incoming data, or any sanitisation
you'd wish to perform before making any potentially expensive database calls or updates.

## IFeedReader

Handles the retrieval and extraction of rss data. Should be able to handle remote locations and local files, and should fail
safe, returning as much information as possible, and only return null if the resource could not be retrieved or was an invalid format.

This may not be attainable depending on your implementation, however you should stive to do so as often as possible.