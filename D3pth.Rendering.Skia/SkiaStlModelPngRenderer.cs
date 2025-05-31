using D3pth.Abstractions.Models;
using D3pth.Abstractions.Rendering;

namespace D3pth.Rendering.Skia;

public class SkiaStlModelPngRenderer(SkiaStlModelRenderer renderer) : IStlModelPngRenderer
{
    public byte[] Render(int imageWidth, int imageHeight, IStlModel stlModel, IModelMetadata modelMetadata,
        RenderOptions? options = null)
    {
        var context = new SkiaStlModelRenderContext();

        renderer.Render(imageWidth, imageHeight, stlModel, modelMetadata, context, options);

        return context.Surface!.Snapshot()!.Encode()!.ToArray()!;
    }
}
