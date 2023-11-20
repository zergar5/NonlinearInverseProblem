using DirectProblem.Core;
using DirectProblem.Core.Base;
using DirectProblem.Core.Boundary;
using DirectProblem.Core.GridComponents;
using DirectProblem.Core.Local;

namespace DirectProblem.TwoDimensional.Assembling.Boundary;

public class FirstBoundaryProvider
{
    private readonly Grid<Node2D> _grid;
    private int[][]? _indexes;
    private Vector[]? _values;

    public FirstBoundaryProvider(Grid<Node2D> grid)
    {
        _grid = grid;
    }

    public FirstConditionValue[] GetConditionsValues(FirstCondition[] conditions)
    {
        var conditionsValues = new FirstConditionValue[conditions.Length];

        if (_indexes is null)
        {
            _indexes = new int[conditionsValues.Length][];

            for (var i = 0; i < conditionsValues.Length; i++)
            {
                _indexes[i] = new int[2];
            }
        }

        if (_values is null)
        {
            _values = new Vector[conditionsValues.Length];

            for (var i = 0; i < conditionsValues.Length; i++)
            {
                _values[i] = new Vector(2);
            }
        }

        for (var i = 0; i < conditions.Length; i++)
        {
            var (indexes, _) = _grid.Elements[conditions[i].ElementIndex].GetBoundNodeIndexes(conditions[i].Bound, _indexes[i]);

            conditionsValues[i] = new FirstConditionValue(new LocalVector(indexes, _values[i]));
        }

        return conditionsValues;
    }

    public FirstCondition[] GetConditions(int elementsByLength, int elementsByHeight)
    {
        var conditions = new FirstCondition[elementsByLength + elementsByHeight];
        var j = 0;

        for (var i = 0; i < elementsByLength; i++, j++)
        {
            conditions[j] = new FirstCondition(i, Bound.Lower);
        }

        for (var i = 0; i < elementsByHeight; i++, j++)
        {
            conditions[j] = new FirstCondition((i + 1) * elementsByLength - 1, Bound.Right);
        }

        return conditions;
    }
}