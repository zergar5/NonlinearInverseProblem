using DirectProblem.Core.Base;
using DirectProblem.Core.Global;

namespace InverseProblem.SLAE;

public class Regularizer
{
    private readonly GaussElimination _gaussElimination;
    public Matrix BufferMatrix { get; set; }
    public Vector BufferVector { get; set; }
    public Vector ResidualBufferVector { get; set; }

    public Regularizer(GaussElimination gaussElimination)
    {
        _gaussElimination = gaussElimination;
    }

    public double Regularize(Equation<Matrix> equation, Vector trueCurrents)
    {
        var alpha = CalculateAlpha(equation.Matrix);

        alpha = FindPossibleAlpha(equation, alpha, trueCurrents, out var residual);

        alpha = FindBestAlpha(equation, alpha, trueCurrents, residual);

        return alpha;
    }

    private double CalculateAlpha(Matrix matrix)
    {
        var n = matrix.CountRows;
        var alpha = 0d;

        for (var i = 0; i < n; i++)
        {
            alpha += matrix[i, i];
        }

        alpha /= n * 10e14;

        return alpha;
    }

    private void AssembleSLAE(Equation<Matrix> equation, double alpha, Vector trueCurrents)
    {
        Matrix.CreateIdentityMatrix(BufferMatrix);

        Matrix.Sum(equation.Matrix, Matrix.Multiply(alpha, BufferMatrix, BufferMatrix), BufferMatrix);

        Vector.Subtract(
            equation.RightPart, Vector.Multiply(
                alpha, Vector.Subtract(equation.Solution, trueCurrents, BufferVector),
                BufferVector),
            BufferVector);
    }

    private double CalculateResidual(Equation<Matrix> equation, double alpha, Vector trueCurrents)
    {
        Matrix.CreateIdentityMatrix(BufferMatrix);

        Matrix.Sum(equation.Matrix, Matrix.Multiply(alpha, BufferMatrix, BufferMatrix), BufferMatrix);

        Matrix.Multiply(BufferMatrix, BufferVector, ResidualBufferVector);

        Vector.Subtract(
            equation.RightPart, Vector.Multiply(
                alpha, Vector.Subtract(equation.Solution, trueCurrents, BufferVector),
                BufferVector),
            BufferVector);

        return Vector.Subtract(
            BufferVector,
            ResidualBufferVector, BufferVector)
            .Norm;
    }

    private double FindPossibleAlpha(Equation<Matrix> equation, double alpha, Vector trueCurrents, out double residual)
    {
        for (; ; )
        {
            try
            {
                AssembleSLAE(equation, alpha, trueCurrents);

                BufferVector = _gaussElimination.Solve(BufferMatrix, BufferVector);

                residual = CalculateResidual(equation, alpha, trueCurrents);

                break;
            }
            catch { }
            finally
            {
                alpha *= 1.5;
            }
        }

        return alpha;
    }

    private double FindBestAlpha(Equation<Matrix> equation, double alpha, Vector trueCurrents, double residual)
    {
        var ratio = 1d;

        do
        {
            try
            {
                AssembleSLAE(equation, alpha, trueCurrents);

                BufferVector = _gaussElimination.Solve(BufferMatrix, BufferVector);

                var currentResidual = CalculateResidual(equation, alpha, trueCurrents);

                ratio = currentResidual / residual;
            }
            catch { }
            finally
            {
                alpha *= 1.5;
            }
        } while (ratio is < 1.999d or > 3d);

        return alpha / 1.5;
    }
}