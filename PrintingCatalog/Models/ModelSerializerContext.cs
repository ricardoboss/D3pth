using System.Text.Json.Serialization;

namespace PrintingCatalog.Models;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ModelMetadata))]
internal sealed partial class ModelSerializerContext : JsonSerializerContext;
