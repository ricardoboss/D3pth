using D3pth.Abstractions.Models;
using D3pth.Abstractions.Rendering;
using D3pth.Abstractions.Services;
using D3pth.Rendering.Skia;
using D3pth.Sdk.Services;
using Raylib_cs;

const int windowWidth = 800;
const int windowHeight = 800;

Task<byte[]>? renderTask = null;
byte[]? lastRender = null;
IStlModel? model = null;
IModelMetadata? metadata = null;

IStlModelLoader stlModelLoader = new StlModelLoader();
IModelMetadataLoader modelMetadataLoader = new JsonModelMetadataLoader();
IPngRenderer renderer = new SkiaPngRenderer(new());

MainLoop();

return;

void StartRender(int imageWidth, int imageHeight, int rotation)
{
    renderTask = Task.Run(() => RenderModel(imageWidth, imageHeight, rotation));
}

void Update(int imageWidth, int imageHeight, int frameCount)
{
    if (renderTask == null)
    {
        StartRender(imageWidth, imageHeight, frameCount * 3);

        return;
    }

    if (renderTask.IsCompleted)
    {
        if (renderTask.IsCompletedSuccessfully)
            lastRender = renderTask.Result;

        StartRender(imageWidth, imageHeight, frameCount * 3);
    }
}

void MainLoop()
{
    var drawCount = 0;
    var frameRate = 60;

    Texture2D? texture = null;

    Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
    Raylib.InitWindow(windowWidth, windowHeight, "D3pth Renderer");

    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.White);
    Raylib.EndDrawing();

    Raylib.SetTargetFPS(60);
    while (!Raylib.WindowShouldClose())
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.White);

        if (lastRender != null)
        {
            if (texture.HasValue)
                Raylib.UnloadTexture(texture.Value);

            var image = Raylib.LoadImageFromMemory(".png", lastRender);
            texture = Raylib.LoadTextureFromImage(image);
            Raylib.UnloadImage(image);

            lastRender = null;

            drawCount++;
        }

        Update(Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), drawCount);

        if (texture.HasValue)
            Raylib.DrawTexture(texture.Value, 0, 0, Color.White);

        frameRate = (int)(frameRate * 0.9f + (1f / Raylib.GetFrameTime()) * 0.1f);

        Raylib.DrawText(drawCount.ToString(), 10, 10, 12, Color.Black);
        Raylib.DrawText(frameRate.ToString(), 10, 24, 12, Color.Black);

        Raylib.EndDrawing();
    }

    Raylib.CloseWindow();
}

async Task<byte[]> RenderModel(int imageWidth, int imageHeight, int rotation)
{
    if (model == null || metadata == null)
    {
        var info = new FileInfo("Utah_teapot_(solid).stl");
        model = await stlModelLoader.LoadAsync(info);
        metadata = await modelMetadataLoader.LoadAsync(info);
    }

    metadata.Rotation = rotation % 360 - 180;

    return renderer.Render(imageWidth, imageHeight, model, metadata);
}
