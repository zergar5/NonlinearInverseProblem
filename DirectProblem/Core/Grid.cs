using DirectProblem.Core.GridComponents;

namespace DirectProblem.Core;

public class Grid<TPoint>
{
    public TPoint[] Nodes { get; }
    public Element[] Elements { get; }
    public int ElementsByLength { get; }
    public int ElementsByHeight { get; }
    public Area[]? Areas { get; }

    public Grid(TPoint[] nodes, Element[] elements, int elementsByLength, int elementsByHeight)
    {
        Nodes = nodes;
        Elements = elements;
        ElementsByLength = elementsByLength;
        ElementsByHeight = elementsByHeight;
    }

    public Grid(TPoint[] nodes, Element[] elements, int elementsByLength, int elementsByHeight, Area[] areas)
        : this(nodes, elements, elementsByLength, elementsByHeight)
    {
        Areas = areas;
    }

    public IEnumerator<Element> GetEnumerator() => ((IEnumerable<Element>)Elements).GetEnumerator();
}