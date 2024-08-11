using PrintingCatalog.Interfaces;
using PrintingCatalog.Models;

namespace PrintingCatalog.Services;

public class StlModelLoader : IStlModelLoader
{
    public async Task<IStlModel> LoadAsync(FileInfo file, CancellationToken cancellationToken = default)
    {
        await using var stream = file.OpenRead();
        using var reader = new BinaryReader(stream);

        var header = new byte[80];
        if (reader.Read(header, 0, 80) != 80)
            throw new InvalidDataException("Invalid STL file: header is missing or incomplete");

        if (header.AsSpan().StartsWith("solid"u8))
            throw new NotSupportedException("ASCII STL files are not supported");

        var numberOfTriangles = reader.ReadUInt32();

        var triangles = new Triangle[numberOfTriangles];
        for (var i = 0; i < numberOfTriangles; i++)
        {
            var normalX = reader.ReadSingle();
            var normalY = reader.ReadSingle();
            var normalZ = reader.ReadSingle();

            var aX = reader.ReadSingle();
            var aY = reader.ReadSingle();
            var aZ = reader.ReadSingle();

            var bX = reader.ReadSingle();
            var bY = reader.ReadSingle();
            var bZ = reader.ReadSingle();

            var cX = reader.ReadSingle();
            var cY = reader.ReadSingle();
            var cZ = reader.ReadSingle();

            var attributeByteCount = reader.ReadUInt16();

            var triangle = new Triangle(
                Normal: new(normalX, normalY, normalZ),
                A: new(aX, aY, aZ),
                B: new(bX, bY, bZ),
                C: new(cX, cY, cZ),
                AttributeByteCount: attributeByteCount
            );

            triangles[i] = triangle;
        }

        return new StlModel
        {
            Metadata = new ModelMetadata
            {
                Name = Path.GetFileNameWithoutExtension(file.Name),
                SourceFile = file,
            },
            Header = header,
            Triangles = triangles,
        };
    }
}
