using D3pth.Abstractions.Models;

namespace D3pth.Abstractions.Catalog;

public interface ICatalogGenerator
{
    ICatalog Generate(string sourceFolder, CatalogModelCollection modelCollection);
}
