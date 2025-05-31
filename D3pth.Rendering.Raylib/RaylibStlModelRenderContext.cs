using D3pth.Abstractions.Rendering;
using Raylib_cs;

namespace D3pth.Rendering.Raylib;

public class RaylibStlModelRenderContext : IStlModelRenderContext
{
    public Mesh Mesh { get; set; }
}
