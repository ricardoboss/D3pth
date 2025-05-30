namespace D3pth.Abstractions.Models;

public interface IStlModel
{
    FileInfo File { get; }

    Triangle[] Triangles { get; }

    string Md5Hash { get; }
}
