using System.Collections;

namespace DirectProblem.Core.GridComponents;

public class SourcesLine
{
    public Node2D PointA { get; }
    public Node2D PointB { get; }
    public double Current { get; }

    public SourcesLine(Node2D pointA, Node2D pointB, double current)
    {
        PointA = pointA;
        PointB = pointB;
        Current = current;
    }

    public Node2D this[int index]
    {
        get
        {
            return index switch
            {
                0 => PointA,
                1 => PointB,
                _ => throw new IndexOutOfRangeException()
            };
        }
    }

    public IEnumerator GetEnumerator() => new SourceLineEnumerator(this);
}

class SourceLineEnumerator : IEnumerator
{
    private readonly Node2D _pointA;
    private readonly Node2D _pointB;
    private int _position = -1;

    public SourceLineEnumerator(SourcesLine sourcesLine)
    {
        _pointA = sourcesLine.PointA;
        _pointB = sourcesLine.PointB;
    }

    object IEnumerator.Current => Current;

    public Node2D Current
    {
        get
        {
            return _position switch
            {
                -1 or >= 2 => throw new ArgumentException(),
                0 => _pointA,
                1 => _pointB,
                _ => throw new IndexOutOfRangeException()
            };
        }
    }

    public bool MoveNext()
    {
        if (_position < 2)
        {
            _position++;
            return true;
        }

        return false;
    }

    public void Reset() => _position = -1;
}