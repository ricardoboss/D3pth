namespace PrintingCatalog.Interfaces;

public interface IModelMetadata
{
    string? Name { get; }

    string? Description { get; }

    string? Color { get; }
}
