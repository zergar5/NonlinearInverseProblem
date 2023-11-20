using DirectProblem.GridGenerator.Intervals.Core;

namespace DirectProblem.GridGenerator.Intervals.Splitting;

public class ProportionalSplitter : IIntervalSplitter
{
    public int Steps { get; }
    public double DischargeRatio { get; }

    private readonly double _lengthCoefficient;

    public ProportionalSplitter(int steps, double dischargeRatio)
    {
        if (Math.Abs(DischargeRatio - 1d) < 1e-16)
            throw new NotSupportedException();

        Steps = steps;
        DischargeRatio = dischargeRatio;
        _lengthCoefficient = (DischargeRatio - 1d) / (Math.Pow(DischargeRatio, Steps) - 1d);
    }

    public IEnumerable<double> EnumerateValues(Interval interval)
    {
        var step = interval.Length * _lengthCoefficient;

        for (var stepNumber = 0; stepNumber <= Steps; stepNumber++)
        {
            var value = interval.Begin + step * (Math.Pow(DischargeRatio, stepNumber) - 1d) / (DischargeRatio - 1d);

            yield return value;
        }
    }
}