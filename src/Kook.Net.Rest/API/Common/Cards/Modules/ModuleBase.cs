using Kook.Net.Converters;
using System.Text.Json.Serialization;

namespace Kook.API;

internal class ModuleBase : IModule
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(ModuleTypeConverter))]
    public ModuleType Type { get; set; }
}
