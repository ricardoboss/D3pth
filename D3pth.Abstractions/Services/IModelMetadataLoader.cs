using D3pth.Abstractions.Models;

namespace D3pth.Abstractions.Services;

public interface IModelMetadataLoader
{
    Task<IModelMetadata> LoadAsync(FileInfo modelFile, CancellationToken cancellationToken = default);
}
