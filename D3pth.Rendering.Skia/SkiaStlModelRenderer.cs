using System.Numerics;
using D3pth.Abstractions.Models;
using D3pth.Abstractions.Rendering;
using SkiaSharp;
using Plane = D3pth.Abstractions.Models.Plane;

namespace D3pth.Rendering.Skia;

public class SkiaStlModelRenderer : IStlModelRenderer
{
    public byte[] RenderToPng(IStlModel stlModel, RenderMode renderMode = RenderMode.Shaded,
        RenderOptions options = RenderOptions.None)
    {
        const int imageWidth = 1024;
        const int imageHeight = 1024;

        var surface = SKSurface.Create(new SKImageInfo(imageWidth, imageHeight));

        using var canvas = surface!.Canvas;
        canvas!.Clear(SKColors.Transparent);

        var cameraPosition = new Vector3(150, 100, 150);
        var target = new Vector3(0, 0, 0);
        var up = new Vector3(0, -1, 0); // kinda hacky?
        var viewMatrix = Matrix4x4.CreateLookAt(cameraPosition, target, up);

        var lightPosition = new Vector3(-80, -150, 200);
        var lightColor = SKColors.White;
        const float ambientIntensity = 0.45f;
        const float diffuseIntensity = 0.6f;

        const float fieldOfView = MathF.PI / 6; // 30 degrees
        const float aspectRatio = (float)imageWidth / imageHeight;
        const float nearPlane = 0.1f;
        const float farPlane = 1000f;

        var zoom = stlModel.Metadata.Zoom ?? 1;
        var actualFieldOfView = fieldOfView / zoom;

        var projectionMatrix =
            Matrix4x4.CreatePerspectiveFieldOfView(actualFieldOfView, aspectRatio, nearPlane, farPlane);

        var modelMatrix = Matrix4x4.Identity;

        // hack: models seem to be flipped in the x-axis and have x/y as their base plane
        modelMatrix *= Matrix4x4.CreateScale(-1, 1, 1); // flip x-axis
        modelMatrix *= Matrix4x4.CreateRotationX(-MathF.PI / 2); // rotate about x-axis so y-axis is up

        var modelBaseTranslation = CalculateModelBaseTranslation(stlModel.Triangles, modelMatrix);
        modelMatrix *= Matrix4x4.CreateTranslation(modelBaseTranslation);
        var modelRotation =
            CalculateModelRotation(stlModel.Metadata.BasePlane, stlModel.Metadata.Rotation);
        modelMatrix *= modelRotation;
        modelMatrix *= Matrix4x4.CreateScale(2f);

        if (options.HasFlag(RenderOptions.DrawGrid))
            DrawGrid(canvas, imageWidth, imageHeight, viewMatrix, projectionMatrix);

        var projected =
            CalculateTriangleDepths(TessellateTriangles(ProjectTriangles(stlModel.Triangles, modelMatrix, viewMatrix, projectionMatrix)))
                .OrderByDescending(t => t.Depth);

        switch (renderMode)
        {
            case RenderMode.Shaded:
                var modelColor = SKColors.LightGray;
                if (stlModel.Metadata.Color is { } color)
                {
                    modelColor = SKColor.Parse(color);
                }

                DrawModelShaded(projected, canvas, imageWidth, imageHeight, modelColor, lightPosition, lightColor,
                    ambientIntensity, diffuseIntensity);
                break;
            case RenderMode.Depth:
                DrawModelDepth(projected, canvas, imageWidth, imageHeight);
                break;
            case RenderMode.Wireframe:
                DrawModelWireframe(projected, canvas, imageWidth, imageHeight);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(renderMode), renderMode, "Unknown render mode");
        }

        if (options.HasFlag(RenderOptions.DrawAxes))
            DrawAxes(canvas, imageWidth, imageHeight, viewMatrix, projectionMatrix);
        // DrawSun(canvas, imageWidth, imageHeight, lightPosition, lightColor, viewMatrix, projectionMatrix);

