namespace D3pth.Abstractions;

public interface ICatalogGenerator
{
    Task<ICatalog> GenerateAsync(string sourceFolder, CancellationToken cancellationToken = default);
}
