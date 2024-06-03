using API.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller;

public class MessagesController(IUserRepository user, IMessageRepository message, IMapper mapper) : BaseApiController
{
    private readonly IUserRepository _userRepository = user;
    private readonly IMessageRepository _messageRepository = message;
    private readonly IMapper _mapper = mapper;

    [HttpPost]
    public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
    {
        var username = User.GetUsername();

        if (username == createMessageDTO.RecipientUsername.ToLower()) return BadRequest("You cannot send message to ypurself");

        var sender = await _userRepository.GetUserByUsernameAsync(username);
        var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDTO.RecipientUsername);

        if (recipient == null) return NotFound();

        var message = new Message
        {
            SenderUsername = sender.UserName,
            Sender = sender,
            RecipientUsername = recipient.UserName,
            Recipient = recipient,
            Content = createMessageDTO.Content
        };

        _messageRepository.AddMessage(message);

        if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDTO>(message));

        return BadRequest("Error on save message");
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MessageDTO>>> GetMessageForUser([FromQuery] MessageParams messageParams)
    {
        messageParams.UserName = User.GetUsername();
        var message = await _messageRepository.GetMessageForUser(messageParams);
        Response.AddPaginationHeader(new PaginationHeader(message.CurrentPage, message.PageSize, message.TotalCount, message.TotalPages));
        return message;
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<MessageDTO>> GetMessageThread(string username)
    {
        var currentUsername = User.GetUsername();

        return Ok(await _messageRepository.GetMessageThread(currentUsername, username));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUsername();

        var message = await _messageRepository.GetMessage(id);

        if (message.SenderUsername != username && message.RecipientUsername != username)
            return Unauthorized();

        if (message.SenderUsername == username) message.SenderDeleted = true;
        if (message.RecipientUsername == username) message.RecipientDeleted = true;

        if (message.SenderDeleted && message.RecipientDeleted)
        {
            _messageRepository.DeleteMessage(message);
        }

        if(await _messageRepository.SaveAllAsync())return Ok();

        return BadRequest("Error on deleting the message");
    }
}
