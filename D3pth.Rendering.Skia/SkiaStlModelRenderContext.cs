using D3pth.Abstractions.Rendering;
using SkiaSharp;

namespace D3pth.Rendering.Skia;

public class SkiaStlModelRenderContext : IStlModelRenderContext
{
    public SKSurface? Surface { get; set; }
}
