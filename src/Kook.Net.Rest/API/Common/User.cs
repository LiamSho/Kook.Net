using System.Text.Json.Serialization;

namespace Kook.API;

internal class User
{
    [JsonPropertyName("id")]
    public ulong Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("identify_num")]
    public string IdentifyNumber { get; set; }

    [JsonPropertyName("online")]
    public bool Online { get; set; }

    [JsonPropertyName("os")]
    public string OperatingSystem { get; set; }

    [JsonPropertyName("bot")]
    public bool? Bot { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }

    [JsonPropertyName("vip_avatar")]
    public string BuffAvatar { get; set; }

    [JsonPropertyName("is_vip")]
    public bool? HasBuff { get; set; }

    [JsonPropertyName("vip_amp")]
    public bool? HasAnnualBuff { get; set; }

    [JsonPropertyName("is_ai_reduce_noise")]
    public bool? IsDenoiseEnabled { get; set; }

    [JsonPropertyName("tag_info")]
    public UserTag UserTag { get; set; }

    [JsonPropertyName("banner")]
    public string Banner { get; set; }

    [JsonPropertyName("nameplate")]
    public Nameplate[] Nameplates { get; set; }

    [JsonPropertyName("is_sys")]
    public bool? IsSystemUser { get; set; }
}
