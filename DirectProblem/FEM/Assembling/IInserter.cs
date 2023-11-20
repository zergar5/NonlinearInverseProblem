using DirectProblem.Core.Base;
using DirectProblem.Core.Local;

namespace DirectProblem.FEM.Assembling;

public interface IInserter<in TMatrix>
{
    public void InsertMatrix(TMatrix globalMatrix, LocalMatrix localMatrix);
    public void InsertVector(Vector vector, LocalVector localVector);
}