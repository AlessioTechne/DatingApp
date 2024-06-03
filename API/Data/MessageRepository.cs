using API.DTO;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
{
    private readonly DataContext _context = context;
    private readonly IMapper _mapper = mapper;

    public void AddMessage(Message message)
    {
        _context.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        _context.Remove(message);
    }

    public async Task<Message> GetMessage(int id)
    {
        return await _context.Messages.FindAsync(id);
    }


    public async Task<PagedList<MessageDTO>> GetMessageForUser(MessageParams messageParams)
    {
        var query = _context.Messages.OrderByDescending(x => x.MessageSent).AsQueryable();

        query = messageParams.Container switch
        {
            "InBox" => query.Where(x => x.RecipientUsername == messageParams.UserName && x.RecipientDeleted == false),
            "OutBox" => query.Where(x => x.SenderUsername == messageParams.UserName && x.SenderDeleted == false),
            _ => query.Where(x => x.RecipientUsername == messageParams.UserName && x.DateRead == null && x.RecipientDeleted == false)
        };

        var messages = query.ProjectTo<MessageDTO>(_mapper.ConfigurationProvider);

        return await PagedList<MessageDTO>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUsername, string recipientUsername)
    {
        var messages = await _context.Messages
        .Include(u => u.Sender).ThenInclude(p => p.Photos)
        .Include(u => u.Recipient).ThenInclude(p => p.Photos)
        .Where(m => m.RecipientUsername == currentUsername && m.SenderUsername == recipientUsername && m.RecipientDeleted == false||
           m.RecipientUsername == recipientUsername && m.SenderUsername == currentUsername && m.SenderDeleted == false)
        .OrderByDescending(m => m.MessageSent)
        .ToListAsync();

        var unreadMessages = messages.Where(m => m.DateRead == null && m.RecipientUsername == currentUsername).ToList();

        if (unreadMessages.Any())
        {
            foreach (var message in unreadMessages)
            {
                message.DateRead = DateTime.Now;
            }

            await _context.SaveChangesAsync();

        }
        return _mapper.Map<IEnumerable<MessageDTO>>(messages);
    }
    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
