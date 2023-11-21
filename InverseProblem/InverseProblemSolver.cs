using DirectProblem;
using DirectProblem.Core.Base;
using DirectProblem.Core.Boundary;
using DirectProblem.Core.Global;
using DirectProblem.Core.GridComponents;
using DirectProblem.FEM;
using DirectProblem.GridGenerator;
using DirectProblem.SLAE;
using InverseProblem.Assembling;
using InverseProblem.SLAE;

namespace InverseProblem;

public class InverseProblemSolver
{
    private static readonly GaussElimination GaussElimination = new();
    private static readonly Regularizer Regularizer = new(GaussElimination);
    private static readonly DirectProblemSolver DirectProblemSolver = new();
    private readonly GridBuilder2D _gridBuilder2D;
    private SLAEAssembler _slaeAssembler;
    private Matrix _bufferMatrix;
    private Vector _bufferVector;
    private Vector _residualBufferVector;

    private SourcesLine _sourceLine;
    private ReceiversLine[] _receiversLines;

    private Parameter[] _parameters;
    private Vector _trueValues;
    private Vector _initialValues;

    private double[] _truePotentialDifferences;

    private double[] _rPoints;
    private double[] _zPoints;
    private Area[] _areas;
    private double[] _sigmas;
    private FirstCondition[] _firstConditions;

    public InverseProblemSolver(GridBuilder2D gridBuilder2D)
    {
        _gridBuilder2D = gridBuilder2D;
    }

    public InverseProblemSolver SetSource(SourcesLine sourcesLine)
    {
        _sourceLine = sourcesLine;

        return this;
    }

    public InverseProblemSolver SetReceivers(ReceiversLine[] receivers)
    {
        _receiversLines = receivers;

        return this;
    }

    public InverseProblemSolver SetParameters(Parameter[] parameters, Vector trueValues, Vector initialValues)
    {
        _parameters = parameters;
        _trueValues = trueValues;
        _initialValues = initialValues;

        return this;
    }

    public InverseProblemSolver SetTruePotentialDifferences(double[] truePotentialDifferences)
    {
        _truePotentialDifferences = truePotentialDifferences;

        return this;
    }

    public InverseProblemSolver SetInitialDirectProblemParameters(double[] rPoints, double[] zPoints, Area[] areas,
        double[] sigmas, FirstCondition[] firstConditions)
    {
        _rPoints = rPoints;
        _zPoints = zPoints;
        _areas = areas;
        _sigmas = sigmas;
        _firstConditions = firstConditions;

        return this;
    }

    private void Init()
    {
        _slaeAssembler = new SLAEAssembler(_gridBuilder2D, DirectProblemSolver, _sourceLine, _receiversLines,
            _parameters, _initialValues, _truePotentialDifferences, _rPoints, _zPoints, _areas, _sigmas,
            _firstConditions);

        _bufferMatrix = Matrix.CreateIdentityMatrix(_parameters.Length);
        _bufferVector = new Vector(_parameters.Length);
        _residualBufferVector = new Vector(_parameters.Length);
        Regularizer.BufferMatrix = _bufferMatrix;
        Regularizer.BufferVector = _bufferVector;
        Regularizer.ResidualBufferVector = _residualBufferVector;
    }

    public Vector Solve()
    {
        //Собрать сетку для базовых значений, учесть только А электрод
        //Посчитать разности потенциалов M-N для А, сделать аналогично для B, но потенциалы буду с минусом
        //Сложить разности при А и при Б
        Init();

        var residual = 1d;
        Equation<Matrix> equation = null!;

        for (var i = 1; i <= MethodsConfig.MaxIterations && residual > MethodsConfig.Eps; i++)
        {
            equation = _slaeAssembler.BuildEquation();

            var alpha = Regularizer.Regularize(equation, _trueValues);

            Matrix.CreateIdentityMatrix(_bufferMatrix);

            Matrix.Sum(equation.Matrix, Matrix.Multiply(alpha, _bufferMatrix, _bufferMatrix), equation.Matrix);

            Vector.Subtract(
                equation.RightPart, Vector.Multiply(
                    alpha, Vector.Subtract(equation.Solution, _trueValues, _bufferVector),
                    _bufferVector),
                equation.RightPart);

            _bufferMatrix = equation.Matrix.Copy(_bufferMatrix);
            _bufferVector = equation.RightPart.Copy(_bufferVector);

            _bufferVector = GaussElimination.Solve(_bufferMatrix, _bufferVector);

            residual = CalculateResidual(equation);

            Vector.Sum(equation.Solution, _bufferVector, equation.Solution);

            UpdateParameters(equation.Solution);

            CourseHolder.GetInfo(i, residual);
        }

        Console.WriteLine();

        return equation.Solution;
    }

    private void UpdateParameters(Vector vector)
    {
        for (var i = 0; i < _parameters.Length; i++)
        {
            _slaeAssembler.SetParameter(_parameters[i], vector[i]);
        }
    }

    private double CalculateResidual(Equation<Matrix> equation)
    {
        return Vector.Subtract(
            equation.RightPart,
            Matrix.Multiply(equation.Matrix, _bufferVector, _residualBufferVector),
            equation.RightPart)
            .Norm;
    }
}