using System.Runtime.InteropServices;

namespace StructKsuid;

/// <summary>
/// The random class is not thread safe, but the desired behavior is that each new Ksuid is determined from the last,
/// so the same Random instance is used across all threads. This is the behavior of the GO library.
/// https://github.com/segmentio/ksuid/blob/d24e51dda38d4a3994a500616c71cd36ec385889/ksuid.go#L217
/// </summary>
internal static class InternalRandom
{
    private static readonly Random r = new ();
    private static readonly object _lock = new ();
    private static uint lastTimestamp;
    private static ulong lastPayloadA;
    private static ulong lastPayloadB;
    
    // dot KSUID uses this approach, but I don't think it will work for my use case
    private static readonly Random GlobalRandom = new ();
    private static readonly ThreadLocal<Random> LocalRandom = new (() =>
    {
        lock (GlobalRandom)
        {
            return new Random(GlobalRandom.Next());
        }
    });


    /// <summary>
    /// Creates a new Ksuid based off of two pathways. If the timestamp is the same as the last created Ksuid,
    /// then the previous payload is incremented by 1 and set to the bytes. Otherwise a new Ksuid is created
    /// with a random payload.
    ///
    /// This method is thread-safe, but it locks for its execution so that the payloads can be guaranteed to be sorted
    /// based on their creation sequences if the timestamp matches the last created timestamp.
    /// </summary>
    /// <param name="timestamp">current timestamp that the new ksuid is being created for</param>
    /// <param name="bytes"></param>
    public static void NextBytes(uint timestamp, Span<byte> bytes)
    {
        var payload = MemoryMarshal.Cast<byte, ulong>(bytes);
        lock (_lock)
        {
            if (lastTimestamp == timestamp && lastPayloadB != default)
            {
                // do a 128-bit add that overflows into the timestamp, as done in https://github.com/segmentio/ksuid/blob/d24e51dda38d4a3994a500616c71cd36ec385889/ksuid.go#L325
                unchecked
                {
                    lastPayloadB += 1;
                    if (lastPayloadB == 0UL)
                    {
                        lastPayloadA += 1;
                        if (lastPayloadA == 0L)
                            timestamp++;
                    }
                }
                
                payload[0] = lastPayloadA;
                payload[1] = lastPayloadB;
            }
            else
            {
                r.NextBytes(bytes);
                lastPayloadA = payload[0];
                lastPayloadB = payload[1];
            }

            lastTimestamp = timestamp;
        }
    }

    /// <summary>
    /// Thread-safe. Creates a random payload. Does not track the last item created and cannot guarantee sortability
    /// of the payload.
    /// </summary>
    public static void NextRandomBytes(Span<byte> bytes) => LocalRandom.Value!.NextBytes(bytes);
}