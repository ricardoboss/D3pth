using System.Diagnostics.CodeAnalysis;

namespace D3pth.Abstractions.Models;

public interface IModelMetadata
{
    string? Name { get; }

    string? Description { get; }

    string? Color { get; }

    Plane? BasePlane { get; }

    float? Rotation { get; set; }

    float? Zoom { get; }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum Plane
{
    XY,
    XZ,
    YZ,
    NegativeXY,
    NegativeXZ,
    NegativeYZ,
}
