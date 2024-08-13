using System.Text.Json.Serialization;
using PrintingCatalog.Interfaces;

namespace PrintingCatalog.Models;

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ModelMetadata))]
[JsonSerializable(typeof(Plane))]
internal sealed partial class ModelSerializerContext : JsonSerializerContext;