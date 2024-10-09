using DirectProblem.GridGenerator.Intervals.Core;

namespace DirectProblem.GridGenerator.Intervals.Splitting;

public interface IIntervalSplitter
{
    public IEnumerable<double> EnumerateValues(Interval interval);
}