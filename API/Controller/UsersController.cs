using API.Data;
using API.DTO;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

namespace API.Controller;


[Authorize]
public class UsersController(IUserRepository repository) : BaseApiController
{
    private readonly IUserRepository _repository = repository;

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
}
