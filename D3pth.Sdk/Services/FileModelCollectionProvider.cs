using System.Runtime.CompilerServices;
using D3pth.Abstractions.Models;
using D3pth.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace D3pth.Sdk.Services;

public class FileModelCollectionProvider(
    IFileDiscoverer fileDiscoverer,
    IStlModelLoader stlModelLoader,
    IModelMetadataLoader metadataLoader,
    ILogger<FileModelCollectionProvider> logger
) : IModelCollectionProvider
{
    public async Task<CatalogModelCollection> LoadAsync(string sourceFolder,
        CancellationToken cancellationToken = default)
    {
        var entries = await LoadModelsAsync(sourceFolder, cancellationToken).ToListAsync(cancellationToken);

        return new(entries);
    }

    private async IAsyncEnumerable<CatalogEntry> LoadModelsAsync(string sourceFolder,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var modelFile in fileDiscoverer.Discover(sourceFolder))
        {
            IStlModel model;
            try
            {
                model = await stlModelLoader.LoadAsync(modelFile, cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error loading '{ModelFile}': {Message}", modelFile.FullName, e.Message);

                continue;
            }

            IModelMetadata metadata;
            try
            {
                metadata = await metadataLoader.LoadAsync(modelFile, cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error loading metadata for model '{ModelFile}': {Message}", modelFile.FullName, e.Message);

                continue;
            }

            yield return new(model, metadata);
        }
    }
}
