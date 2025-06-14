using D3pth.Abstractions.Models;

namespace D3pth.Abstractions.Rendering;

public interface IModelRenderer
{
    void Render<TContext>(TContext context, IStlModel stlModel, IModelMetadata modelMetadata,
        RenderOptions? options = null) where TContext : IModelRenderContext;
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

    public int TesselationLevel { get; set; } = 5;

    public RenderMode Mode { get; set; } = RenderMode.Shaded;
}
