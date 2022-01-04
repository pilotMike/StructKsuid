# StructKsuid
An implementation of [the segment.io ksuid](https://github.com/segmentio/ksuid/tree/d24e51dda38d4a3994a500616c71cd36ec385889), a K-sortable unique identifier,
but as a struct and with 0 allocations.

The KSUID is sortable lexically (ToString()) and as an object.

## Why was this made
There are two other dotnet implementations of KSUID that are popular, 
[Ksuid.Net](https://github.com/JoyMoe/Ksuid.Net) and
[DotKsuid](https://github.com/viveks7/DotKsuid). However, I have a use case where I need to 
create a lot of KSUIDs in succession and have those be lexically sortable based on the order
in which they were created. 

The GO implementation from segment.io has a `next` method that takes a ksuid and returns the next one.
I modeled my implementation off of this so that one can call `Ksuid.NextKsuid()` in succession and
each result will be sortable based on the order in which they were created.

I also exposed a `Ksuid.RandomKsuid()` that gives a Ksuid with the timestamp and a random payload.

## Differences with the original and other libs
Besides being a struct and no-alloc, the data is stored as `uint timestamp`, `ulong a`, `ulong b` 
instead of as a timestamp and/or byte array. In doing so, the bytes for the timestamp are no longer
switched for endianness as they are in the other implementations. Thus, I can't guarantee that
bytes made by other libraries would be compatible here.

## Performance
Performance is pretty strong, coming in at about 10x the speed of Ksuid.Net and just a bit slower
DotKsuid. The only allocations made are when calling `ToString()`.
I suspect that part of the reason for DotKsuid coming in slightly faster is that
this implementation has to copy the 3 parts of the payload instead of passing the
reference to the byte array.

Creating a new Ksuid
![](https://github.com/pilotMike/StructKsuid/blob/master/create%20benchmarks.png)

Parsing  
![](https://github.com/pilotMike/StructKsuid/blob/master/parse%20benchmarks.png)

# Usage
I tried to keep the API close to the dotnet Guid API.

`var ksuid = Ksuid.NextKsuid()` // Creates a new Ksuid that is sortable from the last one created

`var ksuid = Ksuid.RandomKsuid()` // Creates an "untracked" ksuid with the current timestamp and a random payload.
Not guaranteed to be sortable from the last, except by timestamp.

`var ksuid = Ksuid.FromTimestamp(DateTime | uint)` // Similar to RandomKsuid, but with a given timestamp

`var ksuid = Ksuid.FromBytes()`

`var bytes = ksuid.GetBytes()`

`var timestampUtc = ksuid.TimestampUtc`

# Cautions
I did not make a serializer for this Ksuid. 

Also be sure that when sorting the strings in dotnet,
that the sorting is case sensitive. I had to add do ...`.OrderBy(string => string, StringComparison.Ordinal)`
to get the text to be sorted correctly, because the default comparer is case-insensitive. 
You can see this in the unit tests.
