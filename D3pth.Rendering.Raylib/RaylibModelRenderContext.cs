using D3pth.Abstractions.Rendering;
using Raylib_cs;

namespace D3pth.Rendering.Raylib;

public class RaylibModelRenderContext : IModelRenderContext
{
    public required int CanvasWidth { get; set; }

    public required int CanvasHeight { get; set; }

    public Model Model { get; set; }
}
