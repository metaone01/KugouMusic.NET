using System.Text.Json.Serialization;

namespace KuGou.Net.Abstractions.Models;

/// <summary>
///     登录响应数据
/// </summary>
public record LoginResponse : KgBaseModel
{
    /// <summary>
    ///     用户 ID。
    /// </summary>
    [property: JsonPropertyName("userid")] public long? UserId { get; set; }

    /// <summary>
    ///     登录 Token。
    /// </summary>
    [property: JsonPropertyName("token")] public string? Token { get; set; }

    /// <summary>
    ///     附加登录凭证 `t1`。
    /// </summary>
    [property: JsonPropertyName("t1")] public string? T1 { get; set; }
}

/// <summary>
///     发送验证码响应
/// </summary>
public record SendCodeResponse : KgBaseModel
{
    /// <summary>
    ///     接口返回状态码。
    /// </summary>
    [property: JsonPropertyName("code")] public long Code { get; set; }
}
