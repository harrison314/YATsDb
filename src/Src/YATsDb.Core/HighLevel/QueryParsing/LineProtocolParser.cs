using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core.HighLevel.QueryParsing;

public class LineProtocolParser
{
    private const int MaxValueCount = 16;
    private readonly TimeProvider timeProvider;

    public LineProtocolParser(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
    }

    public List<HighLevelInputDataPoint> Parse(string lines)
    {
        Span<Range> regions = stackalloc Range[3];
        Span<Range> frs = stackalloc Range[3];
        Span<Range> numbers = stackalloc Range[16];

        List<HighLevelInputDataPoint> result = new List<HighLevelInputDataPoint>(5);
        int lineNumber = 0;

        foreach (ReadOnlySpan<char> line in lines.AsSpan().EnumerateLines())
        {
            if (line.IsEmpty)
            {
                lineNumber++;
                continue;
            }

            string measurementName;
            string? tag;
            double[] values;
            long unixTimeInMs;

            int regionCount = line.Split(regions, ' ', StringSplitOptions.RemoveEmptyEntries);
            if (regionCount != 2 && regionCount != 3)
            {
                throw new YatsdbSyntaxException($"Invalid region count in line {lineNumber}.");
            }

            ReadOnlySpan<char> fr = line[regions[0]];
            int frCount = fr.Split(frs, ',');
            if (frCount == 1)
            {
                measurementName = fr[frs[0]].ToString();
                tag = null;
            }
            else if (frCount == 2)
            {
                measurementName = fr[frs[0]].ToString();
                tag = fr[frs[1]].ToString();
            }
            else
            {
                throw new YatsdbSyntaxException($"Invalid count of characters ',' in first region on line {lineNumber}.");
            }

            ReadOnlySpan<char> numberRegion = line[regions[1]];
            int numberCount = numberRegion.Split(numbers, ',');
            if (numberCount > MaxValueCount)
            {
                throw new YatsdbSyntaxException($"The number of values ​​in the second section is too high on row {lineNumber}. The maximum line of values ​​is {MaxValueCount}.");
            }

            values = new double[numberCount];
            for (int i = 0; i < values.Length; i++)
            {
                ReadOnlySpan<char> potentialNumber = numberRegion[numbers[i]];
                if (potentialNumber.SequenceEqual("NULL")
                    || potentialNumber.SequenceEqual("null"))
                {
                    values[i] = double.NaN;
                }
                else
                {
                    if (double.TryParse(potentialNumber, System.Globalization.CultureInfo.InvariantCulture, out double cv))
                    {
                        values[i] = cv;
                    }
                    else
                    {
                        throw new YatsdbSyntaxException($"It is not possible to parse the {i}-th value ('{potentialNumber}') on line {lineNumber}.");
                    }
                }
            }

            if (regionCount == 3)
            {
                if (!long.TryParse(line[regions[2]], System.Globalization.CultureInfo.InvariantCulture, out unixTimeInMs))
                {
                    throw new YatsdbSyntaxException($"Line {lineNumber} does not contain a valid timestamp.");
                }
            }
            else
            {
                unixTimeInMs = this.timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
            }

            result.Add(new HighLevelInputDataPoint(measurementName.AsMemory(),
                unixTimeInMs,
                values,
                tag.AsMemory()));

            lineNumber++;
        }

        return result;
    }
}
