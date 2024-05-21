using API.Data;
using API.Entities;
<<<<<<< HEAD
using Microsoft.AspNetCore.Authorization;
=======
>>>>>>> 397e4f4e700a50188659627e5d43bae6f081e4d6
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controller;

<<<<<<< HEAD
[Authorize]
public class UsersController(DataContext datacontext) : BaseApiController
=======
[ApiController]
[Route("api/[controller]")]
public class UsersController(DataContext datacontext) : ControllerBase
>>>>>>> 397e4f4e700a50188659627e5d43bae6f081e4d6
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
