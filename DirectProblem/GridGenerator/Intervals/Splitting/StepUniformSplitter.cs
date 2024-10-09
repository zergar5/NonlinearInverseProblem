using DirectProblem.GridGenerator.Intervals.Core;

namespace DirectProblem.GridGenerator.Intervals.Splitting;

public readonly record struct StepUniformSplitter : IIntervalSplitter
{
    private readonly double _step;

    public StepUniformSplitter(double step)
    {
        _step = step;
    }

    public IEnumerable<double> EnumerateValues(Interval interval)
    {
        var stepNumber = 0;
        var value = interval.Begin + stepNumber * _step;

        while (interval.Has(value))
        {
            yield return value;

            stepNumber++;
            value = interval.Begin + stepNumber * _step;

            if (!(interval.End < value)) continue;

            //if (!(Math.Abs(interval.End + _step - value) < 1e-15))
            //{
            //    throw new ArgumentException("Invalid step or interval");
            //}
        }
    }
}