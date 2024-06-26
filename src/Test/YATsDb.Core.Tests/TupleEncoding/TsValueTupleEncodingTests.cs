using FsCheck.Xunit;
using System.Text;
using YATsDb.Core.Tests.FsCheckInfrastructure;
using YATsDb.Core.TupleEncoding;

namespace YATsDb.Core.Tests.TupleEncoding;

public class TsValueTupleEncodingTests
{
    [Theory]
    [InlineData(1, "")]
    [InlineData(2, "")]
    [InlineData(20, "")]
    [InlineData(153, "")]
    [InlineData(1, "tag/my/tag")]
    [InlineData(23, "tag/my/tag")]
    [InlineData(100, "tag/my/tagad/asadvsdvsgdgsddsgda")]
    public void TsValueTupleEncoding_Encode(int valuesCount, string tag)
    {
        double[] values = Enumerable.Range(0, valuesCount).Select(_ => Random.Shared.NextDouble()).ToArray();
        _ = TsValueTupleEncoding.Encode(values, tag.AsSpan());
    }

    [Property(StartSize = 200, Arbitrary = new Type[] { typeof(DateTimeOffsetArb) })]
    public void TsValueTupleEncoding_DecodeWithTag(double value1, double value2, double value3, NonZeroNotEmptyString tag)
    {
        double[] values = new double[] { value1, value2, double.NaN, value3 };
        int[] indexes = new int[] { 0, 1, 2, 3, 4, 41 };
        double[] extractValues = new double[6];

        byte[] value = TsValueTupleEncoding.Encode(values, tag.Value);

        TsValueTupleEncoding.DecodeValues(value, indexes, extractValues);

        this.AssertDoubleEqual(extractValues[0], value1);
        this.AssertDoubleEqual(extractValues[1], value2);
        this.AssertDoubleEqual(extractValues[2], double.NaN);
        this.AssertDoubleEqual(extractValues[3], value3);
        this.AssertDoubleEqual(extractValues[4], double.NaN);
        this.AssertDoubleEqual(extractValues[5], double.NaN);
    }

    [Property]
    public void TsValueTupleEncoding_Decode(double value1, double value2, double value3)
    {
        double[] values = new double[] { value1, value2, double.NaN, value3 };
        int[] indexes = new int[] { 0, 1, 2, 3, 4, 41 };
        double[] extractValues = new double[6];

        byte[] value = TsValueTupleEncoding.Encode(values, ReadOnlySpan<char>.Empty);

        TsValueTupleEncoding.DecodeValues(value, indexes, extractValues);

        this.AssertDoubleEqual(extractValues[0], value1);
        this.AssertDoubleEqual(extractValues[1], value2);
        this.AssertDoubleEqual(extractValues[2], double.NaN);
        this.AssertDoubleEqual(extractValues[3], value3);
        this.AssertDoubleEqual(extractValues[4], double.NaN);
        this.AssertDoubleEqual(extractValues[5], double.NaN);
    }

    [Theory]
    [InlineData(1, "")]
    [InlineData(2, "")]
    [InlineData(20, "")]
    [InlineData(153, "")]
    [InlineData(1, "tag/my/tag")]
    [InlineData(23, "tag/my/tag")]
    [InlineData(100, "tag/my/tagad/asadvsdvsgdgsddsgda")]
    public void TsValueTupleEncoding_DecodeTagdata(int valuesCount, string tag)
    {
        double[] values = Enumerable.Range(0, valuesCount).Select(_ => Random.Shared.NextDouble()).ToArray();
        byte[] value = TsValueTupleEncoding.Encode(values, tag.AsSpan());

        ReadOnlyMemory<byte> utf8Tag = TsValueTupleEncoding.DecodeTagData(value);

        string newTag = Encoding.UTF8.GetString(utf8Tag.Span);
        Assert.Equal(newTag, tag);
    }

    private void AssertDoubleEqual(double value1, double value2)
    {
        if (double.IsNaN(value1) && double.IsNaN(value2))
        {
            return;
        }

        if (double.IsPositiveInfinity(value1) && double.IsPositiveInfinity(value2))
        {
            return;
        }

        if (double.IsNegativeInfinity(value1) && double.IsNegativeInfinity(value2))
        {
            return;
        }

        Assert.Equal(value1, value2);
    }
}