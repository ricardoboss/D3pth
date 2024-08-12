using System.Runtime.CompilerServices;
using PrintingCatalog.Interfaces;
using PrintingCatalog.Models;
using QuestPDF.Infrastructure;

namespace PrintingCatalog.Services;

internal sealed class QuestPdfCatalogGenerator(
    IFileDiscoverer fileDiscoverer,
    IStlModelLoader stlModelLoader,
    IStlModelRenderer stlModelRenderer
) : ICatalogGenerator
{
    public async Task<ICatalog> GenerateAsync(string sourceFolder, CancellationToken cancellationToken = default)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var models = await LoadModelsAsync(sourceFolder, cancellationToken).ToListAsync(cancellationToken);
        var sourceDir = new DirectoryInfo(sourceFolder);

        return new Catalog(models, stlModelRenderer, sourceDir);
    }

    private async IAsyncEnumerable<IStlModel> LoadModelsAsync(string sourceFolder,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var modelFile in fileDiscoverer.DiscoverAsync(sourceFolder, extensions: "stl").WithCancellation(cancellationToken))
            yield return await stlModelLoader.LoadAsync(modelFile, cancellationToken);
    }
}
