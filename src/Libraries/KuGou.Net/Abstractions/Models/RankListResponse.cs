using System.Text.Json.Serialization;

namespace KuGou.Net.Abstractions.Models;

/// <summary>
///     榜单列表结果。
/// </summary>
public record RankListResponse : KgBaseModel
{
    /// <summary>
    ///     榜单列表。
    /// </summary>
    [property: JsonPropertyName("info")] public List<RankListItem> Info { get; set; } = new();
}

/// <summary>
///     单个榜单信息。
/// </summary>
public record RankListItem
{
    /// <summary>
    ///     榜单封面图地址。
    /// </summary>
    [property: JsonPropertyName("img_9")]
    public string? Cover
    {
        get => field?.Replace("{size}", "250");
        set;
    }

    /// <summary>
    ///     榜单 ID。
    /// </summary>
    [property: JsonPropertyName("rankid")] public long FileId { get; set; }

    /// <summary>
    ///     榜单名称。
    /// </summary>
    [property: JsonPropertyName("rankname")]
    public string Name { get; set; } = "";
}