        return surface.Snapshot()!.Encode()!.ToArray()!;
    }

    private static void DrawModelShaded(IOrderedEnumerable<ProjectedTriangleWithDepth> projected, SKCanvas canvas,
        int imageWidth, int imageHeight,
        SKColor modelColor, Vector3 lightPosition, SKColor lightColor, float ambientIntensity, float diffuseIntensity)
    {
        foreach (var (normal, a, b, c, _) in projected)
        {
            DrawTriangleShaded(canvas, imageWidth, imageHeight, normal, a, b, c, modelColor, lightPosition,
                lightColor, ambientIntensity, diffuseIntensity);
        }
    }

    private static void DrawModelWireframe(IOrderedEnumerable<ProjectedTriangleWithDepth> projected, SKCanvas canvas,
        int imageWidth, int imageHeight)
    {
        var index = 0;
        foreach (var (_, a, b, c, _) in projected)
        {
            DrawTriangleDebug(canvas, imageWidth, imageHeight, index++, a, b, c);
        }
    }

    private static void DrawModelDepth(IOrderedEnumerable<ProjectedTriangleWithDepth> projected, SKCanvas canvas,
        int imageWidth, int imageHeight)
    {
        foreach (var (_, a, b, c, depth) in projected)
        {
            DrawTriangleDepthDebug(canvas, imageWidth, imageHeight, depth, a, b, c);
        }
    }

    private static Matrix4x4 CalculateModelRotation(Plane? basePlane, float? yAxisRotation)
    {
        // basePlane and frontPlane are relative to the model
        // we need to rotate the model so that the base plane matches the xz-plane and the front plane matches the yz-plane

        var rotation = Matrix4x4.Identity;

        switch (basePlane)
        {
            case Plane.XY:
                rotation *= Matrix4x4.CreateRotationX(-MathF.PI / 2);
                break;
            case Plane.XZ:
                break;
            case Plane.YZ:
                rotation *= Matrix4x4.CreateRotationZ(MathF.PI / 2);
                break;
            case Plane.NegativeXY:
                rotation *= Matrix4x4.CreateRotationX(MathF.PI / 2);
                break;
            case Plane.NegativeXZ:
                rotation *= Matrix4x4.CreateRotationZ(MathF.PI) * Matrix4x4.CreateRotationY(MathF.PI);
                break;
            case Plane.NegativeYZ:
                rotation *= Matrix4x4.CreateRotationZ(-MathF.PI / 2);
                break;
            case null:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(basePlane), basePlane, null);
        }

        if (yAxisRotation is not { } yDeg)
            return rotation;

        ArgumentOutOfRangeException.ThrowIfGreaterThan(Math.Abs(yDeg), 180, "Rotation");

        rotation *= Matrix4x4.CreateRotationY(yDeg * MathF.PI / 180);

        return rotation;
    }

    private static IEnumerable<ProjectedTriangle> ProjectTriangles(IEnumerable<Triangle> triangles,
        Matrix4x4 modelMatrix, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
    {
        foreach (var (normal, a, b, c, _) in triangles)
        {
            var modelA = Vector3.Transform(a, modelMatrix);
            var modelB = Vector3.Transform(b, modelMatrix);
            var modelC = Vector3.Transform(c, modelMatrix);

            var projectedA = TransformPoint(modelA, viewMatrix, projectionMatrix);
            var projectedB = TransformPoint(modelB, viewMatrix, projectionMatrix);
            var projectedC = TransformPoint(modelC, viewMatrix, projectionMatrix);

            yield return new ProjectedTriangle(normal, projectedA, projectedB, projectedC);
        }
    }

    private static IEnumerable<ProjectedTriangle> TessellateTriangles(IEnumerable<ProjectedTriangle> triangles)
    {
        const float maxArea = 0.00001f;
        foreach (var (normal, a, b, c) in triangles)
        {
            var area = MathF.Abs(Vector3.Cross(b - a, c - a).Length()) / 2f;
            if (area > maxArea)
            {
                var splits = SplitTriangle(a, b, c).Select(t => new ProjectedTriangle(normal, t.a, t.b, t.c));
                var nextLevel = TessellateTriangles(splits);
                foreach (var triangle in nextLevel)
                    yield return triangle;
            }
            else
                yield return new ProjectedTriangle(normal, a, b, c);
        }
    }

    private static IEnumerable<ProjectedTriangleWithDepth> CalculateTriangleDepths(
        IEnumerable<ProjectedTriangle> triangles
    )
    {
        foreach (var (normal, a, b, c) in triangles)
        {
            var minDepth = Math.Min(a.Z, Math.Min(b.Z, c.Z));
            var maxDepth = Math.Max(a.Z, Math.Max(b.Z, c.Z));

            var depth = minDepth + (maxDepth - minDepth) / 2;

            yield return new ProjectedTriangleWithDepth(normal, a, b, c, depth);
        }
    }

    private static IEnumerable<(Vector3 a, Vector3 b, Vector3 c)> SplitTriangle(Vector3 A, Vector3 B, Vector3 C)
    {
        // Step 1: Calculate the midpoints
        var midAB = (A + B) / 2;
        var midBC = (B + C) / 2;
        var midCA = (C + A) / 2;

        // Step 2: Form the smaller triangles
        yield return (A, midAB, midCA);
        yield return (B, midBC, midAB);
        yield return (C, midCA, midBC);
        yield return (midAB, midBC, midCA);
    }

    private static Vector3 CalculateModelBaseTranslation(IEnumerable<Triangle> triangles, Matrix4x4 modelMatrix)
    {
        var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        foreach (var (_, a, b, c, _) in triangles)
        {
            var transformedA = Vector3.Transform(a, modelMatrix);
            var transformedB = Vector3.Transform(b, modelMatrix);
            var transformedC = Vector3.Transform(c, modelMatrix);

            min = Vector3.Min(min, transformedA);
            min = Vector3.Min(min, transformedB);
            min = Vector3.Min(min, transformedC);

            max = Vector3.Max(max, transformedA);
            max = Vector3.Max(max, transformedB);
            max = Vector3.Max(max, transformedC);
        }

        var boundingBoxCenter = (max + min) / 2;

        // move y=0 to the bottom of the model
        var translateY = -boundingBoxCenter.Y;

        // move center of the model, so it touches x/z axis
        var translateX = -boundingBoxCenter.X;
        var translateZ = -boundingBoxCenter.Z;

        return new(translateX, translateY, translateZ);
    }

    private static void DrawGrid(SKCanvas canvas, int width, int height, Matrix4x4 viewMatrix,
        Matrix4x4 projectionMatrix)
    {
        const int gridSize = 200;
        const int gridStep = 10;

        using var paint = new SKPaint();
        paint.Color = SKColors.Gray.WithAlpha(60);

        for (var x = -gridSize; x <= gridSize; x += gridStep)
        {
            var a = new Vector3(x, 0, -gridSize);
            var b = new Vector3(x, 0, gridSize);

            var aTransformed = TransformPoint(a, viewMatrix, projectionMatrix);
            var bTransformed = TransformPoint(b, viewMatrix, projectionMatrix);

            DrawLine(canvas, width, height, aTransformed, bTransformed, paint);
        }

        for (var z = -gridSize; z <= gridSize; z += gridStep)
        {
            var a = new Vector3(-gridSize, 0, z);
            var b = new Vector3(gridSize, 0, z);

            var aTransformed = TransformPoint(a, viewMatrix, projectionMatrix);
            var bTransformed = TransformPoint(b, viewMatrix, projectionMatrix);

            DrawLine(canvas, width, height, aTransformed, bTransformed, paint);
        }
    }

    private static void DrawSun(SKCanvas canvas, int width, int height, Vector3 lightPosition, SKColor lightColor,
        Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
    {
        var sunRadius = 20;

        using var paint = new SKPaint();
        paint.Color = lightColor;

        var sunRadiusTransformed = TransformPoint(lightPosition, viewMatrix, projectionMatrix);

        DrawCircle(canvas, width, height, sunRadiusTransformed, sunRadius, paint);
    }

    private static void DrawAxes(SKCanvas canvas, int width, int height, Matrix4x4 viewMatrix,
        Matrix4x4 projectionMatrix)
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

        var originTransformed = TransformPoint(origin, viewMatrix, projectionMatrix);
        var xAxisTransformed = TransformPoint(xAxis, viewMatrix, projectionMatrix);
        var yAxisTransformed = TransformPoint(yAxis, viewMatrix, projectionMatrix);
        var zAxisTransformed = TransformPoint(zAxis, viewMatrix, projectionMatrix);

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

    private static void DrawTriangleDebug(SKCanvas canvas,
        int width,
        int height,
        int index,
        Vector3 a,
        Vector3 b,
        Vector3 c)
    {
        // Convert to screen space (e.g., from -1..1 to canvas size)
        var p1 = new SKPoint((a.X + 1) * width / 2, (a.Y + 1) * height / 2);
        var p2 = new SKPoint((b.X + 1) * width / 2, (b.Y + 1) * height / 2);
        var p3 = new SKPoint((c.X + 1) * width / 2, (c.Y + 1) * height / 2);

        using var path = new SKPath();
        path.FillType = SKPathFillType.EvenOdd;
        path.MoveTo(p1);
        path.LineTo(p2);
        path.LineTo(p3);
        path.LineTo(p1);

        using var paint = new SKPaint();
        paint.Color = SKColor.FromHsl(index % 360, 255, 128, 40);
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 1;
        paint.IsAntialias = true;

        canvas.DrawPath(path, paint);
    }

    private static void DrawTriangleDepthDebug(SKCanvas canvas,
        int width,
        int height,
        float depth,
        Vector3 a,
        Vector3 b,
        Vector3 c)
    {
        using var paint = new SKPaint();

        const float depthScale = 5000f;
        const float depthOffset = 0.999f;
        var scaledDepth = depthScale * (depth - depthOffset);
        // scaledDepth = MathF.Min(MathF.Max(scaledDepth, 0), 1);

        paint.Color = SKColor.FromHsl((360f * scaledDepth) % 360f, 50, 50);
        paint.Style = SKPaintStyle.Fill;

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

    private static void DrawCircle(SKCanvas canvas, int width, int height, Vector3 center, float radius, SKPaint paint)
    {
        // Convert to screen space (e.g., from -1..1 to canvas size)
        var p1 = new SKPoint((center.X + 1) * width / 2, (center.Y + 1) * height / 2);

        // Draw the circle
        canvas.DrawCircle(p1.X, p1.Y, radius, paint);
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

        // Calculate the diffuse intensity using Lambert's cosine law
        var diffuse = MathF.Max(Vector3.Dot(normal, lightDir), 0.0f);

        // Calculate final intensity
        var intensity = ambientIntensity + diffuseIntensity * diffuse;

        // Apply the light color to the intensity
        var r = (byte)Math.Min(modelColor.Red * intensity * (lightColor.Red / 255.0f), 255);
        var g = (byte)Math.Min(modelColor.Green * intensity * (lightColor.Green / 255.0f), 255);
        var b = (byte)Math.Min(modelColor.Blue * intensity * (lightColor.Blue / 255.0f), 255);

        return new(r, g, b, modelColor.Alpha); // Preserve the original alpha channel
    }
}

internal sealed record ProjectedTriangle(Vector3 Normal, Vector3 A, Vector3 B, Vector3 C);
internal sealed record ProjectedTriangleWithDepth(Vector3 Normal, Vector3 A, Vector3 B, Vector3 C, float Depth);
