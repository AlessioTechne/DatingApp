using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controller;

[ApiController]
[Route("api/[controller]")]
public class UsersController(DataContext datacontext) : ControllerBase
{
    private readonly DataContext _datacontext = datacontext;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers(){
        var users = await _datacontext.Users.ToListAsync();

        return users;
    }

    
    [HttpGet("{id}")]
    public async Task<ActionResult<AppUser>> GetSingleUsers(int id)
    {
        var user = await _datacontext.Users.FindAsync(id);

        return user;
    }
}
