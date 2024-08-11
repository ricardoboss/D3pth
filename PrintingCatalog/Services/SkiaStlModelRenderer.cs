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

        canvas!.Clear(SKColors.White);

        // center the canvas
        canvas.Translate(imageWidth / 2f, imageHeight / 2f);

        // RenderCube(canvas, view);
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

    private static void RenderCube(SKCanvas canvas, SK3dView view)
    {
        // define the cube face
        var face = SKRect.Create(0, 0, 100, 100);

        // draw the left face
        using (new SKAutoCanvasRestore(canvas, true))
        {
            // get the face in the correct location
            view.Save();
            view.RotateYDegrees(-90);
            view.ApplyToCanvas(canvas);
            view.Restore();

            // draw the face
            var leftFace = new SKPaint
            {
                Color = SKColors.LightGray,
                IsAntialias = true,
            };
            canvas.DrawRect(face, leftFace);
        }

        // draw the right face
        using (new SKAutoCanvasRestore(canvas, true))
        {
            // get the face in the correct location
            view.Save();
            view.TranslateZ(-100);
            view.ApplyToCanvas(canvas);
            view.Restore();

            // draw the face
            var rightFace = new SKPaint
            {
                Color = SKColors.Gray,
                IsAntialias = true,
            };
            canvas.DrawRect(face, rightFace);
        }

        // draw the top face
        using (new SKAutoCanvasRestore(canvas, true))
        {
            // get the face in the correct location
            view.Save();
            view.RotateXDegrees(90);
            view.ApplyToCanvas(canvas);
            view.Restore();

            // draw the face
            var topFace = new SKPaint
            {
                Color = SKColors.DarkGray,
                IsAntialias = true,
            };
            canvas.DrawRect(face, topFace);
        }
    }
}