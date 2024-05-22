using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.Entities;
using API.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;

namespace API.Controller;

public class AccountController(DataContext datacontext, ITokenServices tokenServices) : BaseApiController
{
    private readonly DataContext _datacontext = datacontext;
    private readonly ITokenServices _tokenServices = tokenServices;

    [HttpPost("register")]
    public async Task<ActionResult<AppUser>> Register(RegisterDTO registerDTO){
        
        if(await UserExist(registerDTO.UserName)){return BadRequest("Username is taken");}
        
        using var hmac = new HMACSHA512();
        
        var user = new AppUser{
            UserName = registerDTO.UserName.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
            PasswordSalt = hmac.Key
        };
        
        _datacontext.Users.Add(user);
        await _datacontext.SaveChangesAsync();
        
        return user;
    }

[HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO){
        var user = await _datacontext.Users.FirstOrDefaultAsync(x => x.UserName == loginDTO.UserName);
        if(user == null){
            return Unauthorized();
        }

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

        for(int i = 0; i<computedHash.Length; i++) {
            if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
        }

         return new UserDTO{
            UserName = loginDTO.UserName,
            Token = _tokenServices.CreateToken(user)
        };
    }

    private async Task<bool> UserExist(string username){
        return await _datacontext.Users.AnyAsync(x=> x.UserName==username);
    }
}