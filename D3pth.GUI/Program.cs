using D3pth.Abstractions.Models;
using D3pth.Abstractions.Rendering;
using D3pth.Abstractions.Services;
using D3pth.Rendering.Skia;
using D3pth.Sdk.Services;
using Raylib_cs;

const int windowWidth = 800;
const int windowHeight = 900;

IStlModel? model = null;
IModelMetadata? metadata = null;
byte[]? png = null;
_ = RenderModel(windowWidth, windowHeight, 0);

Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.VSyncHint);
Raylib.InitWindow(windowWidth, windowHeight, "Hello World");

Raylib.BeginDrawing();
Raylib.ClearBackground(Color.White);
Raylib.EndDrawing();

var drawCount = 0;
Texture2D? texture = null;

while (!Raylib.WindowShouldClose())
{
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.White);

    if (png != null)
    {
        if (texture.HasValue)
            Raylib.UnloadTexture(texture.Value);

        var image = Raylib.LoadImageFromMemory(".png", png);
        texture = Raylib.LoadTextureFromImage(image);
        Raylib.UnloadImage(image);

        drawCount++;
        png = null;
        _ = RenderModel(Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), drawCount * 3);
    }

    if (texture.HasValue)
        Raylib.DrawTexture(texture.Value, 0, 0, Color.White);

    Raylib.DrawText(drawCount.ToString(), 10, 10, 12, Color.Black);

    Raylib.EndDrawing();
}

Raylib.CloseWindow();

return;

async Task RenderModel(int imageWidth, int imageHeight, int rotation)
{
    if (model == null || metadata == null)
    {
        IStlModelLoader stlModelLoader = new StlModelLoader();
        IModelMetadataLoader modelMetadataLoader = new JsonModelMetadataLoader();

        var info = new FileInfo("Utah_teapot_(solid).stl");
        model = await stlModelLoader.LoadAsync(info);
        metadata = await modelMetadataLoader.LoadAsync(info);
    }

    metadata.Rotation = rotation % 360 - 180;

    IStlModelRenderer stlModelRenderer = new SkiaStlModelRenderer();
    png = stlModelRenderer.RenderToPng(imageWidth, imageHeight, model, metadata);
}
