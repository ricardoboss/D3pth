using PrintingCatalog.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PrintingCatalog.Models;

public class Catalog(IReadOnlyList<IStlModel> models, IStlModelRenderer renderer) : ICatalog, IDocument
{
    byte[] ICatalog.GeneratePdf() => this.GeneratePdf();

    public void Compose(IDocumentContainer container) => container.Page(ComposePage);

    private void ComposePage(PageDescriptor page)
    {
        page.Margin(4);
        page.ContinuousSize(210, Unit.Millimetre);
        page.Content().Table(ComposeTable);
    }

    private void ComposeTable(TableDescriptor table)
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

        foreach (var model in models.DistinctBy(m => m.Metadata.CatalogNumber).OrderBy(m => m.Metadata.CatalogNumber))
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
}