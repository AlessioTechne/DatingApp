using API.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller;

public class LikesController(IUserRepository userRepository, ILikeRepository likeRepository) : BaseApiController
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ILikeRepository _likeRepository = likeRepository;

    [HttpPost("{username}")]
    public async Task<IActionResult> AddLike(string username)
    {
        var sourceId = User.GetUserId();
        var likedUser = await _userRepository.GetUserByUsernameAsync(username);
        var sourceUser = await _likeRepository.GetUserWithLike(sourceId);

        if (likedUser == null) return NotFound();
        if (sourceUser.UserName == username) return BadRequest("You cannot like yourself");

        var userlike = await _likeRepository.GetUserLike(sourceId, likedUser.Id);

        if (userlike != null) return BadRequest("You already like this user");

        userlike = new UserLike
        {
            SourceUserId = sourceId,
            TargetUserId = likedUser.Id
        };

        sourceUser.LikedUser.Add(userlike);

        if (await _userRepository.SaveAllAsync()) return Ok();

        return BadRequest("Failed to like user");
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<LikeDTO>>> GetUserLikes([FromQuery] LikesParams likesParams)
    {
        likesParams.UserId = User.GetUserId();

        var user = await _likeRepository.GetUserLike(likesParams);

        Response.AddPaginationHeader(new PaginationHeader(user.CurrentPage, user.PageSize, user.TotalCount, user.TotalPages));
        return Ok(user);
    }
}
