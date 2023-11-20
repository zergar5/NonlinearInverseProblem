using DirectProblem.Core.Boundary;
using DirectProblem.Core.Global;

namespace DirectProblem.FEM.Assembling.Global;

public interface IGaussExcluder<TMatrix>
{
    public void Exclude(Equation<TMatrix> equation, FirstConditionValue condition);
}