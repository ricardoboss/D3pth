namespace PrintingCatalog.Interfaces;

public interface IStlModelRenderer
{
    byte[] RenderToPng(IStlModel stlModel, RenderMode renderMode = RenderMode.Shaded);
}

public enum RenderMode
{
    Shaded,
    Depth,
    Wireframe,
}
