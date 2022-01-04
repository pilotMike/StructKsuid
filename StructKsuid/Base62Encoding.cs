using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

[assembly:InternalsVisibleTo("StructKsuid.Tests")]

namespace StructKsuid;

internal static class Base62Encoding
{
    const ulong uintMaxValue = uint.MaxValue;
    private const uint BaseValue = 62;
    private const int offsetUpperCase = 10;
    private const int offsetLowerCase = 36;

    private static readonly byte[] base62Characters =
        Encoding.UTF8.GetBytes("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");

    private static readonly byte[] zeroString = new byte[27];

    public static void FromBase62(string text, out uint timestamp, out ulong a, out ulong b)
    {
        Span<byte> output = stackalloc byte[20];
        FromBase62(text, output);
        timestamp = MemoryMarshal.Cast<byte, uint>(output)[0];
        var ulongs = MemoryMarshal.Cast<byte, ulong>(output[4..]);
        a = ulongs[0];
        b = ulongs[1];
    }

    internal static byte ConvertToBase62Value(char digit) => (byte)(digit switch
    {
        >= '0' and <= '9' => digit - '0',
        >= 'A' and <= 'Z' => offsetUpperCase + digit - 'A',
        _ => offsetLowerCase + digit - 'a'
    });
    
    
    /// <summary>
    /// This function decodes the base 62 representation of the src KSUID to the
    /// binary form into dst.
    ///
    /// In order to support a couple of optimizations the function assumes that src
    /// is 27 bytes long and dst is 20 bytes long.
    ///
    /// Any unused bytes in dst will be set to zero.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="output"></param>
    private static void FromBase62(string text, Span<byte> output)
    {
        var txt = text.AsSpan();
        _ = txt[26]; // doing this can help the jitter eliminate bounds checks

        Span<uint> parts = stackalloc uint[27]
        {
            ConvertToBase62Value(txt[0]),
            ConvertToBase62Value(txt[1]),
            ConvertToBase62Value(txt[2]),
            ConvertToBase62Value(txt[3]),
            ConvertToBase62Value(txt[4]),
            ConvertToBase62Value(txt[5]),
            ConvertToBase62Value(txt[6]),
            ConvertToBase62Value(txt[7]),
            ConvertToBase62Value(txt[8]),
            ConvertToBase62Value(txt[9]),
            ConvertToBase62Value(txt[10]),
            ConvertToBase62Value(txt[11]),
            ConvertToBase62Value(txt[12]),
            ConvertToBase62Value(txt[13]),
            ConvertToBase62Value(txt[14]),
            ConvertToBase62Value(txt[15]),
            ConvertToBase62Value(txt[16]),
            ConvertToBase62Value(txt[17]),
            ConvertToBase62Value(txt[18]),
            ConvertToBase62Value(txt[19]),
            ConvertToBase62Value(txt[20]),
            ConvertToBase62Value(txt[21]),
            ConvertToBase62Value(txt[22]),
            ConvertToBase62Value(txt[23]),
            ConvertToBase62Value(txt[24]),
            ConvertToBase62Value(txt[25]),
            ConvertToBase62Value(txt[26])
        };
        
        var destLength = output.Length;
        Span<uint> quotient = stackalloc uint[27];

        while (parts.Length > 0)
        {
            quotient.Clear();
            ulong remainder = 0;
            int counter = 0;
            foreach (var part in parts)
            {
                ulong accumulator = part + remainder * BaseValue;
                var digit = accumulator / uintMaxValue;
                remainder = (accumulator % uintMaxValue);
                if (counter != 0 || digit != 0)
                {
                    quotient[counter] = (uint)digit;
                    counter++;
                }
            }

            var slice = output.Slice(destLength - 4);
            MemoryMarshal.Cast<byte, uint>(slice)[0] = (uint)remainder;
            destLength -= 4;

            quotient.Slice(0, counter).CopyTo(parts);
            parts = parts.Slice(0, counter);
        }
    }

    public static void ToBase62(ReadOnlySpan<byte> source, Span<byte> dest)
    {
        // source = 20 bytes, dest = 27bytes
        
        // map the internal bytes to the base62 indexes then do the lookup

        FastEncodeToBase62(source, dest);
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = base62Characters[dest[i]];
        }
    }

    private static void FastEncodeToBase62(ReadOnlySpan<byte> ksuid, Span<byte> dest)
    {
        // assumes the input array is 20 and the output is 27 in length
        
        Span<uint> parts = stackalloc uint[5];
        MemoryMarshal.Cast<byte, uint>(ksuid).CopyTo(parts);

        var destLength = dest.Length;
        Span<uint> quotient = stackalloc uint[5];

        while (parts.Length > 0)
        {
            quotient.Clear();
            ulong remainder = 0;
            int counter = 0;

            foreach (var part in parts)
            {
                ulong accumulator = part + remainder * uintMaxValue;
                var digit = accumulator / BaseValue;
                remainder = accumulator % BaseValue;
                if (counter != 0 || digit != 0)
                {
                    quotient[counter] = (uint)digit;
                    counter++;
                }
            }

            destLength--;
            dest[destLength] = (byte)remainder;
            quotient.Slice(0, counter).CopyTo(parts);
            parts = parts.Slice(0, counter);
        }
        
        // Add padding at the head of the destination buffer for all bytes that were
        // not set.
        zeroString[..destLength].CopyTo(dest);
    }
}