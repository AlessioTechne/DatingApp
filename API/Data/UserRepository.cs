using API.DTO;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository(DataContext context, IMapper mapper) : IUserRepository
{
    private readonly DataContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<MemberDTO>> GetMembersAsync()
    {
        return await _context.Users.ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
                                   .ToListAsync();
    }

    public async Task<MemberDTO> GetMemberByUsernameAsync(string username)
    {
        return await _context.Users.Where(x => x.UserName == username)
                                   .ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
                                   .SingleOrDefaultAsync();
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.Include(p => p.Photos)
                                   .SingleOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await _context.Users.Include(p => p.Photos)
                                   .ToListAsync();
    }

    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public void Update(AppUser appUser)
    {
        _context.Entry(appUser).State = EntityState.Modified;
    }
}
