namespace PrintingCatalog.Interfaces;

public interface IStlModelRenderer
{
    byte[] RenderToPng(IStlModel stlModel);
}
