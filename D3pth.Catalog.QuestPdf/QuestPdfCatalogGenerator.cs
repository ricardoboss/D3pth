using System.Diagnostics.CodeAnalysis;
using D3pth.Abstractions.Catalog;
using D3pth.Abstractions.Models;
using D3pth.Abstractions.Rendering;
using QuestPDF;
using QuestPDF.Infrastructure;

namespace D3pth.Catalog.QuestPdf;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public sealed class QuestPdfCatalogGenerator(IPngRenderer pngRenderer) : ICatalogGenerator
{
    public ICatalog Generate(string sourceFolder, CatalogModelCollection modelCollection)
    {
        Settings.License = LicenseType.Community;

        var sourceDir = new DirectoryInfo(sourceFolder);

        return new QuestPdfCatalog(modelCollection, pngRenderer, sourceDir);
    }
}
