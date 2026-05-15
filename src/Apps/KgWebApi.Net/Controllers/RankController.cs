using KuGou.Net.Abstractions.Models;
using KuGou.Net.Clients;
using Microsoft.AspNetCore.Mvc;

namespace KgWebApi.Net.Controllers;

[ApiController]
[Route("rank")]
public class RankController(RankClient rankClient) : ControllerBase
{
    /// <summary>
    ///     获取所有榜单
    /// </summary>
    /// <param name="withsong">是否包含榜单歌曲。</param>
    /// <returns>榜单列表。</returns>
    [HttpGet("list")]
    [ProducesResponseType(typeof(RankListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRankList([FromQuery] int withsong = 1)
    {
        var result = await rankClient.GetAllRanksAsync(withsong);
        return Ok(result);
    }

    [HttpGet("info")]
    public async Task<IActionResult> GetRankInfo(
        [FromQuery] int rankid,
        [FromQuery(Name = "rank_cid")] int? rankCid = null,
        [FromQuery(Name = "album_img")] int albumImg = 1,
        [FromQuery] string? zone = null)
    {
        var result = await rankClient.GetRankInfoRawAsync(rankid, rankCid, albumImg, zone);
        return Ok(result);
    }

    /// <summary>
    ///     获取榜单歌曲
    /// </summary>
    /// <param name="rankid">榜单 ID。</param>
    /// <param name="page">页码。</param>
    /// <param name="pagesize">每页数量。</param>
    /// <returns>榜单歌曲分页结果。</returns>
    [HttpGet("audio")]
    [ProducesResponseType(typeof(RankSongResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRankSongs(
        [FromQuery] int rankid,
        [FromQuery] int page = 1,
        [FromQuery] int pagesize = 30)
    {
        var result = await rankClient.GetRankSongsAsync(rankid, page, pagesize);
        return Ok(result);
    }
}
