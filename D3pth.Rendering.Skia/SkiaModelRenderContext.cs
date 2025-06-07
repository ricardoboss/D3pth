using D3pth.Abstractions.Rendering;
using SkiaSharp;

namespace D3pth.Rendering.Skia;

public class SkiaModelRenderContext : IModelRenderContext
{
    public required int CanvasWidth { get; set; }

    public required int CanvasHeight { get; set; }

    public SKSurface? Surface { get; set; }
}
