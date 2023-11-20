namespace DirectProblem.Core.GridComponents;

public class Node2D
{
    public double R { get; set; }
    public double Z { get; set; }

    public Node2D(double r, double z)
    {
        R = r;
        Z = z;
    }

    public static double Distance(Node2D node1, Node2D node2)
    {
        return Math.Sqrt(Math.Pow(node2.R - node1.R, 2) + Math.Pow(node2.Z - node1.Z, 2));
    }
}