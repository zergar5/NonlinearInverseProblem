using DirectProblem.Core.Base;

namespace DirectProblem.TwoDimensional.Assembling.MatrixTemplates;

public class MassMatrixTemplate
{
    public static Matrix MassMatrix => new(
        new[,]
        {
            { 2d, 1d },
            { 1d, 2d }
        });

    public static Matrix MassRMatrix => new(
        new[,]
        {
            { 1d, 1d },
            { 1d, 3d }
        });
}