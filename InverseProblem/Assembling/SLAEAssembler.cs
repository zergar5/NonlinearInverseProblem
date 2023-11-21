using DirectProblem;
using DirectProblem.Core;
using DirectProblem.Core.Base;
using DirectProblem.Core.Boundary;
using DirectProblem.Core.Global;
using DirectProblem.Core.GridComponents;
using DirectProblem.FEM;
using DirectProblem.GridGenerator;
using DirectProblem.GridGenerator.Intervals.Splitting;
using DirectProblem.TwoDimensional;
using DirectProblem.TwoDimensional.Assembling.Local;
using Vector = DirectProblem.Core.Base.Vector;

namespace InverseProblem.Assembling;

public class SLAEAssembler
{
    private static readonly LinearFunctionsProvider LinearFunctionsProvider = new();
    public const double Delta = 1e-3;

    private readonly GridBuilder2D _gridBuilder2D;
    private readonly DirectProblemSolver _directProblemSolver;

    private readonly SourcesLine _sourcesLine;
    private readonly ReceiversLine[] _receiversLines;
    private readonly Parameter[] _parameters;
    private readonly double[] _truePotentialDifferences;
    private double[] _weightsSquares;
    private readonly double[] _potentialDifferences;
    private readonly double[][] _derivativesPotentialDifferences;

    private double[] _rPoints;
    private double[] _zPoints;
    private Area[] _areas;
    private double[] _sigmas;
    private FirstCondition[] _firstConditions;

    private readonly Equation<Matrix> _equation;

    private Grid<Node2D> _grid;
    private LocalBasisFunctionsProvider _localBasisFunctionsProvider;
    private FEMSolution _femSolution;

    public SLAEAssembler
    (
        GridBuilder2D gridBuilder2D,
        DirectProblemSolver directProblemSolver,
        SourcesLine sourcesLine,
        ReceiversLine[] receiversLines,
        Parameter[] parameters,
        Vector initialValues,
        double[] truePotentialDifferences,
        double[] rPoints,
        double[] zPoints,
        Area[] areas,
        double[] sigmas,
        FirstCondition[] firstConditions
    )
    {
        _gridBuilder2D = gridBuilder2D;
        _directProblemSolver = directProblemSolver;
        _sourcesLine = sourcesLine;
        _receiversLines = receiversLines;
        _parameters = parameters;
        _truePotentialDifferences = truePotentialDifferences;
        _rPoints = rPoints;
        _zPoints = zPoints;
        _areas = areas;
        _sigmas = sigmas;
        _firstConditions = firstConditions;

        CalculateWeights();

        _equation = new Equation<Matrix>(new Matrix(parameters.Length), initialValues,
            new Vector(parameters.Length));

        _potentialDifferences = new double[_receiversLines.Length];
        _derivativesPotentialDifferences = new double[parameters.Length][];

        for (var i = 0; i < parameters.Length; i++)
        {
            _derivativesPotentialDifferences[i] = new double[_receiversLines.Length];
        }
    }

    public void SetParameter(Parameter parameter, double value)
    {
        if (parameter.ParameterType == ParameterType.HorizontalBound)
        {
            _zPoints[parameter.Index] = value;
        }
        else if (parameter.ParameterType == ParameterType.VerticalBound)
        {
            _rPoints[parameter.Index] = value;
        }
        else
        {
            _sigmas[parameter.Index] = value;
        }
    }

    public double GetParameter(Parameter parameter)
    {
        return parameter.ParameterType switch
        {
            ParameterType.HorizontalBound => _zPoints[parameter.Index],
            ParameterType.VerticalBound => _rPoints[parameter.Index],
            _ => _sigmas[parameter.Index]
        };
    }

    public Equation<Matrix> BuildEquation()
    {
        AssembleSLAE();
        return _equation;
    }

    private void CalculateWeights()
    {
        _weightsSquares = new double[_truePotentialDifferences.Length];

        for (var i = 0; i < _receiversLines.Length; i++)
        {
            _weightsSquares[i] = Math.Pow(1d / _truePotentialDifferences[i], 2);
        }
    }

    private void AssembleSLAE()
    {
        CalculatePotentialDifferences();
        AssembleMatrix();
        AssembleRightPart();
    }

