using D3pth.Abstractions.Models;

namespace D3pth.Sdk.Models;

public class ModelMetadata : IModelMetadata
{
    public string? Name { get; set; }

    public string? Description { get; init; }

    public string? Color { get; init; }

    public Plane? BasePlane { get; init; }

    public float? Rotation { get; set; }

    public float? Zoom { get; init; }
}
