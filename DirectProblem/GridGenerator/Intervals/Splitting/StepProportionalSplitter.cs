using DirectProblem.GridGenerator.Intervals.Core;

namespace DirectProblem.GridGenerator.Intervals.Splitting;

public readonly record struct StepProportionalSplitter : IIntervalSplitter
{
    public double DischargeRatio { get; }

    private readonly double _step;

    public StepProportionalSplitter(double step, double dischargeRatio)
    {
        if (Math.Abs(dischargeRatio - 1d) < 1e-15)
            throw new NotSupportedException();

        _step = step;
        DischargeRatio = dischargeRatio;
    }

    public IEnumerable<double> EnumerateValues(Interval interval)
    {
        var values = new List<double>();
        var stepNumber = 0;
        var value = 0d;

        if (DischargeRatio < 1)
        {
            value = interval.End;
            var dischargeRatio = 1 / DischargeRatio;

            while (interval.Has(value))
            {
                values.Add(value);
                var nextValue = value - _step * Math.Pow(dischargeRatio, stepNumber);

                value = nextValue;
                stepNumber++;

                if (!(interval.Begin > value)) continue;
                values.Add(interval.Begin);
                break;
            }

            values.Reverse();
        }
        else
        {
            value = interval.Begin;
            while (interval.Has(value))
            {
                values.Add(value);
                var nextValue = value + _step * Math.Pow(DischargeRatio, stepNumber);

                value = nextValue;
                stepNumber++;

                if (!(interval.End < value)) continue;
                values.Add(interval.End);
                break;
            }
        }

        foreach (var pointValue in values)
        {
            yield return pointValue;
        }
    }
}