    private void AssembleDirectProblem()
    {
        _grid = _gridBuilder2D
            .SetRAxis(new AxisSplitParameter(
                    _rPoints,
                    new UniformSplitter(150)
                )
            )
            .SetZAxis(new AxisSplitParameter(
                    _zPoints,
                    new UniformSplitter(20),
                    new UniformSplitter(93)
                )
            )
            .SetAreas(new Area[]
            {
                new(0, new Node2D(_rPoints[0], _zPoints[^2]),
                    new Node2D(_rPoints[1], _zPoints[^1])),
                new(1, new Node2D(_rPoints[0], _zPoints[^3]),
                    new Node2D(_rPoints[1], _zPoints[^2]))
            })
            .Build();

        _localBasisFunctionsProvider = new LocalBasisFunctionsProvider(_grid, LinearFunctionsProvider);

        _directProblemSolver
            .SetGrid(_grid)
            .SetMaterials(_sigmas)
            .SetSource(_sourcesLine)
            .SetFirstConditions(_firstConditions);
    }

    private void SolveDirectProblem()
    {
        var solution = _directProblemSolver.Solve();

        _femSolution = new FEMSolution(_grid, solution, _localBasisFunctionsProvider);
    }

    private void AssembleMatrix()
    {
        for (var q = 0; q < _equation.Matrix.CountRows; q++)
        {
            for (var s = 0; s < _equation.Matrix.CountColumns; s++)
            {
                _equation.Matrix[q, s] = 0;

                for (var i = 0; i < _receiversLines.Length; i++)
                {
                    _equation.Matrix[q, s] += _weightsSquares[i] * _derivativesPotentialDifferences[q][i] *
                                              _derivativesPotentialDifferences[s][i];
                }
            }
        }
    }

    private void AssembleRightPart()
    {
        for (var q = 0; q < _equation.Matrix.CountRows; q++)
        {
            _equation.RightPart[q] = 0;

            for (var i = 0; i < _receiversLines.Length; i++)
            {
                _equation.RightPart[q] -= _weightsSquares[i] *
                                          (_potentialDifferences[i] - _truePotentialDifferences[i]) *
                                          _derivativesPotentialDifferences[q][i];
            }
        }
    }

    private void CalculatePotentialDifferences()
    {
        // посчитать МКЭ
        AssembleDirectProblem();
        SolveDirectProblem();

        CalculatePotentialDifference(_potentialDifferences);

        // посчитать все производные
        for (var i = 0; i < _parameters.Length; i++)
        {
            var parameterValue = GetParameter(_parameters[i]);
            SetParameter(_parameters[i], parameterValue + Delta);

            AssembleDirectProblem();
            SolveDirectProblem();

            CalculatePotentialDifference(_derivativesPotentialDifferences[i]);

            SetParameter(_parameters[i], parameterValue);

            for (var j = 0; j < _receiversLines.Length; j++)
            {
                _derivativesPotentialDifferences[i][j] =
                    (_derivativesPotentialDifferences[i][j] - _potentialDifferences[j]) / Delta;
            }
        }
    }

    private void CalculatePotentialDifference(double[] potentialDifferences)
    {
        for (var i = 0; i < _receiversLines.Length; i++)
        {
            var distanceSourceM = Node2D.Distance(_sourcesLine.PointA, _receiversLines[i].PointM);
            var distanceSourceN = Node2D.Distance(_sourcesLine.PointA, _receiversLines[i].PointN);

            var potentialM = _femSolution.CalculatePotential(new Node2D(distanceSourceM, 0));
            var potentialN = _femSolution.CalculatePotential(new Node2D(distanceSourceN, 0));

            var potentialDifferenceAMN = potentialM - potentialN;

            distanceSourceM = Node2D.Distance(_sourcesLine.PointB, _receiversLines[i].PointM);
            distanceSourceN = Node2D.Distance(_sourcesLine.PointB, _receiversLines[i].PointN);

            potentialM = -_femSolution.CalculatePotential(new Node2D(distanceSourceM, 0));
            potentialN = -_femSolution.CalculatePotential(new Node2D(distanceSourceN, 0));

            var potentialDifferenceBMN = potentialM - potentialN;

            potentialDifferences[i] = potentialDifferenceAMN + potentialDifferenceBMN;
        }
    }
}