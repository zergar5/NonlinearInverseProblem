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
using DirectProblem.TwoDimensional.Assembling.MatrixTemplates;

namespace DirectProblem.TwoDimensional.Assembling.Global;

public class GlobalAssembler<TNode>
{
    public const double Delta = 1e-13;

    private readonly Grid<Node2D> _grid;
    private readonly IMatrixPortraitBuilder<SymmetricSparseMatrix> _matrixPortraitBuilder;
    private readonly ILocalAssembler _localAssembler;
    private readonly IInserter<SymmetricSparseMatrix> _inserter;
    private readonly IGaussExcluder<SymmetricSparseMatrix> _gaussExcluder;
    private Equation<SymmetricSparseMatrix> _equation;
    private SymmetricSparseMatrix _preconditionMatrix;
    private Matrix _massMatrix = MassMatrixTemplate.MassMatrix;
    private int[] _indexes = new int[2];
    private Vector _bufferVector = new(2);
    private Vector _thetas = new(2);

    public GlobalAssembler
    (
        Grid<Node2D> grid,
        IMatrixPortraitBuilder<SymmetricSparseMatrix> matrixPortraitBuilder,
        ILocalAssembler localAssembler,
        IInserter<SymmetricSparseMatrix> inserter,
        IGaussExcluder<SymmetricSparseMatrix> gaussExcluder
    )
    {
        _grid = grid;
        _matrixPortraitBuilder = matrixPortraitBuilder;
        _localAssembler = localAssembler;
        _inserter = inserter;
        _gaussExcluder = gaussExcluder;
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

    public GlobalAssembler<TNode> ApplySource(Source source)
    {
        ApplySource(source.StartPoint, source.Current);
        ApplySource(source.EndPoint, -source.Current);

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

    private void ApplySource(Node2D node, double current)
    {
        var element = _grid.Elements.First(x => ElementHas(x, node));
        var (indexes, height) = element.GetBoundNodeIndexes(Bound.Left, _indexes);

        for (var i = 0; i < indexes.Length; i++)
        {
            _thetas[i] = current / (2 * Math.PI * node.R);
        }

        Matrix.Multiply(_massMatrix, _thetas, _bufferVector);
        Vector.Multiply(height / 6, _bufferVector, _bufferVector);

        _inserter.InsertVector(_equation.RightPart, new LocalVector(element.NodesIndexes, _bufferVector));
    }

    private bool ElementHas(Element element, Node2D node)
    {
        var leftCornerNode = _grid.Nodes[element.NodesIndexes[0]];
        var rightCornerNode = _grid.Nodes[element.NodesIndexes[^1]];
        return (leftCornerNode.R <= node.R ||
               Math.Abs(leftCornerNode.R - node.R) < Delta) &&
               (leftCornerNode.Z <= node.Z ||
                Math.Abs(leftCornerNode.Z - node.Z) < Delta) &&
               (rightCornerNode.R >= node.R ||
                Math.Abs(rightCornerNode.R - node.R) < Delta) &&
               (rightCornerNode.Z >= node.Z ||
                Math.Abs(rightCornerNode.Z - node.Z) < Delta);
    }
}