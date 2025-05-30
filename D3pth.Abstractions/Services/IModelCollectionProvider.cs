using D3pth.Abstractions.Models;

namespace D3pth.Abstractions.Services;

public interface IModelCollectionProvider
{
    Task<CatalogModelCollection> LoadAsync(string sourceFolder, CancellationToken cancellationToken = default);
}
