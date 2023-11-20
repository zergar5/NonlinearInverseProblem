using DirectProblem;
using DirectProblem.Core.GridComponents;
using DirectProblem.GridGenerator;
using DirectProblem.GridGenerator.Intervals.Splitting;
using DirectProblem.TwoDimensional.Assembling.Boundary;
using System.Globalization;
using DirectProblem.Core.Base;
using DirectProblem.FEM;
using InverseProblem;
using InverseProblem.Assembling;
using DirectProblem.TwoDimensional.Assembling.Local;
using DirectProblem.TwoDimensional;
using Microsoft.VisualBasic;

Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

//24800
var rBorder = 24800d;
//-150
var zBorder = -150d;

var trueRPoints = new[] { 0d, rBorder };
var trueZPoints = new[] { zBorder, -20d, 0d };

var trueAreas = new Area[]
{
    new(0, new Node2D(trueRPoints[0], trueZPoints[^2]),
        new Node2D(trueRPoints[1], trueZPoints[^1])),
    new(1, new Node2D(trueRPoints[0], trueZPoints[^3]),
        new Node2D(trueRPoints[1], trueZPoints[^2]))
};

var gridBuilder2D = new GridBuilder2D();
var trueGrid = gridBuilder2D
    .SetRAxis(new AxisSplitParameter(
            trueRPoints,
            new UniformSplitter(2480)
        )
    )
    .SetZAxis(new AxisSplitParameter(
            trueZPoints,
            new UniformSplitter(20),
            new UniformSplitter(130)
        )
    )
    .SetAreas(trueAreas)
    .Build();

var trueSigmas = new[] { 0.01d, 0.1d };

var trueSourcesLine = new SourcesLine(new Node2D(0d, 0d), new Node2D(100d, 0d), 1d);

var receiversLines = new ReceiversLine[]
{
    new(new Node2D(200d, 0d), new Node2D(300d, 0d)),
    new(new Node2D(500d, 0d), new Node2D(600d, 0d)),
    new(new Node2D(1000d, 0d), new Node2D(1100d, 0d))
};

var truePotentialDifferences = new double[receiversLines.Length];

var firstBoundaryProvider = new FirstBoundaryProvider(trueGrid);

var firstConditions =
    firstBoundaryProvider.GetConditions(trueGrid.ElementsByLength, trueGrid.ElementsByHeight);

var directProblemSolver = new DirectProblemSolver();
var solution = directProblemSolver
    .SetGrid(trueGrid)
    .SetMaterials(trueSigmas)
    .SetSource(trueSourcesLine)
    .SetFirstConditions(firstConditions)
    .Solve();

var femSolution = new FEMSolution(trueGrid, solution,
    new LocalBasisFunctionsProvider(trueGrid, new LinearFunctionsProvider()));

//var dstanceSourceM = Node2D.Distance(trueSourcesLine.PointA, receiversLines[0].PointM);
//var dstanceSourceN = Node2D.Distance(trueSourcesLine.PointA, receiversLines[0].PointN);

//var ptentialM = femSolution.CalculatePotential(new Node2D(dstanceSourceM, 0));
//var ptentialN = femSolution.CalculatePotential(new Node2D(dstanceSourceN, 0));

//var newptentialM = ptentialM;
//var newptentialN = ptentialN;

//do
//{
//    ptentialM = newptentialM;
//    ptentialN = newptentialN;

//    zBorder -= 10;

//    trueRPoints = new[] { 0d, rBorder };
//    trueZPoints = new[] { zBorder, -20d, 0d };

//    trueAreas = new Area[]
//    {
//    new(0, new Node2D(trueRPoints[0], trueZPoints[^2]),
//        new Node2D(trueRPoints[1], trueZPoints[^1])),
//    new(1, new Node2D(trueRPoints[0], trueZPoints[^3]),
//        new Node2D(trueRPoints[1], trueZPoints[^2]))
//    };

