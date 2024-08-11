using System.Numerics;
using PrintingCatalog.Interfaces;
using SkiaSharp;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace PrintingCatalog.Services;

public class SkiaStlModelRenderer : IStlModelRenderer
{
    public async Task<byte[]> RenderToPngAsync(IStlModel stlModel, CancellationToken cancellationToken = default)
    {
        const int imageWidth = 512;
        const int imageHeight = 512;

        var surface = SKSurface.Create(new SKImageInfo(imageWidth, imageHeight));
        using var canvas = surface!.Canvas;

        canvas!.Clear(SKColors.Transparent);

        // center the canvas
        canvas.Translate(imageWidth / 2f, imageHeight / 2f);

        await RenderTrianglesAsync(canvas, stlModel.Triangles);

        return surface.Snapshot()!.Encode()!.ToArray()!;
    }

    private static async Task RenderTrianglesAsync(SKCanvas canvas, Triangle[] triangles)
    {
        var canvasMatrix = SKMatrix.CreateTranslation(0, 0)
            .PostConcat(SKMatrix.CreateRotation(0))
            .PostConcat(SKMatrix.CreateScale(15, 15));

        var hue = 0f;

        foreach (var (_, a, b, c, _) in triangles)
        {
            var path = TraceTriangle(a, b, c);

            path.Transform(canvasMatrix);

            // rotate hue of the triangle
            hue += 1f;
            hue %= 255f;
            var paint = new SKPaint
            {
                Color = SKColor.FromHsl(hue, 255, 128, 80),
                IsAntialias = true,
            };

            canvas.DrawPath(path, paint);
        }
    }

    private static SKPath TraceTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        var face = new SKPath
        {
            FillType = SKPathFillType.EvenOdd,
        };

        var aProjected = ProjectPoint(a);
        var bProjected = ProjectPoint(b);
        var cProjected = ProjectPoint(c);

        face.MoveTo(aProjected.X, aProjected.Y);
        face.LineTo(bProjected.X, bProjected.Y);
        face.LineTo(cProjected.X, cProjected.Y);
        face.Close();

        return face;
    }

    private static Vector2 ProjectPoint(Vector3 point)
    {
        const float perspective = 1000;
        var scale = perspective / (point.Z + perspective);

        var x = point.X * scale;
        var y = point.Y * scale;

        return new(x, y);
    }
}