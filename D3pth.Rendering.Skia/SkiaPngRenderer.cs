using D3pth.Abstractions.Models;
using D3pth.Abstractions.Rendering;

namespace D3pth.Rendering.Skia;

public class SkiaPngRenderer(SkiaModelRenderer renderer) : IPngRenderer
{
    public byte[] Render(int imageWidth, int imageHeight, IStlModel stlModel, IModelMetadata modelMetadata,
        RenderOptions? options = null)
    {
        var context = new SkiaModelRenderContext
        {
            CanvasWidth = imageWidth,
            CanvasHeight = imageHeight,
        };

        renderer.Render(context, stlModel, modelMetadata, options);

        return context.Surface!.Snapshot()!.Encode()!.ToArray()!;
    }
}
