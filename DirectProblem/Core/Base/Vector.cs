namespace DirectProblem.Core.Base;

public class Vector
{
    private readonly double[] _values;

    public Vector()
    {
        _values = Array.Empty<double>();
    }

    public Vector(int size)
    {
        _values = new double[size];
    }

    public Vector(double[] values)
    {
        _values = values;
    }

    public double this[int index]
    {
        get => _values[index];
        set => _values[index] = value;
    }

    public int Count => _values.Length;
    public double Norm => Math.Sqrt(ScalarProduct(this, this));

    public static double ScalarProduct(Vector vector1, Vector vector2)
    {
        var result = 0d;
        for (var i = 0; i < vector1.Count; i++)
        {
            result += vector1[i] * vector2[i];
        }
        return result;
    }

    public double ScalarProduct(Vector vector)
    {
        return ScalarProduct(this, vector);
    }

    public static Vector Sum(Vector vector1, Vector vector2, Vector? result = null)
    {
        result ??= new Vector(vector1.Count);

        if (vector1.Count != vector2.Count)
            throw new ArgumentOutOfRangeException(
            $"{nameof(vector1)} and {nameof(vector2)} must have same size");

        for (var i = 0; i < vector1.Count; i++)
        {
            result[i] = vector1[i] + vector2[i];
        }

        return result;
    }

    public static Vector Subtract(Vector vector1, Vector vector2, Vector? result = null)
    {
        result ??= new Vector(vector1.Count);

        if (vector1.Count != vector2.Count)
            throw new ArgumentOutOfRangeException(
            $"{nameof(vector1)} and {nameof(vector2)} must have same size");

        for (var i = 0; i < vector1.Count; i++)
        {
            result[i] = vector1[i] - vector2[i];
        }

        return result;
    }

    public static Vector Multiply(double number, Vector vector, Vector? result = null)
    {
        result ??= new Vector(vector.Count);

        for (var i = 0; i < vector.Count; i++)
        {
            result[i] = vector[i] * number;
        }

        return result;
    }

    public void Fill(double value)
    {
        Array.Fill(_values, value);
    }

    public void Clear()
    {
        Array.Clear(_values);
    }

    public Vector Clone()
    {
        var clone = (double[])_values.Clone();

        return new Vector(clone);
    }

    public Vector Copy(Vector vector)
    {
        if (Count != vector.Count)
            throw new ArgumentOutOfRangeException(
                $"{nameof(_values)}  and  {nameof(vector)} must have same size");

        Array.Copy(_values, vector._values, Count);

        return vector;
    }

    public IEnumerator<double> GetEnumerator() => ((IEnumerable<double>)_values).GetEnumerator();
}