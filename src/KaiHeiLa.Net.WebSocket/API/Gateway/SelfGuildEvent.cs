using System.Text.Json.Serialization;

namespace KaiHeiLa.API.Gateway;

internal class SelfGuildEvent
{
    [JsonPropertyName("guild_id")]
    public ulong GuildId { get; set; }
}