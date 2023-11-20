using DirectProblem.Core;
using DirectProblem.Core.GridComponents;

namespace DirectProblem.FEM.Assembling;

public interface IMatrixPortraitBuilder<out TMatrix>
{
    TMatrix Build(Grid<Node2D> grid);
}