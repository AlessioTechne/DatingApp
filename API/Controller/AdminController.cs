using API.Entities;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controller;
public class AdminController(UserManager<AppUser> userManager) : BaseApiController
{
   private readonly UserManager<AppUser> _userManager = userManager;

   [Authorize(Policy = "RequireAdminRole")]
   [HttpGet("users-with-roles")]
   public async Task<ActionResult> GetUserWithRoles()
   {
      var user = await _userManager.Users.OrderBy(x => x.UserName)
         .Select(x => new
         {
            x.Id,
            Username = x.UserName,
            Roles = x.UserRoles.Select(x => x.Roles.Name).ToList()
         }).ToListAsync();

      return Ok(user);
   }

   [Authorize(Policy = "RequireAdminRole")]
   [HttpPost("edit-roles/{username}")]
   public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
   {
      if (string.IsNullOrEmpty(roles))
         return BadRequest("You must select at least one role");

      var selectedRoles = roles.Split(",").ToArray();

      var user = await _userManager.FindByNameAsync(username);

      if (user == null)
         return NotFound();

      var userRoles = await _userManager.GetRolesAsync(user);

      var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

      if (!result.Succeeded) return BadRequest("Failed to add to roles");

      result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

      if (!result.Succeeded) return BadRequest("Failed to remove to roles");

      return Ok(await _userManager.GetRolesAsync(user));
   }


   [Authorize(Policy = "ModeratePhotoRole")]
   [HttpGet("photos-to-moderate")]
   public ActionResult GetPhotoForModeration()
   {
      return Ok("admin ormod can see this");
   }
}
