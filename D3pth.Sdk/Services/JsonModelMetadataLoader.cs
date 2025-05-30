using System.Text.Json;
using D3pth.Abstractions.Models;
using D3pth.Abstractions.Services;
using D3pth.Sdk.Models;

namespace D3pth.Sdk.Services;

public class JsonModelMetadataLoader : IModelMetadataLoader
{
    public async Task<IModelMetadata> LoadAsync(FileInfo modelFile, CancellationToken cancellationToken = default)
    {
        ModelMetadata? metadata = null;

        var metadataFile = new FileInfo(Path.ChangeExtension(modelFile.FullName, ".json"));
        if (metadataFile.Exists)
        {
            await using var stream = metadataFile.OpenRead();
            using var reader = new StreamReader(stream);

            var json = await reader.ReadToEndAsync(cancellationToken);
            metadata = JsonSerializer.Deserialize(json, ModelSerializerContext.Default.ModelMetadata);
        }

        var fallbackName = Path.GetFileNameWithoutExtension(modelFile.Name);

        metadata ??= new();

        if (string.IsNullOrEmpty(metadata.Name))
            metadata.Name = fallbackName;

        return metadata;
    }
}
