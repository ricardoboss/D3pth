using PrintingCatalog.Interfaces;

namespace PrintingCatalog.Models;

public class ModelMetadata : IModelMetadata
{
    public string? CatalogNumber { get; set; }

    public string? Name { get; set; }

    public string? Description { get; init; }

    public string? Category { get; init; }

    public string? Subcategory { get; init; }

    public string? Color { get; init; }

    public Plane? BasePlane { get; init; }

    public float? Rotation { get; init; }

    public float? Zoom { get; init; }
}