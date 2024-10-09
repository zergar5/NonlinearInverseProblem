namespace DirectProblem.Core;

public interface IPointsCollection<out TPoint>
{
    public int Length { get; }
    public TPoint this[int index] { get; }
    public int RLength { get; }
    public int ZLength { get; }
}