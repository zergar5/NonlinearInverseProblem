using DirectProblem.Core.GridComponents;

namespace InverseProblem.Assembling;

public class ParametersCollection
{
    public double SourcePower { get; private set; }
    public Material[] Materials { get; }

    public ParametersCollection(double sourcePower, Material[] materials)
    {
        SourcePower = sourcePower;
        Materials = materials;
    }

    public double GetParameterValue(Parameter parameter)
    {
        return parameter.ParameterType switch
        {
            ParameterType.Current => SourcePower,
            ParameterType.Sigma => Materials[parameter.Index].Sigma,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void SetParameterValue(Parameter parameter, double value)
    {
        switch (parameter.ParameterType)
        {
            case ParameterType.Current:
                SourcePower = value;
                break;
            case ParameterType.Sigma:
                Materials[parameter.Index].Sigma = value;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}