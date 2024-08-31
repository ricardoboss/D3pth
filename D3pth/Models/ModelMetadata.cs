using D3pth.Interfaces;

namespace D3pth.Models;

public class ModelMetadata : IModelMetadata
{
    public string? Name { get; set; }

    public string? Description { get; init; }

    public string? Color { get; init; }

    public Plane? BasePlane { get; init; }

    public float? Rotation { get; init; }

    public float? Zoom { get; init; }
}