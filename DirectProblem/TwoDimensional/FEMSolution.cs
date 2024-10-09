using DirectProblem.Core;
using DirectProblem.Core.Base;
using DirectProblem.Core.GridComponents;
using DirectProblem.TwoDimensional.Assembling.Local;

namespace DirectProblem.TwoDimensional;

public class FEMSolution
{
    private readonly Grid<Node2D> _grid;
    private readonly Vector _solution;
    private readonly LocalBasisFunctionsProvider _basisFunctionsProvider;

    public FEMSolution(Grid<Node2D> grid, Vector solution, LocalBasisFunctionsProvider basisFunctionsProvider)
    {
        _grid = grid;
        _solution = solution;
        _basisFunctionsProvider = basisFunctionsProvider;
    }

    public double Calculate(Node2D point)
    {
        if (AreaHas(point))
        {
            var element = _grid.Elements.First(x => ElementHas(x, point));

            var basisFunctions = _basisFunctionsProvider.GetBilinearFunctions(element);

            var sum = 0d;

            sum += element.NodesIndexes
                .Select((t, i) => _solution[t] * basisFunctions[i].Calculate(point))
                .Sum();

            return sum;
        }

        return double.NaN;
    }

    public double CalcError(Func<Node2D, double> u)
    {
        var solution = new Vector(_solution.Count);
        var trueSolution = new Vector(_solution.Count);

        for (var i = 0; i < _solution.Count; i++)
        {
            solution[i] = Calculate(_grid.Nodes[i]);
            trueSolution[i] = u(_grid.Nodes[i]);
        }

        Vector.Subtract(solution, trueSolution, trueSolution);

        return trueSolution.Norm;
    }

    private bool ElementHas(Element element, Node2D node)
    {
        var leftCornerNode = _grid.Nodes[element.NodesIndexes[0]];
        var rightCornerNode = _grid.Nodes[element.NodesIndexes[^1]];
        return node.R >= leftCornerNode.R && node.Z >= leftCornerNode.Z &&
               node.R <= rightCornerNode.R && node.Z <= rightCornerNode.Z;
    }

    private bool AreaHas(Node2D node)
    {
        var leftCornerNode = _grid.Nodes[0];
        var rightCornerNode = _grid.Nodes[^1];
        return node.R >= leftCornerNode.R && node.Z >= leftCornerNode.Z &&
               node.R <= rightCornerNode.R && node.Z <= rightCornerNode.Z;
    }
}