using DirectProblem.Core;
using DirectProblem.Core.Base;
using DirectProblem.Core.Boundary;
using DirectProblem.Core.Global;
using DirectProblem.Core.GridComponents;
using DirectProblem.Core.Local;
using DirectProblem.FEM.Assembling;
using DirectProblem.FEM.Assembling.Global;
using DirectProblem.FEM.Assembling.Local;
using DirectProblem.TwoDimensional.Assembling.Local;

namespace DirectProblem.TwoDimensional.Assembling.Global;

public class GlobalAssembler<TNode>
{
    private readonly Grid<Node2D> _grid;
    private readonly IMatrixPortraitBuilder<SymmetricSparseMatrix> _matrixPortraitBuilder;
    private readonly ILocalAssembler _localAssembler;
    private readonly IInserter<SymmetricSparseMatrix> _inserter;
    private readonly IGaussExcluder<SymmetricSparseMatrix> _gaussExcluder;
    private readonly LocalBasisFunctionsProvider _localBasisFunctionsProvider;
    private Equation<SymmetricSparseMatrix> _equation;
    private SymmetricSparseMatrix _preconditionMatrix;
    private Vector _bufferVector = new(4);

    public GlobalAssembler
    (
        Grid<Node2D> grid,
        IMatrixPortraitBuilder<SymmetricSparseMatrix> matrixPortraitBuilder,
        ILocalAssembler localAssembler,
        IInserter<SymmetricSparseMatrix> inserter,
        IGaussExcluder<SymmetricSparseMatrix> gaussExcluder,
        LocalBasisFunctionsProvider localBasisFunctionsProvider
    )
    {
        _grid = grid;
        _matrixPortraitBuilder = matrixPortraitBuilder;
        _localAssembler = localAssembler;
        _inserter = inserter;
        _gaussExcluder = gaussExcluder;
        _localBasisFunctionsProvider = localBasisFunctionsProvider;
    }

    public GlobalAssembler<TNode> AssembleEquation()
    {
        var globalMatrix = _matrixPortraitBuilder.Build(_grid);
        _preconditionMatrix = globalMatrix.Clone();
        _equation = new Equation<SymmetricSparseMatrix>(
            globalMatrix,
            new Vector(_grid.Nodes.Length),
            new Vector(_grid.Nodes.Length)
        );

        foreach (var element in _grid)
        {
            var localMatrix = _localAssembler.AssembleMatrix(element);

            _inserter.InsertMatrix(_equation.Matrix, localMatrix);
        }

        return this;
    }

    public GlobalAssembler<TNode> ApplySource(SourcesLine sources)
    {
        var element = _grid.Elements.First(x => ElementHas(x, sources.PointA));

        var basisFunctions = _localBasisFunctionsProvider.GetBilinearFunctions(element);

        for (var i = 0; i < element.NodesIndexes.Length; i++)
        {
            _bufferVector[i] = sources.Current / (2 * Math.PI) * basisFunctions[i].Calculate(sources.PointA);
        }

        _inserter.InsertVector(_equation.RightPart, new LocalVector(element.NodesIndexes, _bufferVector));

        return this;
    }

    public GlobalAssembler<TNode> ApplyFirstConditions(FirstConditionValue[] conditions)
    {
        foreach (var condition in conditions)
        {
            _gaussExcluder.Exclude(_equation, condition);
        }

        return this;
    }

    public Equation<SymmetricSparseMatrix> BuildEquation()
    {
        return _equation;
    }

    public SymmetricSparseMatrix AllocatePreconditionMatrix()
    {
        _preconditionMatrix = _equation.Matrix.Copy(_preconditionMatrix);
        return _preconditionMatrix;
    }

    private bool ElementHas(Element element, Node2D node)
    {
        var leftCornerNode = _grid.Nodes[element.NodesIndexes[0]];
        var rightCornerNode = _grid.Nodes[element.NodesIndexes[^1]];
        return node.R >= leftCornerNode.R && node.Z >= leftCornerNode.Z &&
               node.R <= rightCornerNode.R && node.Z <= rightCornerNode.Z;
    }
}