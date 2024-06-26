using YATsDb.Core.TupleEncoding;

namespace YATsDb.Core.Tests.TupleEncoding;

public class TupleEncoderUseCaseTests
{
    [Fact]
    public void TupleEnocder_LongAfterString()
    {
        byte[] data = TupleEncoder.Create(1,
            1u,
            "testData",
            "",
            1569L);

        bool result = TupleEncoder.TryDeconstruct(data,
                     1,
                     out uint _,
                     out string _,
                     out string description,
                     out long unixTimestampInMs);

        Assert.True(result);
        Assert.Equal(1569L, unixTimestampInMs);
    }
}