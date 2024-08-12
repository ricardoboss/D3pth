using System.Numerics;
using PrintingCatalog.Interfaces;
using SkiaSharp;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace PrintingCatalog.Services;

public class SkiaStlModelRenderer : IStlModelRenderer
{
    public async Task<byte[]> RenderToPngAsync(IStlModel stlModel, CancellationToken cancellationToken = default)
    {
        const int imageWidth = 1024;
        const int imageHeight = 1024;

        var surface = SKSurface.Create(new SKImageInfo(imageWidth, imageHeight));

        using var canvas = surface!.Canvas;
        canvas!.Clear(SKColors.Transparent);

        var cameraPosition = new Vector3(0, 150, 150);
        var target = new Vector3(0, -190, -150);
        var up = new Vector3(0, -1, 0);
        var viewMatrix = Matrix4x4.CreateLookAt(cameraPosition, target, up);

        var lightPosition = new Vector3(200, 200, 200);
        var lightColor = SKColors.Wheat;
        const float ambientIntensity = 0.3f;
        const float diffuseIntensity = 0.7f;

        const float fieldOfView = MathF.PI / 8; // 45 degrees
        const float aspectRatio = (float)imageWidth / imageHeight;
        const float nearPlane = 0.1f;
        const float farPlane = 1000f;
        var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlane, farPlane);

        var modelMatrix = Matrix4x4.CreateScale(1.5f) *
                          Matrix4x4.CreateRotationZ(MathF.PI);

        // var viewProjectionMatrix = projectionMatrix * viewMatrix;
        // var modelViewProjectionMatrix = viewProjectionMatrix * modelMatrix;
        var modelViewProjectionMatrix = projectionMatrix * modelMatrix;

        var modelColor = SKColors.Gray;
        if (stlModel.Metadata.Color is { } color)
        {
            modelColor = SKColor.Parse(color);
        }

        DrawGrid(canvas, imageWidth, imageHeight, viewMatrix, projectionMatrix);

        var projected = ProjectTriangles(stlModel.Triangles, viewMatrix, modelViewProjectionMatrix)
            .OrderByDescending(t => t.Depth);

        foreach (var (normal, a, b, c, _) in projected)
        {
            DrawTriangleShaded(canvas, imageWidth, imageHeight, normal, a, b, c, modelColor, lightPosition,
                lightColor, ambientIntensity, diffuseIntensity);
        }

        // DrawAxes(canvas, imageWidth, imageHeight, viewMatrix, projectionMatrix);

