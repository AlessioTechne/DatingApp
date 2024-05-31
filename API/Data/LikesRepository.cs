using API.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository(DataContext context) : ILikeRepository
{
    private readonly DataContext _context = context;
    public async Task<UserLike> GetUserLike(int sourceuserId, int targetUserId)
    {
        return await _context.Likes.FindAsync(sourceuserId, targetUserId);
    }

    public async Task<PagedList<LikeDTO>> GetUserLike(LikesParams likesParams)
    {
        var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
        var likes = _context.Likes.AsQueryable();

        if (likesParams.Predicate == "liked")
        {
            likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
            users = likes.Select(like => like.TargetUser);
        }
        if (likesParams.Predicate == "likedBy")
        {
            likes = likes.Where(like => like.TargetUserId == likesParams.UserId);
            users = likes.Select(like => like.SourceUser);
        }

        var likedUsers = users.Select(
            user => new LikeDTO
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
                Id = user.Id,
            }
        );

        return await PagedList<LikeDTO>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
    }

    public async Task<AppUser> GetUserWithLike(int userId)
    {
        return await _context.Users.Include(u => u.LikedUser)
                                   .FirstOrDefaultAsync(x => x.Id == userId);
    }
}
