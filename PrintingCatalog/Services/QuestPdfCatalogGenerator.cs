using System.Runtime.CompilerServices;
using PrintingCatalog.Interfaces;
using PrintingCatalog.Models;
using QuestPDF.Infrastructure;
using Spectre.Console;

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
        foreach (var modelFile in fileDiscoverer.Discover(sourceFolder))
        {
            IStlModel? model = null;
            try
            {
                model = await stlModelLoader.LoadAsync(modelFile, cancellationToken);
            }
            catch (Exception e)
            {
                AnsiConsole.MarkupLine($"[red]Error loading '[bold]{modelFile.FullName}[/]': {e.Message}[/]");
            }

            if (model is not null)
                yield return model;
        }
    }
}
