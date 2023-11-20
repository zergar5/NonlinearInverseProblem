using DirectProblem.Core.GridComponents;
using DirectProblem.TwoDimensional.Assembling.Local;

namespace DirectProblem.Calculus;

public class DerivativeCalculator
{
    private const double Delta = 1.0e-3;

    public double Calculate(LocalBasisFunction localBasisFunction, Node2D point, char variableChar)
    {
        double result;
        if (variableChar == 'r')
        {
            result = localBasisFunction.Calculate(point.R + Delta, point.Z) - localBasisFunction.Calculate(point.R - Delta, point.Z);
        }
        else
        {
            result = localBasisFunction.Calculate(point.R, point.Z + Delta) - localBasisFunction.Calculate(point.R, point.Z - Delta);
        }
        return result / (2.0 * Delta);
    }

    //public double Calculate(Func<Node2D, double> function, Node2D point, char variableChar)
    //{
    //    double result;
    //    if (variableChar == 'r')
    //    {
    //        result = function(point with { R = point.R + Delta }) -
    //                 function(point with { R = point.R - Delta });
    //    }
    //    else
    //    {
    //        result = function(point with { Z = point.Z + Delta }) -
    //                 function(point with { Z = point.Z - Delta });
    //    }
    //    return result / (2.0 * Delta);
    //}

    public double Calculate(Func<double[], double> function, double[] variables, int variableIndex)
    {
        var buffer = variables[variableIndex];

        variables[variableIndex] = buffer + Delta;
        var fLeft = function(variables);

        variables[variableIndex] = buffer - Delta;
        var fRight = function(variables);

        variables[variableIndex] = buffer;

        return (fLeft - fRight) / (2.0 * Delta);
    }
}