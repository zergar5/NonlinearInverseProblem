using DirectProblem.Core;
using DirectProblem.Core.Base;
using DirectProblem.Core.Boundary;
using DirectProblem.Core.Global;
using DirectProblem.Core.GridComponents;
using DirectProblem.SLAE.Preconditions;
using DirectProblem.SLAE.Solvers;
using DirectProblem.TwoDimensional.Assembling;
using DirectProblem.TwoDimensional.Assembling.Boundary;
using DirectProblem.TwoDimensional.Assembling.Global;
using DirectProblem.TwoDimensional.Assembling.Local;

namespace DirectProblem;

public class DirectProblemSolver
{
    private readonly MatrixPortraitBuilder _matrixPortraitBuilder;
    private readonly Inserter _inserter;
    private readonly GaussExcluder _gaussExcluder;
    private readonly MCG _MCG;

    private readonly LocalBasisFunctionsProvider _localBasisFunctionsProvider;
    private readonly LocalMatrixAssembler _localMatrixAssembler;
    private readonly LocalAssembler _localAssembler;
    private readonly FirstBoundaryProvider _firstBoundaryProvider;
    private readonly GlobalAssembler<Node2D> _globalAssembler;

    private Grid<Node2D> _grid;
    private Source _source;
    private FirstConditionValue[] _firstConditions;

    private Equation<SymmetricSparseMatrix> _equation;

    public DirectProblemSolver(Grid<Node2D> grid, Material[] materials)
    {
        _grid = grid;

        _matrixPortraitBuilder = new MatrixPortraitBuilder();
        _inserter = new Inserter();
        _gaussExcluder = new GaussExcluder();
        _MCG = new(new LLTPreconditioner(), new LLTSparse());
        _localBasisFunctionsProvider = new LocalBasisFunctionsProvider(grid);
        _localMatrixAssembler = new LocalMatrixAssembler(grid);
        _localAssembler = new LocalAssembler(_localMatrixAssembler, materials);
        _firstBoundaryProvider = new FirstBoundaryProvider(grid);
        _firstConditions = _firstBoundaryProvider.GetConditionsValues(
            _firstBoundaryProvider.GetConditions(grid.Nodes.RLength - 1, grid.Nodes.ZLength - 1)
            );
        _globalAssembler = new GlobalAssembler<Node2D>(grid, _matrixPortraitBuilder,
            _localAssembler, _inserter, _gaussExcluder);
    }

    public DirectProblemSolver SetGrid(Grid<Node2D> grid)
    {
        _grid = grid;
        _localBasisFunctionsProvider.SetGrid(grid);
        _localMatrixAssembler.SetGrid(grid);
        _firstBoundaryProvider.SetGrid(grid);
        _firstConditions = _firstBoundaryProvider.GetConditionsValues(
            _firstBoundaryProvider.GetConditions(grid.Nodes.RLength - 1, grid.Nodes.ZLength - 1)
        );

        return this;
    }

    public DirectProblemSolver SetMaterials(Material[] materials)
    {
        _localAssembler.SetMaterials(materials);

        return this;
    }

    public DirectProblemSolver SetSource(Source source)
    {
        _source = source;

        return this;
    }

    public DirectProblemSolver AssembleSLAE()
    {
        _equation = _globalAssembler
            .AssembleEquation()
            .ApplySource(_source)
            .ApplyFirstConditions(_firstConditions)
            .BuildEquation();

        return this;
    }

    public Vector Solve()
    {
        var preconditionMatrix = _globalAssembler.AllocatePreconditionMatrix();
        _MCG.SetPrecondition(preconditionMatrix);

        var solution = _MCG.Solve(_equation);

        return solution;
    }
}