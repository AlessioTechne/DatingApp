using API.Entities;
using API.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace API.Controller;

public class AccountController(UserManager<AppUser> userManager, ITokenServices tokenServices, IMapper mapper) : BaseApiController
{
    private UserManager<AppUser> _userManager = userManager;
    private readonly ITokenServices _tokenServices = tokenServices;
    private readonly IMapper _mapper = mapper;

    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
    {

        if (await UserExist(registerDTO.UserName)) { return BadRequest("Username is taken"); }

        var user = _mapper.Map<AppUser>(registerDTO);

        user.UserName = registerDTO.UserName.ToLower();

        var result = await _userManager.CreateAsync(user, registerDTO.Password);

        if (!result.Succeeded) return BadRequest(result.Errors);
        
        var roleResult = await _userManager.AddToRoleAsync(user, "Member");

        if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);

        return new UserDTO
        {
            UserName = user.UserName,
            Token = await _tokenServices.CreateToken(user),
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
    {
        var user = await _userManager.Users.Include(x => x.Photos)
                                            .SingleOrDefaultAsync(x => x.UserName == loginDTO.UserName.ToLower());
        if (user == null)
            return Unauthorized("Invalid username");

        var result = await _userManager.CheckPasswordAsync(user, loginDTO.Password);

        if (!result) return Unauthorized("Invalid Password");

        return new UserDTO
        {
            UserName = loginDTO.UserName,
            Token = await _tokenServices.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    private async Task<bool> UserExist(string username)
    {
        return await _userManager.Users.AnyAsync(x => x.UserName == username);
    }
}