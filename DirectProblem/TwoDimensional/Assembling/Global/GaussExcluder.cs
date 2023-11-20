using DirectProblem.Core.Boundary;
using DirectProblem.Core.Global;
using DirectProblem.FEM.Assembling.Global;

namespace DirectProblem.TwoDimensional.Assembling.Global;

public class GaussExcluder : IGaussExcluder<SymmetricSparseMatrix>
{
    public void Exclude(Equation<SymmetricSparseMatrix> equation, FirstConditionValue condition)
    {
        for (var i = 0; i < condition.Values.Count; i++)
        {
            var row = condition.Values.Indexes[i];
            equation.RightPart[row] = condition.Values[i];
            equation.Matrix[row, row] = 1d;

            foreach (var columnIndex in equation.Matrix[row])
            {
                equation.RightPart[columnIndex] -= equation.Matrix[row, columnIndex] * condition.Values[i];
                equation.Matrix[row, columnIndex] = 0d;
            }

            var column = row;

            for (row = column + 1; row < equation.Matrix.Count; row++)
            {
                if (!equation.Matrix[row].Contains(column)) continue;

                equation.RightPart[row] -= equation.Matrix[row, column] * condition.Values[i];
                equation.Matrix[row, column] = 0d;
            }
        }
    }
}