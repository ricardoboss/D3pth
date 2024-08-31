using System.Runtime.CompilerServices;
using D3pth.Abstractions.Catalog;
using D3pth.Abstractions.Models;
using D3pth.Abstractions.Rendering;
using D3pth.Abstractions.Services;
using Microsoft.Extensions.Logging;
using QuestPDF.Infrastructure;

namespace D3pth.Catalog.QuestPdf;

public sealed class QuestPdfCatalogGenerator(
    IFileDiscoverer fileDiscoverer,
    IStlModelLoader stlModelLoader,
    IStlModelRenderer stlModelRenderer,
    ILogger<QuestPdfCatalogGenerator> logger
) : ICatalogGenerator
{
    public async Task<ICatalog> GenerateAsync(string sourceFolder, CancellationToken cancellationToken = default)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var models = await LoadModelsAsync(sourceFolder, cancellationToken).ToListAsync(cancellationToken);
        var sourceDir = new DirectoryInfo(sourceFolder);

        return new QuestPdfCatalog(models, stlModelRenderer, sourceDir);
    }

    private async IAsyncEnumerable<IStlModel> LoadModelsAsync(string sourceFolder,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var modelFile in fileDiscoverer.Discover(sourceFolder))
        {
            IStlModel? model = null;
            try
            {
                model = await stlModelLoader.LoadAsync(modelFile, cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error loading '{modelFile.FullName}': {e.Message}");
            }

            if (model is not null)
                yield return model;
        }
    }
}
