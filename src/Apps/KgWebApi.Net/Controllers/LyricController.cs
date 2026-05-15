using KuGou.Net.Adapters.Lyrics;
using KuGou.Net.Clients;
using Microsoft.AspNetCore.Mvc;

namespace KgWebApi.Net.Controllers;

[ApiController]
public class LyricController(LyricClient lyricClient) : ControllerBase
{
    [HttpGet("search/lyric")]
    public async Task<IActionResult> SearchLyric(
        [FromQuery] string? hash,
        [FromQuery(Name = "album_audio_id")] string? albumAudioId,
        [FromQuery] string? keywords,
        [FromQuery] string? man)
    {
        var result = await lyricClient.SearchLyricAsync(hash, albumAudioId, keywords, man);
        return Ok(result);
    }

    /// <summary>
    ///     获取歌词内容。
    /// </summary>
    /// <param name="id">歌词候选 ID。</param>
    /// <param name="accesskey">歌词访问 Key。</param>
    /// <param name="fmt">歌词格式。</param>
    /// <param name="decode">是否解码歌词。</param>
    /// <returns>歌词内容和解析后的歌词行。</returns>
    [HttpGet("lyric")]
    [ProducesResponseType(typeof(LyricResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLyric(
        [FromQuery] string id,
        [FromQuery] string accesskey,
        [FromQuery] string fmt = "krc",
        [FromQuery] bool decode = true)
    {
        var result = await lyricClient.GetLyricAsync(id, accesskey, fmt, decode);
        return Ok(result);
    }
}
