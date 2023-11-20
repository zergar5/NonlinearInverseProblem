using DirectProblem.Core.GridComponents;

namespace DirectProblem.FEM;

public class CourseHolder
{
    public static void GetInfo(int iteration, double residual)
    {
        Console.Write($"Iteration: {iteration}, residual: {residual:E14}                                   \r");
    }

    public static void WriteSolution(Node2D point, double value)
    {
        Console.WriteLine($"({point.R},{point.Z}) {value:E14}");
    }

    public static void WriteSolution(Node2D firstPoint, Node2D secondPoint, double value)
    {
        Console.WriteLine($"({firstPoint.R},{firstPoint.Z}) ({secondPoint.R},{secondPoint.Z})  {value:E14}");
    }

    public static void WriteAreaInfo()
    {
        Console.WriteLine("Point not in area or time interval");
    }
}