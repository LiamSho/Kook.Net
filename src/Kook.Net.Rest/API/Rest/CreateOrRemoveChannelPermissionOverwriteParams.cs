using Kook.Net.Converters;
using System.Text.Json.Serialization;

namespace Kook.API.Rest;

internal class CreateOrRemoveChannelPermissionOverwriteParams
{
    [JsonPropertyName("channel_id")]
    public ulong ChannelId { get; set; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(PermissionOverwriteTargetTypeConverter))]
    public PermissionOverwriteTargetType TargetType { get; set; }

    [JsonPropertyName("value")]
    public ulong TargetId { get; set; }

    public CreateOrRemoveChannelPermissionOverwriteParams(ulong channelId, PermissionOverwriteTargetType targetType, ulong targetId)
    {
        ChannelId = channelId;
        TargetType = targetType;
        TargetId = targetId;
    }
}
