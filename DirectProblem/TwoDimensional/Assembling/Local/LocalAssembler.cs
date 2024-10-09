using DirectProblem.Core.Base;
using DirectProblem.Core.GridComponents;
using DirectProblem.Core.Local;
using DirectProblem.FEM.Assembling.Local;
using DirectProblem.TwoDimensional.Parameters;

namespace DirectProblem.TwoDimensional.Assembling.Local;

public class LocalAssembler : ILocalAssembler
{
    private readonly ILocalMatrixAssembler _localMatrixAssembler;
    private Material[] _materials;

    public LocalAssembler
    (
        ILocalMatrixAssembler localMatrixAssembler,
        Material[] materials
    )
    {
        _localMatrixAssembler = localMatrixAssembler;
        _materials = materials;
    }

    public LocalAssembler SetMaterials(Material[] materials)
    {
        _materials = materials;

        return this;
    }

    public LocalMatrix AssembleMatrix(Element element)
    {
        var matrix = GetStiffnessMatrix(element);
        var sigma = _materials[element.MaterialId].Sigma;

        return new LocalMatrix(element.NodesIndexes, Matrix.Multiply(sigma, matrix, matrix));
    }

    private Matrix GetStiffnessMatrix(Element element)
    {
        var stiffness = _localMatrixAssembler.AssembleStiffnessMatrix(element);

        return stiffness;
    }
}