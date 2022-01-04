using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace StructKsuid.Tests;

[TestFixture]
public class StructKsuidTests
{
    
    
    [Test]
    public void CanConvertAndDeconvert()
    {
        var s = Ksuid.NewKsuid().ToString();
        var ksuid = StructKsuid.Ksuid.Parse(s);
        var toString = ksuid.ToString();
        Assert.AreEqual(s, toString);
    }

    [Test]
    public void MaxValue_IsCorrect()
    {
        var expected = "aWgEOyalkvmob9mwZ5hAphaSmxr";
        var maxValue = Ksuid.MaxValue.ToString();
        Assert.AreEqual(expected, maxValue);
    }

    [Test]
    public void MinValue_IsCorrect()
    {
        var expected = "000000000000000000000000000";
        var minValue = Ksuid.MinValue.ToString();
        Assert.AreEqual(expected, minValue);
    }

    [Test]
    public void NewKsuid_ReturnsKsuid_WithCurrentTimestampComponent()
    {
        var x = Ksuid.NewKsuid().TimestampUtc;
        var now = DateTime.UtcNow;
        var nowTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
        var xTime = new DateTime(x.Year, x.Month, x.Day, x.Hour, x.Minute, x.Second);
        
        Assert.AreEqual(nowTime, xTime);
    }

    [Test]
    public void NewKsuid_ReturnsSortedIds_WhenTheTimestampsAreTheSame()
    {
        var a = Ksuid.NewKsuid();
        var b = Ksuid.NewKsuid();
        Assert.Greater(0, a.CompareTo(b));

        var ids1 = Enumerable.Range(0, 10).Select(_ => Ksuid.NewKsuid()).ToArray();
        var sorted = ids1.ToArray();
        Array.Sort(sorted, Comparer<Ksuid>.Default);
        
        CollectionAssert.AreEqual(ids1, sorted);
    }

    // this test is wrong because right now b/c string sorting ignores casing. this cost me a few hours
    // [Test]
    // public void NewKsuid_ReturnsLexicallySortableIds_BasedOnGenerationSequence()
    // {
    //     for (int i = 0; i < 20; i++)
    //     {
    //         var ids1 = Enumerable.Range(0, 10).Select(_ => Ksuid.NewKsuid().ToString()).ToArray();
    //         var sorted = ids1.OrderBy(x => x).ToArray();
    //         
    //         TestContext.WriteLine(string.Join("\n",ids1.Select(x => 
    //             string.Join(";",  Ksuid.Parse(x).GetBytes().Select(b => b.ToString("000")).ToArray()) + " - " + x )));
    //         TestContext.WriteLine(string.Join("\n", sorted.Select(x => 
    //             string.Join(";", Ksuid.Parse(x).GetBytes().Select(b => b.ToString("000")).ToArray()) + " - " + x )));
    //         TestContext.WriteLine("");
    //         for (int j = 0; j < ids1.Length; j++)
    //         {
    //             var a = ids1[j];
    //             var b = sorted[j];
    //
    //             var payloadA = Ksuid.Parse(a);
    //             var payloadB = Ksuid.Parse(b);
    //             Assert.AreEqual(a, b, "payloadA: {0} , payloadB: {1}", 
    //                 string.Join("",payloadA.GetBytes().Select(x =>x.ToString("000")).ToArray()), 
    //                 string.Join("", payloadB.GetBytes().Select(x => x.ToString("000")).ToArray()));
    //         }
    //     }
    // }

    [Test]
    public void TryParse_ForInvalidText_ReturnsFalse()
    {
        // https://github.com/segmentio/ksuid/issues/25 - from the go implementation
        var res = Ksuid.TryParse("aaaaaaaaaaaaaaaaaaaaaaaaaaa", out var k);
        Assert.IsFalse(res);
    }

    // [TestCase(null)]
    // [TestCase(10)]
    // [TestCase(21)]
    // public void Parse_ThrowsOnInvalidSizedArray(int? length)
    // {
    //     byte[]? arr = length != null ? new byte[length.Value] : null;
    //
    //     Assert.Throws<ArgumentException>(() => Ksuid.Parse(arr));
    // }
    
    // [Test]
    // public void Parse_ReturnsCorrectBytePayload()
    // {
    //     const string text = "22hFVr9JAh8wlknAlKEzwpW1CXw";
    //     byte[] expectedBytes = new byte[]
    //     {
    //         14,
    //         82,
    //         105,
    //         147,
    //         207,
    //         205,
    //         13,
    //         91,
    //         204,
    //         227,
    //         100,
    //         125,
    //         150,
    //         101,
    //         130,
    //         56,
    //         170,
    //         122,
    //         80,
    //         106
    //     };
    //
    //     var outputBytes = StructKsuid.Ksuid.Parse(text).GetBytes();
    //     CollectionAssert.AreEqual(expectedBytes, outputBytes);
    // }
    
}