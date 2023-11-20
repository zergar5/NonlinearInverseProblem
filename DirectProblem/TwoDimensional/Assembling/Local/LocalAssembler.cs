using DirectProblem.Core.Base;
using DirectProblem.Core.GridComponents;
using DirectProblem.Core.Local;
using DirectProblem.FEM.Assembling.Local;
using DirectProblem.TwoDimensional.Parameters;

namespace DirectProblem.TwoDimensional.Assembling.Local;

public class LocalAssembler : ILocalAssembler
{
    private readonly ILocalMatrixAssembler _localMatrixAssembler;
    private readonly MaterialsRepository _materialFactory;

    public LocalAssembler
    (
        ILocalMatrixAssembler localMatrixAssembler,
        MaterialsRepository materialFactory
    )
    {
        _localMatrixAssembler = localMatrixAssembler;
        _materialFactory = materialFactory;
    }

    public LocalMatrix AssembleMatrix(Element element)
    {
        var matrix = GetStiffnessMatrix(element);
        var sigma = _materialFactory.GetById(element.MaterialId).Sigma;

        return new LocalMatrix(element.NodesIndexes, Matrix.Multiply(sigma, matrix, matrix));
    }

    private Matrix GetStiffnessMatrix(Element element)
    {
        var stiffness = _localMatrixAssembler.AssembleStiffnessMatrix(element);

        return stiffness;
    }
}