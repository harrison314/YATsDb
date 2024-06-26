using FsCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core.Tests.FsCheckInfrastructure;

public class DateTimeOffsetArb
{
    public static Arbitrary<DateTimeOffset> DateTimeOffsetGen()
    {
        return (from e1 in Gen.Choose(0, int.MaxValue)
                select DateTimeOffset.FromUnixTimeSeconds(e1))
         .ToArbitrary();
    }

    public static Arbitrary<NonZeroNotEmptyString> ReallyNoNullsAnywhere()
    {
        return Arb.Default.String()
            .Filter(s => s != null && !s.Contains("\0") && s.Length > 0)
            .Convert(s => new NonZeroNotEmptyString(s), ans => ans.Value);
    }
}
