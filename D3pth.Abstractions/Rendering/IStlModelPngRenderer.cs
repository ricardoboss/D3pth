using D3pth.Abstractions.Models;

namespace D3pth.Abstractions.Rendering;

public interface IStlModelPngRenderer
{
    byte[] Render(int imageWidth, int imageHeight, IStlModel stlModel, IModelMetadata modelMetadata,
        RenderOptions? options = null);
}
