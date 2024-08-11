namespace PrintingCatalog.Interfaces;

public interface ICatalogGenerator
{
    Task<ICatalog> GenerateAsync(string sourceFolder, CancellationToken cancellationToken = default);
}
