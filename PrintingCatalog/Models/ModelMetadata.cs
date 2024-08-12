using PrintingCatalog.Interfaces;

namespace PrintingCatalog.Models;

public class ModelMetadata : IModelMetadata
{
    public string? Name { get; init; }

    public string? Description { get; init; }

    public string? Color { get; init; }
}
