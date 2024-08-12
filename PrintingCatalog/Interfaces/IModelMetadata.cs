namespace PrintingCatalog.Interfaces;

public interface IModelMetadata
{
    uint? CatalogNumber { get; }

    string? Name { get; }

    string? Description { get; }

    string? Color { get; }
}
