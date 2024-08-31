namespace D3pth.Abstractions.Catalog;

public interface ICatalogGenerator
{
    Task<ICatalog> GenerateAsync(string sourceFolder, CancellationToken cancellationToken = default);
}
