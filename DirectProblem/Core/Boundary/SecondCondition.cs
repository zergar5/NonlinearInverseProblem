using DirectProblem.Core.Local;

namespace DirectProblem.Core.Boundary;

public record struct SecondCondition(int ElementIndex, Bound Bound);
public record struct SecondConditionValue(LocalVector Values);