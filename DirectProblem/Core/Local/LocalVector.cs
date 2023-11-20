using DirectProblem.Core.Base;

namespace DirectProblem.Core.Local;

public class LocalVector
{
    public int[] Indexes { get; }
    private readonly Vector _vector;

    public int Count => _vector.Count;

    public LocalVector(int[] indexes, Vector vector)
    {
        Indexes = indexes;
        _vector = vector;
    }

    public double this[int index]
    {
        get => _vector[index];
        set => _vector[index] = value;
    }

    public IEnumerator<double> GetEnumerator() => _vector.GetEnumerator();
}