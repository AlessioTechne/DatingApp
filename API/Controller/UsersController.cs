using System.Security.Claims;
using API.DTO;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

namespace API.Controller;


[Authorize]
public class UsersController(IUserRepository repository, IMapper mapper) : BaseApiController
{
    private readonly IUserRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers()
    {
        return Ok(await _repository.GetMembersAsync());
    }
   
    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDTO>> GetUserByUsernameAsync(string username)
    {
        return Ok(await _repository.GetMemberByUsernameAsync(username));
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO){
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _repository.GetUserByUsernameAsync(username);
        if (user == null) return NotFound();
        _mapper.Map(memberUpdateDTO, user);
        if(await _repository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update user");
    }
}
