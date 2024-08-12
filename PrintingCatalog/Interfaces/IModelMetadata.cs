using System.Diagnostics.CodeAnalysis;

namespace PrintingCatalog.Interfaces;

public interface IModelMetadata
{
    string? CatalogNumber { get; }

    string? Name { get; }

    string? Description { get; }

    string? Category { get; }

    string? Subcategory { get; }

    string? Color { get; }

    Plane? BasePlane { get; }

    float? Rotation { get; }

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
