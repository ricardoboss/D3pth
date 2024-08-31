using D3pth.Abstractions.Models;

namespace D3pth.Abstractions.Rendering;

public interface IStlModelRenderer
{
    byte[] RenderToPng(IStlModel stlModel, RenderMode renderMode = RenderMode.Shaded,
        RenderOptions options = RenderOptions.None);
}

public enum RenderMode
{
    Shaded,
    Depth,
    Wireframe,
}

[Flags]
public enum RenderOptions
{
    None = 0,
    DrawGrid = 1,
    DrawAxes = 2,
}
