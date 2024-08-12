using PrintingCatalog.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PrintingCatalog.Models;

public class Catalog(IReadOnlyList<IStlModel> models, IStlModelRenderer renderer, DirectoryInfo baseDirectory)
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
            c.Item().Text("Ricardo's großer 3D-Drucker Katalog").FontSize(20).AlignCenter();
            c.Item().Text("Sortiert nach Name");
            c.Item().Table(ComposePreviewTable);
        });
    }

    private void ComposePreviewTable(TableDescriptor table)
    {
        table.ColumnsDefinition(c =>
        {
            c.ConstantColumn(50);
            c.ConstantColumn(250);
            c.ConstantColumn(100);
            c.RelativeColumn();
        });

        table.Header(c =>
        {
            c.Cell().Border(1).Padding(5).Text("Best. #");
            c.Cell().Border(1).Padding(5).Text("Vorschau");
            c.Cell().Border(1).Padding(5).Text("Name");
            c.Cell().Border(1).Padding(5).Text("Beschreibung");
        });

        foreach (var model in models.DistinctBy(m => m.Metadata.CatalogNumber).OrderBy(m => m.Metadata.Name))
        {
            table.Cell().Border(1).Padding(5).Text(model.Metadata.CatalogNumber);

            var imageCell = table.Cell().Border(1).Padding(5);
            try
            {
                var image = renderer.RenderToPng(model);

                imageCell.Image(image);
            }
            catch (Exception e)
            {
                imageCell.AspectRatio(1).Text($"Error: {e.Message}");
            }

            table.Cell().Border(1).Padding(5).Text(model.Metadata.Name);
            table.Cell().Border(1).Padding(5).Text(model.Metadata.Description);
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
            c.ConstantColumn(50);
            c.ConstantColumn(200);
            c.RelativeColumn();
        });

        table.Header(c =>
        {
            c.Cell().Border(1).Padding(5).Text("Best. #");
            c.Cell().Border(1).Padding(5).Text("Name");
            c.Cell().Border(1).Padding(5).Text("Datei");
        });

        foreach (var model in models.DistinctBy(m => m.Metadata.CatalogNumber).OrderBy(m => m.Metadata.CatalogNumber))
        {
            var relativePath = Path.GetRelativePath(baseDirectory.FullName, model.File.FullName);

            table.Cell().Padding(5).Text(model.Metadata.CatalogNumber);
            table.Cell().Padding(5).Text(model.Metadata.Name);
            table.Cell().Padding(5).Text(relativePath).FontSize(10);
        }
    }
}