using D3pth.Abstractions.Models;

namespace D3pth.Abstractions.Rendering;

public interface IStlModelRenderer
{
    byte[] RenderToPng(int imageWidth, int imageHeight, IStlModel stlModel, IModelMetadata modelMetadata, RenderMode renderMode = RenderMode.Shaded,
        RenderOptions? options = null);
}

public enum RenderMode
{
    Shaded,
    Depth,
    Wireframe,
}

public class RenderOptions
{
    public static readonly RenderOptions None = new();

    public bool DrawGrid { get; set; } = false;

    public bool DrawAxes { get; set; } = false;

    public int TesselationLevel { get; set; } = 0;
}
