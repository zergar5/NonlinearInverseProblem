using DirectProblem.Core;
using DirectProblem.Core.GridComponents;
using DirectProblem.GridGenerator.Intervals.Splitting;

namespace DirectProblem.GridGenerator;

public class GridBuilder2D : IGridBuilder<Node2D>
{
    private AxisSplitParameter _rAxisSplitParameter;
    private AxisSplitParameter _zAxisSplitParameter;
    private int[]? _materialsId;
    private Area[]? _areas;

    public GridBuilder2D SetRAxis(AxisSplitParameter splitParameter)
    {
        _rAxisSplitParameter = splitParameter;
        return this;
    }

    public GridBuilder2D SetZAxis(AxisSplitParameter splitParameter)
    {
        _zAxisSplitParameter = splitParameter;
        return this;
    }

    public GridBuilder2D SetMaterials(int[] materialsId)
    {
        _materialsId = materialsId;
        return this;
    }

    public GridBuilder2D SetAreas(Area[] areas)
    {
        _areas = areas;
        return this;
    }

    public Grid<Node2D> Build()
    {
        var points = CreatePoints();

        var elements = CreateElements(points);

        return new Grid<Node2D>(
            points,
            elements,
            _areas
        );
    }

    private PointsCollection CreatePoints()
    {
        if (_rAxisSplitParameter == null || _zAxisSplitParameter == null)
            throw new ArgumentNullException();

        var xNodes = _rAxisSplitParameter.CreateAxis().ToArray();
        var yNodes = _zAxisSplitParameter.CreateAxis().ToArray();

        return new PointsCollection(xNodes, yNodes);
    }

    private Element[] CreateElements(PointsCollection nodes)
    {
        var totalXElements = nodes.RLength - 1;
        var totalYElements = nodes.ZLength - 1;
        var totalElements = totalXElements * totalYElements;

        var elements = new Element[totalElements];

        for (var topRow = 1; topRow < nodes.ZLength; topRow++)
        {
            for (var rightColumn = 1; rightColumn < nodes.RLength; rightColumn++)
            {
                var elementIndex = rightColumn - 1 + (topRow - 1) * totalXElements;
                var indexes = GetNodesIndexes(topRow - 1, rightColumn - 1, totalXElements);

                var leftBottom = nodes[indexes[0]];
                var leftTop = nodes[indexes[2]];
                var rightBottom = nodes[indexes[1]];
                var rightTop = nodes[indexes[3]];

                var length = rightBottom.R - leftBottom.R;
                var height = leftTop.Z - leftBottom.Z;

                var materialId = GetElementMaterial(elementIndex, leftBottom, rightTop);

                elements[elementIndex] = CreateElement(indexes, length, height, materialId);
            }
        }

        return elements;
    }

    private int GetElementMaterial(int elementIndex, Node2D lowerLeftCornerNode, Node2D upperRightCornerNode)
    {
        if (_areas != null)
        {
            var area = _areas.First(a => a
                .Has(lowerLeftCornerNode, upperRightCornerNode));

            return area.MaterialId;
        }
        if (_materialsId is not null)
        {
            return _materialsId[elementIndex];
        }
        return 0;
    }

    private int[] GetNodesIndexes(int bottomRow, int leftColumn, int totalXElements)
    {
        var indexes = new int[4];

        indexes[0] = leftColumn + bottomRow * (totalXElements + 1);
        indexes[1] = leftColumn + 1 + bottomRow * (totalXElements + 1);
        indexes[2] = leftColumn + (bottomRow + 1) * (totalXElements + 1);
        indexes[3] = leftColumn + 1 + (bottomRow + 1) * (totalXElements + 1);

        return indexes;
    }

    private Element CreateElement(int[] nodesIndexes, double length, double height, int materialId)
    {
        var element = new Element(nodesIndexes, length, height, materialId);

        return element;
    }
}