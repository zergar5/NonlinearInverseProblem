using DirectProblem;
using DirectProblem.Core;
using DirectProblem.Core.Base;
using DirectProblem.Core.Global;
using DirectProblem.Core.GridComponents;
using DirectProblem.TwoDimensional;
using DirectProblem.TwoDimensional.Assembling.Local;
using Vector = DirectProblem.Core.Base.Vector;

namespace InverseProblem.Assembling;

public class SLAEAssembler
{
    private readonly DirectProblemSolver[] _directProblemSolver;
    private readonly LocalBasisFunctionsProvider[] _localBasisFunctionsProvider;

    private readonly ParametersCollection[] _parametersCollection;
    private Source _source;
    private readonly Node2D[] _receivers;
    private readonly Parameter[] _parameters;
    private readonly double[] _trueFieldValues;
    private double[] _weightsSquares;
    private double[] _fieldValues;
    private readonly double[,] _fieldValuesDerivatives;
    private readonly Equation<Matrix> _equation;

    private Grid<Node2D> _grid;

    private readonly Task[] _tasks;

    public SLAEAssembler
    (
        DirectProblemSolver[] directProblemSolver,
        LocalBasisFunctionsProvider[] localBasisFunctionsProvider,
        ParametersCollection[] parametersCollection,
        Source source,
        Node2D[] receivers,
        Parameter[] parameters,
        Vector initialValues,
        double[] trueFieldValues
    )
    {
        _directProblemSolver = directProblemSolver;
        _localBasisFunctionsProvider = localBasisFunctionsProvider;

        _parametersCollection = parametersCollection;
        _source = source;
        _receivers = receivers;
        _parameters = parameters;
        _trueFieldValues = trueFieldValues;

        _equation = new Equation<Matrix>(new Matrix(_parameters.Length), initialValues,
            new Vector(_parameters.Length));

        _fieldValues = new double[_receivers.Length];

        _fieldValuesDerivatives = new double[_parameters.Length, _receivers.Length];

        _tasks = new Task[directProblemSolver.Length];
    }

    public SLAEAssembler SetGrid(Grid<Node2D> grid)
    {
        _grid = grid;

        return this;
    }

    public SLAEAssembler SetWeightsSquares(double[] weightsSquares)
    {
        _weightsSquares = weightsSquares;

        return this;
    }

    public SLAEAssembler SetCurrentFieldValues(double[] fieldValues)
    {
        _fieldValues = fieldValues;

        return this;
    }

    public Equation<Matrix> BuildEquation()
    {
        CalculatePhaseDifferences();
        AssembleMatrix();
        AssembleRightPart();

        return _equation;
    }

    private void ChangeMaterials(int solverId)
    {
        _directProblemSolver[solverId].SetMaterials(_parametersCollection[solverId].Materials);
    }

    private void ChangeSourcePower(int solverId, double current)
    {
        _source.Current = current;
        _directProblemSolver[solverId].SetSource(_source);
    }

    private FEMSolution SolveDirectProblem(int solverId)
    {
        var solution = _directProblemSolver[solverId].AssembleSLAE().Solve();

        return new FEMSolution(_grid, solution, _localBasisFunctionsProvider[solverId]);
    }

    private void AssembleMatrix()
    {
        Parallel.For(0, _equation.Matrix.CountRows, q =>
        {
            for (var s = 0; s < _equation.Matrix.CountColumns; s++)
            {
                var sum = 0d;

                for (var k = 0; k < _receivers.Length; k++)
                {
                    sum += _weightsSquares[k] * _fieldValuesDerivatives[q, k] *
                                              _fieldValuesDerivatives[s, k];
                }

                _equation.Matrix[q, s] = sum;
            }
        });
    }

    private void AssembleRightPart()
    {
        Parallel.For(0, _equation.Matrix.CountRows, q =>
        {
            var sum = 0d;

            for (var k = 0; k < _receivers.Length; k++)
            {
                sum -= _weightsSquares[k] *
                       (_fieldValues[k] - _trueFieldValues[k]) *
                       _fieldValuesDerivatives[q, k];
            }

            _equation.RightPart[q] = sum;
        });
    }

    private void CalculatePhaseDifferences()
    {
        for (var i = 0; i < _parameters.Length; i++)
        {
            var taskId = i % _tasks.Length;

            if (i >= _tasks.Length)
            {
                taskId = Task.WaitAny(_tasks);
            }

            var task = new Task(j =>
            {
                var currentParameter = _parameters[(int)j];
                var parameterValue = _parametersCollection[taskId].GetParameterValue(currentParameter);
                var delta = parameterValue * 5e-2;
                _parametersCollection[taskId].SetParameterValue(_parameters[(int)j], parameterValue + delta);

                switch (currentParameter.ParameterType)
                {
                    case ParameterType.Current:
                        ChangeSourcePower(taskId, _parametersCollection[taskId].GetParameterValue(currentParameter));
                        break;
                    case ParameterType.Sigma:
                        ChangeMaterials(taskId);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                CalculateFieldValueDerivatives((int)j, delta, taskId);

                _parametersCollection[taskId].SetParameterValue(_parameters[(int)j], parameterValue);

                if (currentParameter.ParameterType == ParameterType.Current)
                {
                    ChangeSourcePower(taskId, parameterValue);
                }
                else
                {
                    ChangeMaterials(taskId);
                }

            }, i);

            task.Start();

            _tasks[taskId] = task;
        }

        Task.WaitAll(_tasks);
    }

    private void CalculateFieldValueDerivatives(int parameterIndex, double delta, int solverId)
    {
        var femSolution = SolveDirectProblem(solverId);

        for (var i = 0; i < _receivers.Length; i++)
        {
            var field = femSolution.Calculate(_receivers[i]);

            _fieldValuesDerivatives[parameterIndex, i] = field;

            _fieldValuesDerivatives[parameterIndex, i] =
                (_fieldValuesDerivatives[parameterIndex, i] - _fieldValues[i]) / delta;

            //Console.Write($"derivative {parameterIndex} receiver {i}                              \r");
        }
    }
}