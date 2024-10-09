using DirectProblem.Core.GridComponents;

namespace DirectProblem.Core;

public class PointsCollection : IPointsCollection<Node2D>
{
    public int Length => _rAxis.Length * _zAxis.Length;
    public int RLength => _rAxis.Length;
    public int ZLength => _zAxis.Length;
    public Node2D this[int index]
    {
        get
        {
            var row = index / _rAxis.Length;
            var column = index % _rAxis.Length;
            var r = _rAxis[column];
            var z = _zAxis[row];

            return new Node2D(r, z);
        }
    }

    private readonly double[] _rAxis;
    private readonly double[] _zAxis;

    public PointsCollection(double[] rAxis, double[] zAxis)
    {
        _rAxis = rAxis;
        _zAxis = zAxis;
    }
}