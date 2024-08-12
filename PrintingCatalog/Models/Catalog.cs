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
            c.ConstantColumn(200);
            c.ConstantColumn(100);
            c.RelativeColumn();
        });

        foreach (var model in models)
        {
            table.Cell().Border(1).Padding(5)
                .Text($"#{(model.Metadata.CatalogNumber?.ToString() ?? "").PadLeft(4, '0')}");
            table.Cell().Border(1).Padding(5).Image(renderer.RenderToPng(model));
            table.Cell().Border(1).Padding(5).Text(model.Metadata.Name);
            table.Cell().Border(1).Padding(5).Text(model.Metadata.Description);
        }
    }
}