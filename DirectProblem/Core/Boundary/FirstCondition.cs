using DirectProblem.Core.Local;

namespace DirectProblem.Core.Boundary;

public record struct FirstCondition(int ElementIndex, Bound Bound);
public record struct FirstConditionValue(LocalVector Values);