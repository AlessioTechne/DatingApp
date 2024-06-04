using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenServices(IConfiguration configuration, UserManager<AppUser> userManager) : ITokenServices
{
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(configuration["TokenKey"]));
    private readonly UserManager<AppUser> _userManager = userManager;

    public async Task<string> CreateToken(AppUser user)
    {
        var claims = new List<Claim>(){
            new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName)
        };

        var roles = await _userManager.GetRolesAsync(user);

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}