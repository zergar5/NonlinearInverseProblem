using DirectProblem.Core.Base;
using DirectProblem.Core.Global;
using DirectProblem.SLAE.Preconditions;

namespace DirectProblem.SLAE.Solvers;

public class MCG
{
    private readonly LLTPreconditioner _lltPreconditioner;
    private readonly LLTSparse _lltSparse;
    private SymmetricSparseMatrix _preconditionMatrix;
    private Vector _r;
    private Vector _z;

    public MCG(LLTPreconditioner lltPreconditioner, LLTSparse lltSparse)
    {
        _lltPreconditioner = lltPreconditioner;
        _lltSparse = lltSparse;
    }

    public MCG SetPrecondition(SymmetricSparseMatrix preconditionMatrix)
    {
        _preconditionMatrix = preconditionMatrix;
        //_r = new Vector(_preconditionMatrix.Count);
        //_z = new Vector(_preconditionMatrix.Count);

        return this;
    }

    private void PrepareProcess(Equation<SymmetricSparseMatrix> equation)
    {
        _preconditionMatrix = _lltPreconditioner.Decompose(_preconditionMatrix);

        _r = Vector.Subtract(equation.RightPart,
            SymmetricSparseMatrix.Multiply(equation.Matrix, equation.Solution, _r), _r);
        _z = _lltSparse.Solve(_preconditionMatrix, _r, _z);
    }

    public Vector Solve(Equation<SymmetricSparseMatrix> equation)
    {
        PrepareProcess(equation);
        IterationProcess(equation);
        return equation.Solution;
    }

    private void IterationProcess(Equation<SymmetricSparseMatrix> equation)
    {
        var x = equation.Solution;
        var bufferVector = new Vector(x.Count);

        var bNorm = equation.RightPart.Norm;
        var residual = _r.Norm / bNorm;

        for (var i = 1; i <= MethodsConfig.MaxIterations && residual > Math.Pow(MethodsConfig.Eps, 2); i++)
        {
            var scalarMrR = Vector.ScalarProduct(_lltSparse.Solve(_preconditionMatrix, _r, equation.RightPart), _r);

            var AxZ = SymmetricSparseMatrix.Multiply(equation.Matrix, _z, equation.RightPart);

            var alphaK = scalarMrR / Vector.ScalarProduct(AxZ, _z);

            Vector.Sum(x, Vector.Multiply(alphaK, _z, bufferVector), x);

            var rNext = Vector.Subtract(_r, Vector.Multiply(alphaK, AxZ, AxZ), _r);

            var betaK = Vector.ScalarProduct(_lltSparse.Solve(_preconditionMatrix, rNext, bufferVector), rNext) /
                        scalarMrR;

            var zNext = Vector.Sum(_lltSparse.Solve(_preconditionMatrix, rNext, bufferVector),
                Vector.Multiply(betaK, _z, _z), _z);

            residual = rNext.Norm / bNorm;

            _r = rNext;
            _z = zNext;

            //CourseHolder.GetInfo(i, residual);
        }

        //Console.WriteLine();
    }
}