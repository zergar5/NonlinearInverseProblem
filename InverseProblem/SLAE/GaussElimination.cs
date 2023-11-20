using DirectProblem.Core.Base;
using DirectProblem.SLAE;

namespace InverseProblem.SLAE;

public class GaussElimination
{
    public Vector Solve(Matrix matrix, Vector rightPart)
    {
        try
        {
            ForwardElimination(matrix, rightPart);
            return BackSubstitution(matrix, rightPart);
        }
        catch (Exception)
        {
            throw new DivideByZeroException();
        }
    }

    private void ForwardElimination(Matrix matrix, Vector rightPart)
    {
        for (var i = 0; i < matrix.CountRows - 1; i++)
        {
            var max = Math.Abs(matrix[i, i]);

            var rowNumber = i;

            for (var j = i + 1; j < matrix.CountRows; j++)
            {
                if (max < Math.Abs(matrix[j, i]))
                {
                    max = Math.Abs(matrix[j, i]);
                    rowNumber = j;
                }
            }
            if (rowNumber != i)
            {
                matrix.SwapRows(i, rowNumber);
                (rightPart[i], rightPart[rowNumber]) = (rightPart[rowNumber], rightPart[i]);
            }

            if (Math.Abs(matrix[i, i]) > MethodsConfig.Eps)
            {
                for (var j = i + 1; j < matrix.CountRows; j++)
                {
                    var coefficient = matrix[j, i] / matrix[i, i];
                    matrix[j, i] = 0d;
                    rightPart[j] -= coefficient * rightPart[i];

                    for (var k = i + 1; k < matrix.CountRows; k++)
                    {
                        matrix[j, k] -= coefficient * matrix[i, k];
                    }
                }
            }
            else throw new DivideByZeroException();
        }
    }

    private Vector BackSubstitution(Matrix matrix, Vector rightPart)
    {
        var result = rightPart;

        for (var i = matrix.CountRows - 1; i >= 0; i--)
        {
            var sum = 0d;

            for (var j = i + 1; j < matrix.CountRows; j++)
            {
                sum += matrix[i, j] * result[j];
            }

            if (Math.Abs(matrix[i, i]) > MethodsConfig.Eps) result[i] = (rightPart[i] - sum) / matrix[i, i];
            else throw new DivideByZeroException();
        }

        return result;
    }
}