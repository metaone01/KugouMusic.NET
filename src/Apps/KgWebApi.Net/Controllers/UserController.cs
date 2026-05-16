using KuGou.Net.Abstractions.Models;
using KuGou.Net.Clients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KgWebApi.Net.Controllers;

[ApiController]
[Route("user")]
public class UserController(UserClient userClient) : ControllerBase
{
    /// <summary>
    ///     获取当前登录用户详情。
    /// </summary>
    /// <returns>用户详情。</returns>
    [HttpGet("detail")]
    [ProducesResponseType(typeof(UserDetailModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> UserDetail()
    {
        var result = await userClient.GetUserInfoAsync();
        return Ok(result);
    }

    /// <summary>
    ///     获取当前登录用户 VIP 信息。
    /// </summary>
    /// <returns>用户 VIP 信息。</returns>
    [HttpGet("vip/detail")]
    [ProducesResponseType(typeof(UserVipResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UserVipDetail()
    {
        var result = await userClient.GetVipInfoAsync();
        return Ok(result);
    }

    /// <summary>
    ///     分页获取当前登录用户歌单。
    /// </summary>
    /// <param name="page">页码。</param>
    /// <param name="pagesize">每页数量。</param>
    /// <returns>用户歌单结果。</returns>
    [HttpGet("playlist")]
    [ProducesResponseType(typeof(UserPlaylistResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UserPlaylist(
        [FromQuery] int page = 1,
        [FromQuery] int pagesize = 30)
    {
        var result = await userClient.GetPlaylistsAsync(page, pagesize);
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> UserHistory()
    {
        var result = await userClient.GetPlayHistoryAsync();
        return Ok(result);
    }

    [HttpGet("listen")]
    public async Task<IActionResult> UserListen()
    {
        var result = await userClient.GetListenRankAsync();
        return Ok(result);
    }

    [HttpGet("follow")]
    public async Task<IActionResult> UserFollow()
    {
        var result = await userClient.GetFollowedSingersAsync();
        return Ok(result);
    }

    [HttpGet("cloud")]
    public async Task<IActionResult> UserCloud(
        [FromQuery] int page = 1,
        [FromQuery] int pagesize = 30)
    {
        var result = await userClient.GetCloudAsync(page, pagesize);
        return Ok(result);
    }

    [HttpGet("cloud/url")]
    public async Task<IActionResult> UserCloudUrl(
        [FromQuery][Required(AllowEmptyStrings = false)] string hash,
        [FromQuery(Name = "album_audio_id")] string? albumAudioId = null,
        [FromQuery(Name = "audio_id")] string? audioId = null,
        [FromQuery] string? name = null)
    {
        var result = await userClient.GetCloudUrlAsync(hash, albumAudioId, audioId, name);
        return Ok(result);
    }

    [HttpGet("follow/message")]
    public async Task<IActionResult> UserFollowMessage(
        [FromQuery(Name = "id")][Required(AllowEmptyStrings = false)] string artistId,
        [FromQuery] int pagesize = 30)
    {
        var result = await userClient.GetFollowMessagesAsync(artistId, pagesize);
        return Ok(result);
    }

    [HttpGet("video/collect")]
    public async Task<IActionResult> UserVideoCollect(
        [FromQuery] int page = 1,
        [FromQuery] int pagesize = 30)
    {
        var result = await userClient.GetCollectedVideosAsync(page, pagesize);
        return Ok(result);
    }

    [HttpGet("video/love")]
    public async Task<IActionResult> UserVideoLove([FromQuery] int pagesize = 30)
    {
        var result = await userClient.GetLikedVideosAsync(pagesize);
        return Ok(result);
    }

    [HttpGet("/favorite/count")]
    public async Task<IActionResult> FavoriteCount([FromQuery][Required(AllowEmptyStrings = false)] string mixsongids)
    {
        var result = await userClient.GetFavoriteCountAsync(mixsongids);
        return Ok(result);
    }

    [HttpPost("/server/now")]
    public async Task<IActionResult> ServerNow()
    {
        var result = await userClient.GetServerNowAsync();
        return Ok(result);
    }
}