//    gridBuilder2D = new GridBuilder2D();
//    trueGrid = gridBuilder2D
//        .SetRAxis(new AxisSplitParameter(
//                trueRPoints,
//                new UniformSplitter(200)
//            )
//        )
//        .SetZAxis(new AxisSplitParameter(
//                trueZPoints,
//                new UniformSplitter(20),
//                new UniformSplitter(200)
//            )
//        )
//        .SetAreas(trueAreas)
//        .Build();

//    directProblemSolver = new DirectProblemSolver();
//    solution = directProblemSolver
//        .SetGrid(trueGrid)
//        .SetMaterials(trueSigmas)
//        .SetSource(trueSourcesLine)
//        .SetFirstConditions(firstConditions)
//        .Solve();

//    femSolution = new FEMSolution(trueGrid, solution,
//        new LocalBasisFunctionsProvider(trueGrid, new LinearFunctionsProvider()));

//    dstanceSourceM = Node2D.Distance(trueSourcesLine.PointA, receiversLines[0].PointM);
//    dstanceSourceN = Node2D.Distance(trueSourcesLine.PointA, receiversLines[0].PointN);

//    newptentialM = femSolution.CalculatePotential(new Node2D(dstanceSourceM, 0));
//    newptentialN = femSolution.CalculatePotential(new Node2D(dstanceSourceN, 0));

//    Console.Write($" Border: {zBorder}, potential: {newptentialM}                                   \r");

//} while (true);

//return 0;

for (var i = 0; i < receiversLines.Length; i++)
{
    var distanceSourceM = Node2D.Distance(trueSourcesLine.PointA, receiversLines[i].PointM);
    var distanceSourceN = Node2D.Distance(trueSourcesLine.PointA, receiversLines[i].PointN);

    var potentialM = femSolution.CalculatePotential(new Node2D(distanceSourceM, 0));
    var potentialN = femSolution.CalculatePotential(new Node2D(distanceSourceN, 0));

    var potentialDifferenceAMN = potentialM - potentialN;

    distanceSourceM = Node2D.Distance(trueSourcesLine.PointB, receiversLines[i].PointM);
    distanceSourceN = Node2D.Distance(trueSourcesLine.PointB, receiversLines[i].PointN);

    potentialM = -femSolution.CalculatePotential(new Node2D(distanceSourceM, 0));
    potentialN = -femSolution.CalculatePotential(new Node2D(distanceSourceN, 0));

    var potentialDifferenceBMN = potentialM - potentialN;

    truePotentialDifferences[i] = potentialDifferenceAMN + potentialDifferenceBMN;
}

var rPoints = new[] { 0d, rBorder };
var zPoints = new[] { zBorder, -20d, 0d };

var areas = new Area[]
{
    new(0, new Node2D(rPoints[0], zPoints[^2]),
        new Node2D(rPoints[1], zPoints[^1])),
    new(1, new Node2D(rPoints[0], zPoints[^3]),
        new Node2D(rPoints[1], zPoints[^2]))
};

var sigmas = new[] { 0.1d, 0.1d };

var source = new SourcesLine(new Node2D(0d, 0d), new Node2D(100d, 0d), 1d);

var targetParameters = new Parameter[]
{
    new (ParameterType.HorizontalBound, 1),
    new (ParameterType.Sigma, 0),
    //new (ParameterType.Sigma, 1),
};

var trueValues = new Vector(new[] { -20d, 0.01d });
var initialValues = new Vector(new[] { -20d, 0.1d });

var inverseProblemSolver = new InverseProblemSolver(gridBuilder2D);

solution = inverseProblemSolver
    .SetSource(source)
    .SetReceivers(receiversLines)
    .SetParameters(targetParameters, trueValues, initialValues)
    .SetTruePotentialDifferences(truePotentialDifferences)
    .SetInitialDirectProblemParameters(rPoints, zPoints, areas, sigmas, firstConditions)
    .Solve();

foreach (var value in solution)
{
    Console.WriteLine(value);
}