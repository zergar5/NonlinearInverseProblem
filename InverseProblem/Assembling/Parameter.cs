namespace InverseProblem.Assembling;

public enum ParameterType
{
    Current,
    Sigma,
    VerticalBound,
    HorizontalBound
}

public readonly record struct Parameter(ParameterType ParameterType, int Index);