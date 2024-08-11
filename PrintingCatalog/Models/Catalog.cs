using PrintingCatalog.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PrintingCatalog.Models;

public class Catalog(IReadOnlyList<IStlModel> models, IStlModelRenderer renderer) : ICatalog, IDocument
{
    byte[] ICatalog.GeneratePdf() => this.GeneratePdf();

    public void Compose(IDocumentContainer container)
    {
        throw new NotImplementedException();
    }
}
