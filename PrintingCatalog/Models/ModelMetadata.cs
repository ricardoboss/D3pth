using PrintingCatalog.Interfaces;

namespace PrintingCatalog.Models;

public class ModelMetadata : IModelMetadata
{
    public required string Name { get; init; }


    public string? Description { get; init; }

    public required FileInfo SourceFile { get; init; }
}
