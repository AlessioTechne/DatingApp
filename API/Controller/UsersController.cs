using API.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

namespace API.Controller;


[Authorize]
public class UsersController(IUserRepository repository, IMapper mapper, IPhotoServices photoServices) : BaseApiController
{
    private readonly IUserRepository _repository = repository;
    private readonly IMapper _mapper = mapper;
    private readonly IPhotoServices _photoServices = photoServices;

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<PagedList<MemberDTO>>> GetUsers([FromQuery] UserParams userParams)
    {
        var currentUser = await _repository.GetUserByUsernameAsync(User.GetUsername());
        userParams.CurrentUsername = currentUser.UserName;

        if (string.IsNullOrEmpty(userParams.Gender))
        {
            userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
        }

        var user = await _repository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(new PaginationHeader(user.CurrentPage, user.PageSize, user.TotalCount, user.TotalPages));

        return Ok(user);
    }

    [Authorize(Roles = "Member")]
    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDTO>> GetUserByUsernameAsync(string username)
    {
        return Ok(await _repository.GetMemberByUsernameAsync(username));
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
    {
        var user = await _repository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null)
            return NotFound();

        _mapper.Map(memberUpdateDTO, user);

        if (await _repository.SaveAllAsync())
            return NoContent();

        return BadRequest("Failed to update user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
    {
        var user = await _repository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null)
            return NotFound();

        var result = await _photoServices.AddPhotoAsync(file);

        if (result.Error != null)
            return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        if (user.Photos.Count == 0)
            photo.IsMain = true;

        user.Photos.Add(photo);

        if (await _repository.SaveAllAsync())
            return CreatedAtAction(nameof(GetUsers), new { username = user.UserName }, _mapper.Map<PhotoDTO>(photo));

        return BadRequest("Problem adding photo");

    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await _repository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null)
            return NotFound();

        var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);

        if (photo == null)
            return NotFound();

        if (photo.IsMain)
            return BadRequest("This is Already the main photo");

        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        if (currentMain != null)
            currentMain.IsMain = false;

        photo.IsMain = true;

        if (await _repository.SaveAllAsync()) return NoContent();


        return BadRequest("Error on change main photo");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await _repository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null)
            return NotFound();

        var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);

        if (photo == null)
            return NotFound();

        if (photo.IsMain)
            return BadRequest("You can't delete your main photo");

        if (photo.PublicId != null)
        {
            var result = await _photoServices.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null)
                return BadRequest(result.Error.Message);
        }
        user.Photos.Remove(photo);

        if (await _repository.SaveAllAsync()) return Ok();

        return BadRequest("Error on delete photo");
    }
}
