namespace PrintingCatalog.Interfaces;

public interface IStlModelRenderer
{
    Task<byte[]> RenderToPngAsync(IStlModel stlModel, CancellationToken cancellationToken = default);
}
