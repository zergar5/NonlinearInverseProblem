using DirectProblem.Core;
using DirectProblem.Core.Base;
using DirectProblem.Core.Boundary;
using DirectProblem.Core.Global;
using DirectProblem.Core.GridComponents;
using DirectProblem.FEM;
using DirectProblem.SLAE.Preconditions;
using DirectProblem.SLAE.Solvers;
using DirectProblem.TwoDimensional.Assembling;
using DirectProblem.TwoDimensional.Assembling.Boundary;
using DirectProblem.TwoDimensional.Assembling.Global;
using DirectProblem.TwoDimensional.Assembling.Local;
using DirectProblem.TwoDimensional.Assembling.MatrixTemplates;
using DirectProblem.TwoDimensional.Parameters;

namespace DirectProblem;

public class DirectProblemSolver
{
    private static readonly MatrixPortraitBuilder MatrixPortraitBuilder = new();
    private static readonly StiffnessMatrixTemplates StiffnessMatrixTemplates = new();
    private static readonly MassMatrixTemplate MassMatrixTemplate = new();
    private static readonly Inserter Inserter = new();
    private static readonly LinearFunctionsProvider LinearFunctionsProvider = new();
    private static readonly GaussExcluder GaussExcluder = new();
    private static readonly MCG MCG = new(new LLTPreconditioner(), new LLTSparse());
    private LocalBasisFunctionsProvider _localBasisFunctionsProvider;
    private LocalAssembler _localAssembler;
    private GlobalAssembler<Node2D> _globalAssembler;
    private FirstBoundaryProvider _firstBoundaryProvider;

    private Grid<Node2D> _grid;
    private MaterialsRepository _materialRepository;
    private FirstCondition[] _firstConditions;
    private SourcesLine _sources;

    private Equation<SymmetricSparseMatrix> _equation;


    public DirectProblemSolver SetGrid(Grid<Node2D> grid)
    {
        _grid = grid;
        return this;
    }

    public DirectProblemSolver SetMaterials(double[] sigmas)
    {
        _materialRepository = new MaterialsRepository(sigmas);
        return this;
    }

    public DirectProblemSolver SetSource(SourcesLine sources)
    {
        _sources = sources;
        return this;
    }

    public DirectProblemSolver SetFirstConditions(FirstCondition[] firstConditions)
    {
        _firstConditions = firstConditions;
        return this;
    }

    public Vector Solve()
    {
        InitLocal();
        InitGlobal();
        AssembleEquation();
        InitPrecondition();

        var solution = MCG.Solve(_equation);

        return solution;
    }

    private void InitLocal()
    {
        var localMatrixAssembler = new LocalMatrixAssembler(_grid, StiffnessMatrixTemplates, MassMatrixTemplate);

        _localBasisFunctionsProvider = new LocalBasisFunctionsProvider(_grid, LinearFunctionsProvider);
        _localAssembler = new LocalAssembler(localMatrixAssembler, _materialRepository);
    }

    private void InitGlobal()
    {
        _globalAssembler = new GlobalAssembler<Node2D>(_grid, MatrixPortraitBuilder, _localAssembler, Inserter,
            GaussExcluder, _localBasisFunctionsProvider);

        _firstBoundaryProvider = new FirstBoundaryProvider(_grid);
    }

    private void AssembleEquation()
    {
        _equation = _globalAssembler
            .AssembleEquation()
            .ApplySource(_sources)
            .ApplyFirstConditions(_firstBoundaryProvider.GetConditionsValues(_firstConditions))
            .BuildEquation();
    }

    private void InitPrecondition()
    {
        var preconditionMatrix = _globalAssembler.AllocatePreconditionMatrix();
        MCG.SetPrecondition(preconditionMatrix);
    }
}