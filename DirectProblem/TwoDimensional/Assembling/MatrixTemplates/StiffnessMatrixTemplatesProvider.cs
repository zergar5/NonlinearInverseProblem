using DirectProblem.Core.Base;

namespace DirectProblem.TwoDimensional.Assembling.MatrixTemplates;

public class StiffnessMatrixTemplates
{
    public static Matrix StiffnessMatrix => new(
        new[,]
        {
            { 1d, -1d },
            { -1d, 1d }
        });
}