using DirectProblem.Core;
using DirectProblem.Core.GridComponents;
using DirectProblem.FEM;

namespace DirectProblem.TwoDimensional.Assembling.Local;

public class LocalBasisFunctionsProvider
{
    private readonly Grid<Node2D> _grid;
    private readonly LinearFunctionsProvider _linearFunctionsProvider;
    private readonly LocalBasisFunction[] _localBasisFunctions = new LocalBasisFunction[4];

    public LocalBasisFunctionsProvider(Grid<Node2D> grid, LinearFunctionsProvider linearFunctionsProvider)
    {
        _grid = grid;
        _linearFunctionsProvider = linearFunctionsProvider;
    }

    public LocalBasisFunction[] GetBilinearFunctions(Element element)
    {
        var firstXFunction =
            _linearFunctionsProvider.CreateFirstFunction(_grid.Nodes[element.NodesIndexes[1]].R, element.Length);
        var secondXFunction =
            _linearFunctionsProvider.CreateSecondFunction(_grid.Nodes[element.NodesIndexes[0]].R, element.Length);
        var firstYFunction =
            _linearFunctionsProvider.CreateFirstFunction(_grid.Nodes[element.NodesIndexes[2]].Z, element.Height);
        var secondYFunction =
            _linearFunctionsProvider.CreateSecondFunction(_grid.Nodes[element.NodesIndexes[0]].Z, element.Height);

        _localBasisFunctions[0] = new LocalBasisFunction(firstXFunction, firstYFunction);
        _localBasisFunctions[1] = new LocalBasisFunction(secondXFunction, firstYFunction);
        _localBasisFunctions[2] = new LocalBasisFunction(firstXFunction, secondYFunction);
        _localBasisFunctions[3] = new LocalBasisFunction(secondXFunction, secondYFunction);

        return _localBasisFunctions;
    }
}