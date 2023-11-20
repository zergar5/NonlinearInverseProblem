using DirectProblem.Core.Base;

namespace DirectProblem.Core.Global;

public record Equation<TMatrix>(TMatrix Matrix, Vector Solution, Vector RightPart);