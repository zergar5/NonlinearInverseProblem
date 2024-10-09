using DirectProblem.Core.GridComponents;

namespace DirectProblem.TwoDimensional.Parameters;

public class MaterialsRepository
{
    private readonly Material[] _materials;

    public MaterialsRepository(Material[] materials)
    {
        _materials = materials;
    }

    public Material GetById(int id)
    {
        return _materials[id];
    }
}