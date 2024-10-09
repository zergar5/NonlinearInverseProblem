namespace DirectProblem.Core.GridComponents;

public class Area
{
    public const double Delta = 1e-3;
    public int MaterialId { get; private set; }
    public Node2D LowerLeftCorner { get; }
    public Node2D UpperRightCorner { get; }

    public Area(int materialId, Node2D lowerLeftCorner, Node2D upperRightCorner)
    {
        MaterialId = materialId;
        LowerLeftCorner = lowerLeftCorner;
        UpperRightCorner = upperRightCorner;
    }

    public bool Has(Node2D elementLowerLeftCorner, Node2D elementUpperRightCorner)
    {
        return (elementLowerLeftCorner.R > LowerLeftCorner.R ||
                Math.Abs(elementLowerLeftCorner.R - LowerLeftCorner.R) < Delta) &&
               (elementLowerLeftCorner.Z > LowerLeftCorner.Z ||
                Math.Abs(elementLowerLeftCorner.Z - LowerLeftCorner.Z) < Delta) &&
               (elementUpperRightCorner.R < UpperRightCorner.R ||
                Math.Abs(elementUpperRightCorner.R - UpperRightCorner.R) < Delta) &&
               (elementUpperRightCorner.Z < UpperRightCorner.Z ||
                Math.Abs(elementUpperRightCorner.Z - UpperRightCorner.Z) < Delta);
    }
}