using DirectProblem.Core.Base;

namespace DirectProblem.Core.Local;

public class LocalMatrix
{
    public int[] Indexes { get; }
    private readonly Matrix _matrix;

    public LocalMatrix(int[] indexes, Matrix matrix)
    {
        _matrix = matrix;
        Indexes = indexes;
    }

    public double this[int i, int j]
    {
        get => _matrix[i, j];
        set => _matrix[i, j] = value;
    }
}