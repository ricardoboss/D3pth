using PrintingCatalog.Interfaces;

namespace PrintingCatalog.Models;

public class ModelMetadata : IModelMetadata
{
    public uint? CatalogNumber { get; set; }

    public string? Name { get; set; }

    public string? Description { get; init; }

    public string? Color { get; init; }
}
