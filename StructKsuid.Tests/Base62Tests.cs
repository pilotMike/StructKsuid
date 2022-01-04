using System;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;

namespace StructKsuid.Tests;

[TestFixture]
public class Base62Tests
{
    [Test]
    public void EncodedBytesMatch()
    {
        var src = new byte[20];
        Array.Fill(src, (byte)255);

        Span<byte> mine = new byte[27];
        Base62Encoding.ToBase62(src, mine);

        var reference = ToBase62(src);
        
        CollectionAssert.AreEqual(reference, mine.ToArray());
    }

    [Test]
    public void ConversionToLookupBytesIsCorrect()
    {
        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        foreach (var c in chars)
        {
            var index = chars.IndexOf(c);
            var encoded = Base62Encoding.ConvertToBase62Value(c);
            Assert.AreEqual(index, encoded);
        }
    }
    
    [Test]
    public void EncodeAndDecodeBase62()
    {
        var text = "22etmK1wUzazEXIFLmDVAWAYKsd";
        
        Base62Encoding.FromBase62(text, out var timestamp, out var a, out var b);
    }

    [Test]
    public void Reference()
    {
        //byte[] bytes = Encoding.UTF8.GetBytes("2eomijUdQ67XoJdesmjj8sdVkb");
        byte[] bytes = Encoding.UTF8.GetBytes("22etmK1wUzazEXIFLmDVAWAYKsd");
        Span<int> intValues = MemoryMarshal.Cast<byte, int>(bytes);
        var res = ReferenceBase62.Reworked(intValues);
        var original = ReferenceBase62.BaseConvert(intValues);
        
        CollectionAssert.AreEqual(original, res);
    }
    
    
    public static string ToBase62(byte[] src)
    {
        var converted = FastEncodeKsuidToBase62(src);
        var encode62Chars = Base62Characters;
        Span<char> buffer = stackalloc char[27];
        for (int i= converted.Length-1;  i>=0; i--)
        {
            buffer[i] = encode62Chars[converted[i]];
        }
        return buffer.ToString();
    }
    
    public static readonly char[] Base62Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();

    
    private static byte[] FastEncodeKsuidToBase62(byte[] src)
    {
        var dest = new byte[27];

        // To avoid bound checking in the subsequent statements.
        _ = src[19];
        var parts = new uint[5]
        {
            ((uint)src[0]) << 24 | ((uint)src[1]) << 16 | ((uint)src[2]) << 8 | src[3],
            ((uint)src[4]) << 24 | ((uint)src[5]) << 16 | ((uint)src[6]) << 8 | src[7],
            ((uint)src[8]) << 24 | ((uint)src[9]) << 16 | ((uint)src[10]) << 8 | src[11],
            ((uint)src[12]) << 24 | ((uint)src[13]) << 16 | ((uint)src[14]) << 8 | src[15],
            ((uint)src[16]) << 24 | ((uint)src[17]) << 16 | ((uint)src[18]) << 8 | src[19],
        };
        var destLength = dest.Length;
        Span<uint> quotient = stackalloc uint[5];
        while (parts.Length > 0)
        {
            // reusing the quotient array
            quotient.Clear();
            ulong remainder = 0;
            int counter = 0;
            foreach (var part in parts)
            {
                ulong accumulator = part + remainder * uint.MaxValue;
                var digit = accumulator / 62;
                remainder = accumulator % 62;
                if (counter != 0 || digit != 0)
                {
                    quotient[counter] = (uint)digit;
                    counter++;
                }
            }
            destLength--;
            dest[destLength] = (byte)remainder;
            parts = quotient.Slice(0, counter).ToArray();
        }
        return dest;
    }
}