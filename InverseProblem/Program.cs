using DirectProblem;
using DirectProblem.Core.GridComponents;
using DirectProblem.GridGenerator;
using DirectProblem.TwoDimensional;
using DirectProblem.TwoDimensional.Assembling.Local;
using InverseProblem;
using InverseProblem.Assembling;
using InverseProblem.SLAE;
using System.Diagnostics;
using System.Globalization;
using DirectProblem.GridGenerator.Intervals.Splitting;
using Vector = DirectProblem.Core.Base.Vector;

Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

var gridBuilder = new GridBuilder2D();

var trueGrid = gridBuilder
    .SetRAxis(new AxisSplitParameter(
            [1e-4, 10d, 1000d],
            new StepProportionalSplitter(1d, 1.05), 
            new StepProportionalSplitter(2d, 1.05)
        )
    )
    .SetZAxis(new AxisSplitParameter(
            [-1000d, -750d, -500d, -250d, 0d],
        new StepProportionalSplitter(30d, 1 / 1.05),
            new StepProportionalSplitter(20d, 1 / 1.05),
            new StepProportionalSplitter(6d, 1 / 1.05),
            new StepProportionalSplitter(1d, 1 / 1.05)
        )
    )
    .SetAreas(new Area[]
    {
        new(0, new Node2D(1e-4, -250d), new Node2D(1000d, 0d)),
        new(1, new Node2D(1e-4, -500d), new Node2D(1000d, -250d)),
        new(2, new Node2D(1e-4, -750d), new Node2D(1000d, -500d)),
        new(3, new Node2D(1e-4, -1000d), new Node2D(1000d, -750d)),
    })
    .Build();

var trueCurrent = 5d;

var trueMaterials = new Material[]
{
    new(0.001),
    new(0.1),
    new(0.5),
    new(0.01),
};

var source = new Source(new Node2D(1e-4d, 0d), new Node2D(10d, 0d), trueCurrent);
var receivers = new Node2D[2];
var trueFieldValues = new double[receivers.Length];
var noises = new double[receivers.Length];
receivers[0] = new Node2D(30d, 0d);
receivers[1] = new Node2D(60d, 0d);
//receivers[2] = new Node2D(100d, 0d);
noises[0] = 0.99d;
noises[1] = 1d;
//noises[2] = 1d;

var maxThreads = 1;

var targetParameters = new Parameter[]
{
    //new (ParameterType.Current, 0), 
    new (ParameterType.Sigma, 2),
};

DirectProblemSolver[] directProblemSolvers;
LocalBasisFunctionsProvider[] localBasisFunctionsProviders;

if (targetParameters.Length < maxThreads)
{
    directProblemSolvers = new DirectProblemSolver[targetParameters.Length];
    localBasisFunctionsProviders = new LocalBasisFunctionsProvider[targetParameters.Length];
}
else
{
    directProblemSolvers = new DirectProblemSolver[maxThreads];
    localBasisFunctionsProviders = new LocalBasisFunctionsProvider[maxThreads];
}

for (var i = 0; i < directProblemSolvers.Length; i++)
{
    directProblemSolvers[i] = new DirectProblemSolver(trueGrid, trueMaterials)
        .SetGrid(trueGrid).SetMaterials(trueMaterials);
    localBasisFunctionsProviders[i] = new LocalBasisFunctionsProvider(trueGrid);
}

//var resultO = new ResultIO("../InverseProblem/Results/OneSigma/");
//var gridO = new GridIO("../InverseProblem/Results/8OtherSigmasCloseAndNearToWell/");

var solution = directProblemSolvers[0]
    .SetSource(source)
    .AssembleSLAE()
    .Solve();

for (var i = 0; i < receivers.Length; i++)
{
    var femSolution = new FEMSolution(trueGrid, solution, localBasisFunctionsProviders[0]);

    trueFieldValues[i] = femSolution.Calculate(receivers[i]) * noises[i];

    Console.Write($"receiver {i}                                                      \r");
}

//resultO.WriteInverseProblemIteration(receiverLines, truePhaseDifferences, frequencies, "true phase differences.txt");
//gridO.WriteAreas(trueGrid, trueValues, "true areas.txt");

Console.WriteLine();
Console.WriteLine("TrueDirectProblem calculated");

var parametersCollections = new ParametersCollection[directProblemSolvers.Length];

for (var i = 0; i < parametersCollections.Length; i++)
{
    var materials = new Material[]
    {
        new(0.001),
        new(0.1),
        new(0.1),
        new(0.01),
    };

    parametersCollections[i] = new ParametersCollection(5, materials);
}

var initialValues = new Vector([0.1]);

var slaeAssembler = new SLAEAssembler(directProblemSolvers, localBasisFunctionsProviders,
    parametersCollections, source, receivers, targetParameters, initialValues,
    trueFieldValues);

var gaussElimination = new GaussElimination();

var regularizer = new Regularizer(gaussElimination, targetParameters);

var inverseProblemSolver = new InverseProblemSolver(directProblemSolvers, slaeAssembler, regularizer,
    gaussElimination, localBasisFunctionsProviders, trueGrid, parametersCollections, source, receivers,
    targetParameters, trueFieldValues, initialValues);

inverseProblemSolver.Solve();