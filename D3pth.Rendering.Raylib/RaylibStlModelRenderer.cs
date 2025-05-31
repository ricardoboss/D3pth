using D3pth.Abstractions.Models;
using D3pth.Abstractions.Rendering;
using Raylib_cs;

namespace D3pth.Rendering.Raylib;

public class RaylibStlModelRenderer : IStlModelRenderer
{
    public void Render<TContext>(int imageWidth, int imageHeight, IStlModel stlModel, IModelMetadata modelMetadata,
        TContext context, RenderOptions? options = null) where TContext : IStlModelRenderContext
    {
        if (context is not RaylibStlModelRenderContext raylibContext)
            throw new ArgumentException("Invalid context", nameof(context));

        var mesh = new Mesh();
        // mesh.VertexCount = vertices.Count;
        // mesh.TriangleCount = vertices.Count / 3;

        // mesh.Vertices = Raylib.LoadVertexData(vertices.SelectMany(v => new[] { v.X, v.Y, v.Z }).ToArray());
        // mesh.Normals = Raylib.LoadVertexData(normals.SelectMany(n => new[] { n.X, n.Y, n.Z }).ToArray());

        // Raylib.UploadMesh(ref mesh, false);
        raylibContext.Mesh = mesh;
    }
}
