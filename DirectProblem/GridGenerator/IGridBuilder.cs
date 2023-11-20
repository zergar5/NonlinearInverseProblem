using DirectProblem.Core;

namespace DirectProblem.GridGenerator;

public interface IGridBuilder<TPoint>
{
    public Grid<TPoint> Build();
}