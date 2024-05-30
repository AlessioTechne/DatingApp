using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.Entities;
using API.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;
using AutoMapper;

namespace API.Controller;

public class AccountController(DataContext datacontext, ITokenServices tokenServices, IMapper mapper) : BaseApiController
{
    private readonly DataContext _datacontext = datacontext;
    private readonly ITokenServices _tokenServices = tokenServices;
    private readonly IMapper _mapper = mapper;

    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
    {

        if (await UserExist(registerDTO.UserName)) { return BadRequest("Username is taken"); }

        var user = _mapper.Map<AppUser>(registerDTO);

        using var hmac = new HMACSHA512();
        user.UserName = registerDTO.UserName.ToLower();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
        user.PasswordSalt = hmac.Key;

        _datacontext.Users.Add(user);
        await _datacontext.SaveChangesAsync();

        return new UserDTO{
            UserName = registerDTO.UserName,
            Token = _tokenServices.CreateToken(user),
            KnownAs = registerDTO.KnownAs
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
    {
        var user = await _datacontext.Users.Include(x => x.Photos)
                                            .FirstOrDefaultAsync(x => x.UserName == loginDTO.UserName.ToLower());
        if (user == null)
        {
            return Unauthorized();
        }

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
        }

        return new UserDTO
        {
            UserName = loginDTO.UserName,
            Token = _tokenServices.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
            KnownAs = user.KnownAs
        };
    }

    private async Task<bool> UserExist(string username)
    {
        return await _datacontext.Users.AnyAsync(x => x.UserName == username);
    }
}