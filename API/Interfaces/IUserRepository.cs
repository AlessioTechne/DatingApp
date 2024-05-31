using API.DTO;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IUserRepository
{
    void Update(AppUser appUser);
    Task<bool> SaveAllAsync();
    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams);
    Task<AppUser> GetUserByUsernameAsync(string username);
    Task<MemberDTO> GetMemberByUsernameAsync(string username);
    Task<AppUser> GetUserByIdAsync(int id);
}
