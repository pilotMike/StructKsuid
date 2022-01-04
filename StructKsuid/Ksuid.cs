using System.Runtime.InteropServices;
using System.Text;

namespace StructKsuid;

public readonly struct Ksuid : IEquatable<Ksuid>, IComparable<Ksuid>
{
    public static readonly Ksuid MaxValue = new Ksuid(uint.MaxValue, ulong.MaxValue, ulong.MaxValue);
    public static Ksuid MinValue => new Ksuid();
    
    private const int EncodedSize = 27;
    private const int PayloadSize = 16;
    private const int TimestampSize = 4;
    private const int TotalDataSize = 20;
    
    /// <summary>
    /// KSUID's epoch starts more recently so that the 32-bit number space gives a
    /// significantly higher useful lifetime of around 136 years from March 2017.
    /// This number (14e8) was picked to be easy to remember.
    /// This is taken from the segment.io implementation.
    /// </summary>
    private const uint Epoch = 1400000000;

    private readonly uint _timestamp;
    private readonly ulong _a;
    private readonly ulong _b;

    private Ksuid(uint timestamp, ulong bytesA, ulong bytesB)
    {
        _timestamp = timestamp;
        _a = bytesA;
        _b = bytesB;
    }

    private Ksuid(ReadOnlySpan<byte> bytes)
    {
        _timestamp = MemoryMarshal.Cast<byte, uint>(bytes)[0];
        
        var ulongs = MemoryMarshal.Cast<byte, ulong>(bytes[4..]);
        _a = ulongs[0];
        _b = ulongs[1];
    }

    public DateTime TimestampUtc => DateTime.UnixEpoch.AddSeconds(Epoch + _timestamp);

    /// <summary>
    /// Returns the Ksuid data as a byte array.
    /// </summary>
    public byte[] GetBytes()
    {
        var bytes = new byte[20];
        AsBytes(bytes);
        return bytes;
    }

    #region Static Members
    
    /// <summary>
    /// Returns a new Ksuid that increments from the last if the timestamps match.
    /// This locks on the payload generation for tracking, but ensures that any Ksuids
    /// created in sequence are sortable.
    /// </summary>
    public static Ksuid NextKsuid() => FromTimestamp(DateTime.Now);

    /// <summary>
    /// Returns a new Ksuid based off of the current timestamp with a random payload.
    /// Any Ksuids created with this method within the same second are not
    /// going to be sortable based on when they were created.
    /// </summary>
    public static Ksuid RandomKsuid()
    {
        var time = (uint)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - Epoch);
        Span<byte> payload = stackalloc byte[PayloadSize + 4];
        InternalRandom.NextRandomBytes(payload[TimestampSize..]);
        
        // prepend timestamp
        var timestampSpan = MemoryMarshal.Cast<byte, uint>(payload);
        timestampSpan[0] = time;
        
        return new Ksuid(payload);
    }

    /// <summary>
    /// Returns a new Ksuid with the given timestamp and a random payload.
    /// </summary>
    public static Ksuid FromTimestamp(DateTime timestamp)
    {
        var time = (uint)(((DateTimeOffset)timestamp).ToUnixTimeSeconds() - Epoch);
        
        return FromTimestamp(time);
    }

    /// <summary>
    /// Returns a new Ksuid with the given timestamp and a random payload.
    /// </summary>
    public static Ksuid FromTimestamp(uint timestamp)
    {
        Span<byte> payload = stackalloc byte[PayloadSize + 4];
        InternalRandom.NextBytes(timestamp, payload[TimestampSize..]);
        
        // prepend timestamp
        var timestampSpan = MemoryMarshal.Cast<byte, uint>(payload);
        timestampSpan[0] = timestamp;
        
        return new Ksuid(payload);
    }

    /// <summary>
    /// Parses a base62 encoded Ksuid string into an instance.
    /// </summary>
    /// <param name="ksuidText">length 27</param>
    /// <exception cref="ArgumentException">if the string is null or not of length 27</exception>
    /// <exception cref="IndexOutOfRangeException">If the string is invalid, this will be the likely exception</exception>
    public static Ksuid Parse(string ksuidText)
    {
        if (ksuidText?.Length != 27)
            throw new ArgumentException("string must be not null and of length 27");

        Base62Encoding.FromBase62(ksuidText, out var timestamp, out var a, out var b);
        
        return new Ksuid(timestamp, a, b);
    }

    /// <summary>
    /// Parses a base62 encoded Ksuid string into an instance.
    /// </summary>
    /// <param name="ksuidText">length 27</param>
    /// <param name="ksuid">resultant ksuid. default on failure</param>
    /// <returns>bool success</returns>
    public static bool TryParse(string ksuidText, out Ksuid ksuid)
    {
        if (ksuidText?.Length != 27)
        {
            ksuid = new Ksuid();
            return false;
        }

        try
        {
            ksuid = Parse(ksuidText);
            return true;
        }
        catch
        {
            ksuid = new Ksuid();
            return false;
        }
    }

    /// <summary>
    /// Creates a new Ksuid instance based off of the bytes from <see cref="GetBytes"/>
    /// </summary>
    /// <param name="bytes">a span or array of length 20</param>
    /// <exception cref="ArgumentException">input must be of length 20</exception>
    public static Ksuid FromBytes(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != TotalDataSize)
            throw new ArgumentException(nameof(bytes) + "must be of length " + TotalDataSize);
        
        return new Ksuid(bytes);
    }
    
    #endregion
    
    #region Helpers

    private void AsBytes(Span<byte> current)
    {
        MemoryMarshal.Cast<byte, uint>(current)[0] = _timestamp;
        
        var ulongs = MemoryMarshal.Cast<byte, ulong>(current[4..]);
        ulongs[0] = _a;
        ulongs[1] = _b;
    }
    
    #endregion

    #region Comparison and Equality
    public bool Equals(Ksuid other) =>
        _timestamp == other._timestamp &&
        _a == other._a && _b == other._b;

    public override bool Equals(object? obj) => obj is Ksuid other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(_timestamp, _a, _b);

    public int CompareTo(Ksuid other)
    {
        var total = _timestamp.CompareTo(other._timestamp);
        if (total != 0) return total;
        
        total = _a.CompareTo(other._a);
        if (total != 0) return total;
        
        total = _b.CompareTo(other._b);
        return total;
    }
    
    #endregion


    public override string ToString()
    {
        Span<byte> encoded = stackalloc byte[EncodedSize];
        Span<byte> bytes = stackalloc byte[PayloadSize+TimestampSize];
        AsBytes(bytes);
        
        Base62Encoding.ToBase62(bytes, encoded);
        
        return Encoding.UTF8.GetString(encoded);
    }
}