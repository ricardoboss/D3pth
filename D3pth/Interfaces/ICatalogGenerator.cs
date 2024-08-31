namespace D3pth.Interfaces;

public interface ICatalogGenerator
{
    Task<ICatalog> GenerateAsync(string sourceFolder, CancellationToken cancellationToken = default);
}
