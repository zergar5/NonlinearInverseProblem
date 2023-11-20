namespace DirectProblem.Core.GridComponents;

public class ReceiversLine
{
    public Node2D PointM { get; }
    public Node2D PointN { get; }

    public ReceiversLine(Node2D pointM, Node2D pointN)
    {
        PointM = pointM;
        PointN = pointN;
    }
};