        return surface.Snapshot()!.Encode()!.ToArray()!;
    }

    private static IEnumerable<ProjectedTriangle> ProjectTriangles(IEnumerable<Triangle> triangles,
        Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
    {
        foreach (var (_, a, b, c, _) in triangles)
        {
            var aTransformed = TransformPoint(a, viewMatrix, projectionMatrix);
            var bTransformed = TransformPoint(b, viewMatrix, projectionMatrix);
            var cTransformed = TransformPoint(c, viewMatrix, projectionMatrix);

            var depth = Math.Max(aTransformed.Z, Math.Max(bTransformed.Z, cTransformed.Z));

            // not all models have normals
            var normal = CalculateNormal(a, b, c);

            yield return new(normal, aTransformed, bTransformed, cTransformed, depth);
        }
    }

    private static Vector3 CalculateNormal(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        // Calculate the edge vectors
        var edge1 = v2 - v1;
        var edge2 = v3 - v1;

        // Compute the cross product using the right-hand rule
        var normal = Vector3.Cross(edge1, edge2);

        // Normalize the normal vector to ensure it's a unit vector
        normal = Vector3.Normalize(normal);

        return normal;
    }

    private static void DrawGrid(SKCanvas canvas, int width, int height, Matrix4x4 viewMatrix,
        Matrix4x4 viewProjectionMatrix)
    {
        const int gridSize = 100;
        const int gridStep = 10;

        using var paint = new SKPaint();
        paint.Color = SKColors.Gray.WithAlpha(30);

        for (var x = -gridSize; x <= gridSize; x += gridStep)
        {
            var a = new Vector3(x, 0, -gridSize);
            var b = new Vector3(x, 0, gridSize);

            var aTransformed = TransformPoint(a, viewMatrix, viewProjectionMatrix);
            var bTransformed = TransformPoint(b, viewMatrix, viewProjectionMatrix);

            DrawLine(canvas, width, height, aTransformed, bTransformed, paint);
        }

        for (var z = -gridSize; z <= gridSize; z += gridStep)
        {
            var a = new Vector3(-gridSize, 0, z);
            var b = new Vector3(gridSize, 0, z);

            var aTransformed = TransformPoint(a, viewMatrix, viewProjectionMatrix);
            var bTransformed = TransformPoint(b, viewMatrix, viewProjectionMatrix);

            DrawLine(canvas, width, height, aTransformed, bTransformed, paint);
        }
    }

    private static void DrawAxes(SKCanvas canvas, int width, int height, Matrix4x4 viewMatrix,
        Matrix4x4 viewProjectionMatrix)
    {
        var origin = new Vector3(0, 0, 0);
        var xAxis = new Vector3(100, 0, 0);
        var yAxis = new Vector3(0, 100, 0);
        var zAxis = new Vector3(0, 0, 100);

        using var xAxisPaint = new SKPaint();
        xAxisPaint.Color = SKColors.Red;

        using var yAxisPaint = new SKPaint();
        yAxisPaint.Color = SKColors.Blue;

        using var zAxisPaint = new SKPaint();
        zAxisPaint.Color = SKColors.Green;

        var originTransformed = TransformPoint(origin, viewMatrix, viewProjectionMatrix);
        var xAxisTransformed = TransformPoint(xAxis, viewMatrix, viewProjectionMatrix);
        var yAxisTransformed = TransformPoint(yAxis, viewMatrix, viewProjectionMatrix);
        var zAxisTransformed = TransformPoint(zAxis, viewMatrix, viewProjectionMatrix);

        DrawLine(canvas, width, height, originTransformed, xAxisTransformed, xAxisPaint);
        DrawLine(canvas, width, height, originTransformed, yAxisTransformed, yAxisPaint);
        DrawLine(canvas, width, height, originTransformed, zAxisTransformed, zAxisPaint);
    }

    private static Vector3 TransformPoint(Vector3 point, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
    {
        // Convert the point into camera space
        var cameraSpace = Vector3.Transform(point, viewMatrix);

        // Project into 2D space
        var projectedPoint = Vector4.Transform(cameraSpace, projectionMatrix);
        if (projectedPoint.W == 0)
            return new(projectedPoint.X, projectedPoint.Y, projectedPoint.Z);

        // Normalize by w component to perform perspective divide
        projectedPoint.X /= projectedPoint.W;
        projectedPoint.Y /= projectedPoint.W;
        projectedPoint.Z /= projectedPoint.W;

        return new(projectedPoint.X, projectedPoint.Y, projectedPoint.Z);
    }

    private static void DrawTriangleShaded(
        SKCanvas canvas,
        int width,
        int height,
        Vector3 normal,
        Vector3 a,
        Vector3 b,
        Vector3 c,
        SKColor modelColor,
        Vector3 lightPos,
        SKColor lightColor,
        float ambientIntensity,
        float diffuseIntensity
    )
    {
        var center = (a + b + c) / 3;
        var shadingColor = CalculateShading(normal, lightPos, lightColor, center, modelColor, ambientIntensity,
            diffuseIntensity);

        using var paint = new SKPaint();
        paint.Color = shadingColor;

        DrawTriangle(canvas, width, height, a, b, c, paint);
    }

    private static void DrawTriangle(
        SKCanvas canvas,
        int width,
        int height,
        Vector3 a,
        Vector3 b,
        Vector3 c,
        SKPaint paint
    )
    {
        // Convert to screen space (e.g., from -1..1 to canvas size)
        var p1 = new SKPoint((a.X + 1) * width / 2, (a.Y + 1) * height / 2);
        var p2 = new SKPoint((b.X + 1) * width / 2, (b.Y + 1) * height / 2);
        var p3 = new SKPoint((c.X + 1) * width / 2, (c.Y + 1) * height / 2);

        // Draw the triangle
        using var path = new SKPath();
        path.FillType = SKPathFillType.EvenOdd;
        path.MoveTo(p1);
        path.LineTo(p2);
        path.LineTo(p3);
        path.LineTo(p1);
        canvas.DrawPath(path, paint);
    }

    private static void DrawLine(SKCanvas canvas, int width, int height, Vector3 a, Vector3 b, SKPaint paint)
    {
        // Convert to screen space (e.g., from -1..1 to canvas size)
        var p1 = new SKPoint((a.X + 1) * width / 2, (a.Y + 1) * height / 2);
        var p2 = new SKPoint((b.X + 1) * width / 2, (b.Y + 1) * height / 2);

        // Draw the line
        canvas.DrawLine(p1, p2, paint);
    }

    private static SKColor CalculateShading(
        Vector3 normal,
        Vector3 lightPos,
        SKColor lightColor,
        Vector3 vertex,
        SKColor modelColor,
        float ambientIntensity,
        float diffuseIntensity
    )
    {
        // Calculate the light direction
        var lightDir = Vector3.Normalize(lightPos - vertex);

        // Normalize the normal
        var normalizedNormal = Vector3.Normalize(normal);

        // Calculate the diffuse intensity using Lambert's cosine law
        var diffuse = MathF.Max(Vector3.Dot(normalizedNormal, lightDir), 0.0f);

        // Calculate final intensity
        var finalIntensity = ambientIntensity + (diffuseIntensity * diffuse);

        // Ensure the intensity is within the range [0, 1]
        finalIntensity = MathF.Min(finalIntensity, 1.0f);

        // Apply the light color to the intensity
        var r = (byte)(modelColor.Red * finalIntensity * (lightColor.Red / 255.0f));
        var g = (byte)(modelColor.Green * finalIntensity * (lightColor.Green / 255.0f));
        var b = (byte)(modelColor.Blue * finalIntensity * (lightColor.Blue / 255.0f));

        return new(r, g, b, modelColor.Alpha); // Preserve the original alpha channel
    }
}

internal sealed record ProjectedTriangle(Vector3 Normal, Vector3 A, Vector3 B, Vector3 C, float Depth);