# About

This project handles storing data and is an implementation of `IFeedManager` using Sqlite.

All entities _must_ be marked as internal to prevent exposing them to other projects.

The implementation for `IFeedManager` is responsible for turning all internal entities into a detached DTO for use elsewhere. 

# TODO

- Change `ImageUri` to `ImageResource` to better support remote resources, local resources, and data blobs, most likely to
a BLOB type that will basically just be deserialised into a string. More than likely the use of remote content links will go away
and only data blobs will be stored.
    - My goal is to retrieve image blobs from remote resources to better reduce data usage as I don't really want to make
      a call to remote every time a user views detail lists
        - Not because its pointless when it could be cached, but because some people can be creepy and use it to track users
