using API.DTO;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface ILikeRepository
{
    Task<UserLike> GetUserLike(int sourceuserId, int targetUserId);
    Task<AppUser> GetUserWithLike(int userId);
    Task<PagedList<LikeDTO>> GetUserLike(LikesParams likesParams);
}
