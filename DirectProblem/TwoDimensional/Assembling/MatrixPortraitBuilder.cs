using DirectProblem.Core;
using DirectProblem.Core.Global;
using DirectProblem.Core.GridComponents;
using DirectProblem.FEM.Assembling;

namespace DirectProblem.TwoDimensional.Assembling;

public class MatrixPortraitBuilder : IMatrixPortraitBuilder<SymmetricSparseMatrix>
{
    private List<SortedSet<int>> _adjacencyList;

    public SymmetricSparseMatrix Build(Grid<Node2D> grid)
    {
        BuildAdjacencyList(grid);

        var amount = 0;
        var rowsIndexes = new[] { 0 };
        rowsIndexes = rowsIndexes.Concat(_adjacencyList.Select(nodeSet => amount += nodeSet.Count)).ToArray();

        var columnsIndexes = _adjacencyList.SelectMany(nodeList => nodeList).ToArray();

        return new SymmetricSparseMatrix(rowsIndexes, columnsIndexes);
    }

    private void BuildAdjacencyList(Grid<Node2D> grid)
    {
        _adjacencyList = new List<SortedSet<int>>(grid.Nodes.Length);

        for (var i = 0; i < grid.Nodes.Length; i++)
        {
            _adjacencyList.Add([]);
        }

        foreach (var element in grid)
        {
            var nodesIndexes = element.NodesIndexes;

            foreach (var currentNode in nodesIndexes)
            {
                foreach (var nodeIndex in nodesIndexes)
                {
                    if (currentNode > nodeIndex) _adjacencyList[currentNode].Add(nodeIndex);
                }
            }
        }
    }
}