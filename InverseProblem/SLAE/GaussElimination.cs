using DirectProblem.Core.Base;
using DirectProblem.Core.Global;
using DirectProblem.SLAE;

namespace InverseProblem.SLAE;

public class GaussElimination
{
    public Vector Solve(Equation<Matrix> equation)
    {
        try
        {
            ForwardElimination(equation);
            return BackSubstitution(equation);
        }
        catch (Exception)
        {
            throw new DivideByZeroException();
        }
    }

    private void ForwardElimination(Equation<Matrix> equation)
    {
        for (var i = 0; i < equation.Matrix.CountRows - 1; i++)
        {
            var max = Math.Abs(equation.Matrix[i, i]);

            var rowNumber = i;

            for (var j = i + 1; j < equation.Matrix.CountRows; j++)
            {
                if (max < Math.Abs(equation.Matrix[j, i]))
                {
                    max = Math.Abs(equation.Matrix[j, i]);
                    rowNumber = j;
                }
            }
            if (rowNumber != i)
            {
                for (var j = 0; j < equation.Matrix.CountColumns; j++)
                {
                    (equation.Matrix[rowNumber, j], equation.Matrix[i, j]) =
                        (equation.Matrix[i, j], equation.Matrix[rowNumber, j]);
                }

                (equation.RightPart[i], equation.RightPart[rowNumber]) =
                    (equation.RightPart[rowNumber], equation.RightPart[i]);
            }

            if (Math.Abs(equation.Matrix[i, i]) > MethodsConfig.EpsDouble)
            {
                for (var j = i + 1; j < equation.Matrix.CountRows; j++)
                {
                    var coefficient = equation.Matrix[j, i] / equation.Matrix[i, i];
                    equation.Matrix[j, i] = 0d;
                    equation.RightPart[j] -= coefficient * equation.RightPart[i];

                    for (var k = i + 1; k < equation.Matrix.CountRows; k++)
                    {
                        equation.Matrix[j, k] -= coefficient * equation.Matrix[i, k];
                    }
                }
            }
            else throw new DivideByZeroException();
        }
    }

    private Vector BackSubstitution(Equation<Matrix> equation)
    {
        for (var i = equation.Matrix.CountRows - 1; i >= 0; i--)
        {
            var sum = 0d;

            for (var j = i + 1; j < equation.Matrix.CountRows; j++)
            {
                sum += equation.Matrix[i, j] * equation.Solution[j];
            }

            if (Math.Abs(equation.Matrix[i, i]) > MethodsConfig.EpsDouble)
                equation.Solution[i] = (equation.RightPart[i] - sum) / equation.Matrix[i, i];
            else throw new DivideByZeroException();
        }

        return equation.Solution;
    }
}