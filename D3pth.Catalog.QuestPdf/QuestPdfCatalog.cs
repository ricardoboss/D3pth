using D3pth.Abstractions.Catalog;
using D3pth.Abstractions.Models;
using D3pth.Abstractions.Rendering;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace D3pth.Catalog.QuestPdf;

internal sealed class QuestPdfCatalog(CatalogModelCollection models, IStlModelRenderer renderer, DirectoryInfo baseDirectory)
    : ICatalog, IDocument
{
    byte[] ICatalog.GeneratePdf() => this.GeneratePdf();

    public void Compose(IDocumentContainer container) => container
        .Page(ComposeCatalogPage)
        .Page(ComposeAlphabeticalPage);

    private void ComposeCatalogPage(PageDescriptor page)
    {
        page.Margin(16);
        page.ContinuousSize(210, Unit.Millimetre);
        page.Content().Column(c =>
        {
            c.Spacing(16);
            c.Item().Text("Ricardos großer 3D-Druck Katalog").FontSize(20).AlignCenter();
            c.Item().Text("Sortiert nach Name");
            c.Item().Table(ComposePreviewTable);
        });
    }

    private void ComposePreviewTable(TableDescriptor table)
    {
        table.ColumnsDefinition(c =>
        {
            c.ConstantColumn(40);
            c.ConstantColumn(250);
            c.ConstantColumn(100);
            c.RelativeColumn();
        });

        table.Header(c =>
        {
            c.Cell().Border(1).Padding(5).Text("Nr.");
            c.Cell().Border(1).Padding(5).Text("Vorschau");
            c.Cell().Border(1).Padding(5).Text("Name");
            c.Cell().Border(1).Padding(5).Text("Beschreibung");
        });

        foreach (var (model, metadata) in models.OrderBy(m => m.Metadata.Name))
        {
            table.Cell().Border(1).Padding(5).Text(model.Md5Hash[..4]);

            var imageCell = table.Cell().Border(1).Padding(5);
            try
            {
                var image = renderer.RenderToPng(1024, 1024, model, metadata);

                imageCell.Image(image);
            }
            catch (Exception e)
            {
                imageCell.AspectRatio(1).Text($"Error: {e.Message}");
            }

            table.Cell().Border(1).Padding(5).Text(metadata.Name);
            table.Cell().Border(1).Padding(5).Column(c =>
            {
                c.Spacing(5);
                c.Item().Text(metadata.Description);

                if (metadata.Color is null)
                    c.Item().Text("Bitte gewünschte Farbe angeben!").FontColor(Colors.Red.Medium);
            });
        }
    }

    private void ComposeAlphabeticalPage(PageDescriptor page)
    {
        page.Margin(16);
        page.ContinuousSize(210, Unit.Millimetre);
        page.Content().Column(c =>
        {
            c.Spacing(16);
            c.Item().Text("Sortiert nach Katalognummer");
            c.Item().Table(ComposeSortedTable);
            c.Item().Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss} - Base Directory: {baseDirectory.FullName}")
                .FontSize(8);
        });
    }

    private void ComposeSortedTable(TableDescriptor table)
    {
        table.ColumnsDefinition(c =>
        {
            c.ConstantColumn(40);
            c.ConstantColumn(200);
            c.RelativeColumn();
        });

        table.Header(c =>
        {
            c.Cell().Border(1).Padding(5).Text("Nr.");
            c.Cell().Border(1).Padding(5).Text("Name");
            c.Cell().Border(1).Padding(5).Text("Datei");
        });

        foreach (var (model, metadata) in models.OrderBy(e => e.Model.Md5Hash))
        {
            var relativePath = Path.GetRelativePath(baseDirectory.FullName, model.File.FullName);

            table.Cell().Padding(5).Text(model.Md5Hash[..4]);
            table.Cell().Padding(5).Text(metadata.Name);
            table.Cell().Padding(5).Text(relativePath).FontSize(10);
        }
    }
}
