using API.DTO;
using API.Entities;

namespace API.Interfaces;

public interface IUserRepository
{
    void Update(AppUser appUser);
    Task<bool> SaveAllAsync();
    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<IEnumerable<MemberDTO>> GetMembersAsync();
    Task<AppUser> GetUserByUsernameAsync(string username);
    Task<MemberDTO> GetMemberByUsernameAsync(string username);

}
