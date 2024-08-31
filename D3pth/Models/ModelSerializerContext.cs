using System.Text.Json.Serialization;
using D3pth.Interfaces;

namespace D3pth.Models;

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ModelMetadata))]
[JsonSerializable(typeof(Plane))]
internal sealed partial class ModelSerializerContext : JsonSerializerContext;