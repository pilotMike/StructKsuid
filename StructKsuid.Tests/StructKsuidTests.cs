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
        var s = Ksuid.NextKsuid().ToString();
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
    public void NextKsuid_ReturnsKsuid_WithCurrentTimestampComponent()
    {
        var x = Ksuid.NextKsuid().TimestampUtc;
        var now = DateTime.UtcNow;
        var nowTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
        var xTime = new DateTime(x.Year, x.Month, x.Day, x.Hour, x.Minute, x.Second);
        
        Assert.AreEqual(nowTime, xTime);
    }

    [Test]
    public void NextKsuid_ReturnsSortedIds_WhenTheTimestampsAreTheSame()
    {
        var a = Ksuid.NextKsuid();
        var b = Ksuid.NextKsuid();
        Assert.Greater(0, a.CompareTo(b));

        var ids1 = Enumerable.Range(0, 10).Select(_ => Ksuid.NextKsuid()).ToArray();
        var sorted = ids1.ToArray();
        Array.Sort(sorted, Comparer<Ksuid>.Default);
        
        CollectionAssert.AreEqual(ids1, sorted);
    }

    [Test]
    public void NextKsuid_ReturnsLexicallySortableIds_BasedOnGenerationSequence()
    {
        for (int i = 0; i < 20; i++)
        {
            var ids1 = Enumerable.Range(0, 10).Select(_ =>  Ksuid.NextKsuid().ToString()).ToArray();
            var sorted = ids1.OrderBy(x => x, StringComparer.Ordinal).ToArray();
            
            // TestContext.WriteLine(string.Join("\n",ids1.Select(x => 
            //     string.Join(";",  Ksuid.Parse(x).GetBytes().Select(b => b.ToString("000")).ToArray()) + " - " + x )));
            // TestContext.WriteLine(string.Join("\n", sorted.Select(x => 
            //     string.Join(";", Ksuid.Parse(x).GetBytes().Select(b => b.ToString("000")).ToArray()) + " - " + x )));
            // TestContext.WriteLine("");
            for (int j = 0; j < ids1.Length; j++)
            {
                var a = ids1[j];
                var b = sorted[j];
    
                var payloadA = Ksuid.Parse(a);
                var payloadB = Ksuid.Parse(b);
                Assert.AreEqual(a, b, "payloadA: {0} , payloadB: {1}", 
                    string.Join("",payloadA.GetBytes().Select(x =>x.ToString("000")).ToArray()), 
                    string.Join("", payloadB.GetBytes().Select(x => x.ToString("000")).ToArray()));
            }
        }
    }

    [Test]
    public void TryParse_ForInvalidText_ReturnsFalse()
    {
        // https://github.com/segmentio/ksuid/issues/25 - from the go implementation
        var res = Ksuid.TryParse("aaaaaaaaaaaaaaaaaaaaaaaaaaa", out var k);
        Assert.IsFalse(res);
    }

    [TestCase(null)]
    [TestCase(10)]
    [TestCase(21)]
    public void Parse_ThrowsOnInvalidSizedArray(int? length)
    {
        byte[]? arr = length != null ? new byte[length.Value] : null;
    
        Assert.Throws<ArgumentException>(() => Ksuid.FromBytes(arr));
    }

    [Test]
    public void GetBytes_FromBytes_ReturnEquivalentKsuid()
    {
        var id = Ksuid.RandomKsuid();
        var bytes = id.GetBytes();
        var second = Ksuid.FromBytes(bytes);
        
        Assert.AreEqual(id, second);
    }

    [Test]
    public void FromTimestamp_ReturnsNewKsuid_WithGivenTimestamp_RoundedToSeconds()
    {
        var ts = DateTime.UtcNow;
        var id = Ksuid.FromTimestamp(ts);
        var idts = id.TimestampUtc;
        ts = new DateTime(ts.Year, ts.Month, ts.Day, ts.Hour, ts.Minute, ts.Second);
        
        Assert.AreEqual(ts, idts);
    }
    
}