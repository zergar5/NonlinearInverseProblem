using DirectProblem.Core.GridComponents;

namespace DirectProblem.TwoDimensional.Assembling.Local;

public class LocalBasisFunction
{
    private readonly Func<double, double> _rFunction;
    private readonly Func<double, double> _zFunction;

    public LocalBasisFunction(Func<double, double> rFunction, Func<double, double> zFunction)
    {
        _rFunction = rFunction;
        _zFunction = zFunction;
    }

    public double Calculate(Node2D node)
    {
        return Calculate(node.R, node.Z);
    }

    public double Calculate(double r, double z)
    {
        return _rFunction(r) * _zFunction(z);
    }
}