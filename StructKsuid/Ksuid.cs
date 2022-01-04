using System.Reflection.Metadata;
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
    /// </summary>
    private const uint Epoch = 1400000000;

    
    
    //private readonly byte[] _payload; // timestamp(4) + random bytes(16)
    private readonly uint _timestamp;
    private readonly ulong _a;
    private readonly ulong _b;

    private Ksuid(uint timestamp, ulong bytesA, ulong bytesB)
    {
        //Span<uint> timearray = stackalloc uint[1] { timestamp };
        // if (BitConverter.IsLittleEndian)
        // {
        //     ReverseTimestamp(MemoryMarshal.Cast<uint, byte>(timearray));
        // }
        _timestamp = timestamp;
        _a = bytesA;
        _b = bytesB;
    }

    private Ksuid(ReadOnlySpan<byte> bytes)
    {
        _timestamp = MemoryMarshal.Cast<byte, uint>(bytes)[0];
        // if (BitConverter.IsLittleEndian)
        // {
        //     Span<byte> ts = stackalloc byte[TimestampSize];
        //     var tsuint = MemoryMarshal.Cast<byte, uint>(ts);
        //     tsuint[0] = _timestamp;
        //     ReverseTimestamp(ts);
        //     _timestamp = tsuint[0];
        // }
        var ulongs = MemoryMarshal.Cast<byte, ulong>(bytes[4..]);
        _a = ulongs[0];
        _b = ulongs[1];
    }

    public DateTime TimestampUtc => DateTime.UnixEpoch.AddSeconds(Epoch + _timestamp);


    public byte[] GetBytes()
    {
        var bytes = new byte[20];
        AsBytes(bytes);
        return bytes;
    }

    #region Static Members
    
    public static Ksuid NewKsuid() => FromTimestamp(DateTime.Now);

    public static Ksuid FromTimestamp(DateTime timestamp)
    {
        var time = (uint)(((DateTimeOffset)timestamp).ToUnixTimeSeconds() - Epoch);
        
        return FromTimestamp(time);
    }

    public static Ksuid FromTimestamp(uint timestamp)
    {
        Span<byte> payload = stackalloc byte[PayloadSize + 4];
        InternalRandom.NextBytes(timestamp, payload[TimestampSize..]);
        
        // prepend timestamp
        var timestampSpan = MemoryMarshal.Cast<byte, uint>(payload);
        timestampSpan[0] = timestamp;
        // if (BitConverter.IsLittleEndian)
        //     ReverseTimestamp(payload);
        return new Ksuid(payload);
    }

    public static Ksuid Parse(string ksuidText)
    {
        if (ksuidText?.Length != 27)
            throw new ArgumentException("string must be not null and of length 27");

        Base62Encoding.FromBase62(ksuidText, out var timestamp, out var a, out var b);
        
        return new Ksuid(timestamp, a, b);
    }

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

    //private static void ReverseTimestamp(Span<byte> timestamp) => timestamp[..TimestampSize].Reverse();
    
    private void AsBytes(Span<byte> current)
    {
        MemoryMarshal.Cast<byte, uint>(current)[0] = _timestamp;
        // if (BitConverter.IsLittleEndian)
        //     ReverseTimestamp(current);

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
        total += _b.CompareTo(other._b);
        total += _a.CompareTo(other._a);
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