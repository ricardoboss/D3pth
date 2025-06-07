using D3pth.Abstractions.Models;
using D3pth.Abstractions.Rendering;
using Raylib_cs;

namespace D3pth.Rendering.Raylib;

public class RaylibModelRenderer : IModelRenderer
{
    public void Render<TContext>(TContext context, IStlModel stlModel, IModelMetadata modelMetadata,
        RenderOptions? options = null) where TContext : IModelRenderContext
    {
        if (context is not RaylibModelRenderContext raylibContext)
            throw new ArgumentException("Invalid context", nameof(context));

        var model = Raylib_cs.Raylib.LoadModel(stlModel.File.FullName);

        raylibContext.Model = model;
    }
}
