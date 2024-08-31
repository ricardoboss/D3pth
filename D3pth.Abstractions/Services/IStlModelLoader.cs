using D3pth.Abstractions.Models;

namespace D3pth.Abstractions.Services;

public interface IStlModelLoader
{
    Task<IStlModel> LoadAsync(FileInfo file, CancellationToken cancellationToken = default);
}
