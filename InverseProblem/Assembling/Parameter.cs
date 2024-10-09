namespace InverseProblem.Assembling;

public enum ParameterType
{
    Current,
    Sigma,
    Position
}

public readonly record struct Parameter(ParameterType ParameterType, int Index);