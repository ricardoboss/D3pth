namespace D3pth.Abstractions;

public interface IStlModelLoader
{
    Task<IStlModel> LoadAsync(FileInfo file, CancellationToken cancellationToken = default);
}
