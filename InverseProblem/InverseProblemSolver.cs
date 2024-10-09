using DirectProblem;
using DirectProblem.Core;
using DirectProblem.Core.Base;
using DirectProblem.Core.Global;
using DirectProblem.Core.GridComponents;
using DirectProblem.FEM;
using DirectProblem.SLAE;
using DirectProblem.TwoDimensional;
using DirectProblem.TwoDimensional.Assembling.Local;
using InverseProblem.Assembling;
using InverseProblem.SLAE;

namespace InverseProblem;

public class InverseProblemSolver
{
    private readonly DirectProblemSolver[] _directProblemSolver;
    private readonly SLAEAssembler _slaeAssembler;
    private readonly Regularizer _regularizer;
    private readonly GaussElimination _gaussElimination;
    private readonly LocalBasisFunctionsProvider[] _localBasisFunctionsProvider;

    private readonly ParametersCollection[] _parametersCollection;
    private Source _source;
    private readonly Node2D[] _receivers;
    private readonly Parameter[] _parameters;
    private readonly double[] _trueFieldValues;
    private readonly Vector _initialParameterValues;

    private double[] _weightsSquares;
    private readonly double[] _currentFieldValues;
    private readonly Grid<Node2D> _grid;
    private FEMSolution _femSolution;

    public InverseProblemSolver
    (
        DirectProblemSolver[] directProblemSolver,
        SLAEAssembler slaeAssembler,
        Regularizer regularizer,
        GaussElimination gaussElimination,
        LocalBasisFunctionsProvider[] localBasisFunctionsProvider,
        Grid<Node2D> grid,
        ParametersCollection[] parametersCollection,
        Source source,
        Node2D[] receivers,
        Parameter[] parameters,
        double[] trueFieldValues,
        Vector initialParameterValues
    )
    {
        _directProblemSolver = directProblemSolver;
        _slaeAssembler = slaeAssembler;
        _regularizer = regularizer;
        _gaussElimination = gaussElimination;
        _localBasisFunctionsProvider = localBasisFunctionsProvider;
        _grid = grid;
        _parametersCollection = parametersCollection;
        _source = source;
        _receivers = receivers;
        _parameters = parameters;
        _trueFieldValues = trueFieldValues;
        _initialParameterValues = initialParameterValues;

        CalculateWeightsSquares();

        _currentFieldValues = new double[_receivers.Length];
    }

    private void CalculateWeightsSquares()
    {
        _weightsSquares = new double[_receivers.Length];

        for (var i = 0; i < _receivers.Length; i++)
        {
            _weightsSquares[i] = Math.Pow(1 / _trueFieldValues[i], 2);
        }

        _slaeAssembler.SetWeightsSquares(_weightsSquares);
    }

    public Vector Solve()
    {
        var previousFunctional = 2d;
        var functional = 10d;
        Equation<Matrix> equation = null!;

        _slaeAssembler.SetGrid(_grid);

        //var resultO = new ResultIO("../InverseProblem/Results/8OtherSigmasCloseAndNearToWell/");
        //var gridO = new GridIO("../InverseProblem/Results/8OtherSigmasCloseAndNearToWell/");

        CalculateFieldValues();
        //resultO.WriteInverseProblemIteration(_receiverLines, _currentFieldValues, _frequencies, "iteration 0 phase differences.txt");
        //gridO.WriteAreas(_grid, _initialParameterValues, "iteration 0 areas.txt");

        Console.WriteLine($"Iteration: 0");
        for (var j = 0; j < _initialParameterValues.Count; j++)
        {
            Console.WriteLine($"{_initialParameterValues[j]}");
        }

        for (var i = 1; i <= MethodsConfig.MaxIterations && CheckFunctional(functional, previousFunctional); i++)
        {
            equation = _slaeAssembler
                .SetCurrentFieldValues(_currentFieldValues)
                .BuildEquation();

            var regularizedEquation = _regularizer.Regularize(equation, out var alphas);

            var parametersDeltas = _gaussElimination.Solve(regularizedEquation);

            Vector.Sum(equation.Solution, parametersDeltas, equation.Solution);

            UpdateParameters(equation.Solution);

            CalculateFieldValues();

            previousFunctional = functional;

            functional = CalculateFunctional();

            CourseHolder.GetFunctionalInfo(i, functional);

            Console.WriteLine();

            for (var j = 0; j < equation.Solution.Count; j++)
            {
                Console.WriteLine($"{equation.Solution[j]} {parametersDeltas[j]} {alphas[j]}");
            }
        }

        Console.WriteLine();

        return equation.Solution;
    }

    private void UpdateParameters(Vector parametersValues)
    {
        for (var i = 0; i < _parameters.Length; i++)
        {
            foreach (var parametersCollection in _parametersCollection)
            {
                parametersCollection.SetParameterValue(_parameters[i], parametersValues[i]);
            }
        }
    }

    private double CalculateFunctional()
    {
        var functional = 0d;

        for (var i = 0; i < _receivers.Length; i++)
        {
            functional += _weightsSquares[i] * Math.Pow(_currentFieldValues[i] - _trueFieldValues[i], 2);
        }

        return functional;
    }

    private bool CheckFunctional(double currentFunctional, double previousFunctional)
    {
        var functionalRatio = Math.Abs(currentFunctional / previousFunctional);
        return Math.Abs(double.Max(1 / functionalRatio, functionalRatio) - 1) > 1e-7 &&
               currentFunctional >= MethodsConfig.FunctionalPrecision;
    }

    private void ChangeMaterials()
    {
        _directProblemSolver[0].SetMaterials(_parametersCollection[0].Materials);
    }

    private void ChangeSourcePower(double current)
    {
        _source.Current = current;
        _directProblemSolver[0].SetSource(_source);
    }

    private void CalculateFieldValues()
    {
        ChangeMaterials();
        ChangeSourcePower(_parametersCollection[0].SourcePower);
        SolveDirectProblem();

        for (var i = 0; i < _receivers.Length; i++)
        {
            var field = _femSolution.Calculate(_receivers[i]);

            _currentFieldValues[i] = field;

            //Console.Write($"receiver {i}                                     \r");
        }

        Console.WriteLine();
    }

    private void SolveDirectProblem()
    {
        var solution = _directProblemSolver[0].AssembleSLAE().Solve();

        _femSolution = new FEMSolution(_grid, solution, _localBasisFunctionsProvider[0]);
    }
}