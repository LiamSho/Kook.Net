using System.Text.Json.Serialization;
using KaiHeiLa.Net.Converters;

namespace KaiHeiLa.API;

internal class RecommendInfo
{
    [JsonPropertyName("guild_id")] 
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public ulong GuildId { get; set; }
    
    [JsonPropertyName("open_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public uint OpenId { get; set; }
    
    [JsonPropertyName("default_channel_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public ulong DefaultChannelId { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("icon")]
    public string Icon { get; set; }
    
    [JsonPropertyName("banner")] 
    public string Banner { get; set; }
    
    [JsonPropertyName("desc")] 
    public string Description { get; set; }
    
    [JsonPropertyName("status")] 
    public int Status { get; set; }
    
    [JsonPropertyName("tag")] 
    public string Tag { get; set; }
    
    [JsonPropertyName("features")]
    public object[] Features { get; set; }
    
    [JsonPropertyName("level")]
    public BoostLevel BoostLevel { get; set; }
    
    [JsonPropertyName("custom_id")] 
    public string CustomId { get; set; }
    
    [JsonPropertyName("is_official_partner")]
    public int IsOfficialPartner { get; set; }

    [JsonPropertyName("sort")] 
    public int Sort { get; set; }
    
    [JsonPropertyName("audit_status")] 
    public int AuditStatus { get; set; }
    
    [JsonPropertyName("update_day_gap")]

    public int UpdateDayInterval { get; set; }
}