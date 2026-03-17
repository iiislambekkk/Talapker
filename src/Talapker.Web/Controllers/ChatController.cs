using Microsoft.AspNetCore.Mvc;
using Talapker.Application.ChatFeatures;
using Talapker.Application.ChatFeatures.Comands;
using Talapker.Application.ChatFeatures.Queries;
using Wolverine;

namespace Talapker.Web.Controllers;

[ApiController]
[Route("api/chats")]
public class ChatController(IMessageBus messageBus) : ControllerBase
{
    [HttpGet("prospect")]
    public async Task<ActionResult<List<ChatRoomDto>>> GetProspectChats(
        [FromQuery] string prospectId,
        [FromQuery] Guid institutionId)
    {
        return await messageBus.InvokeAsync<List<ChatRoomDto>>(
            new GetProspectChatsQuery(prospectId, institutionId));
    }
    
    [HttpGet("room/{chatRoomId:guid}")]
    public async Task<ActionResult<ChatRoomDto>> GetRoom(Guid chatRoomId)
    {
        return await messageBus.InvokeAsync<ChatRoomDto>(new GetChatRoomQuery(chatRoomId));
    }

    [HttpPost("room")]
    public async Task<ActionResult<ChatRoomDto>> GetOrCreateRoom([FromBody] GetOrCreateChatRoomRequest request)
    {
        return await messageBus.InvokeAsync<ChatRoomDto>(
            new GetOrCreateChatRoomCommand(request.ProspectId, request.AmbassadorId, request.InstitutionId));
    }

    [HttpGet("room/{chatRoomId:guid}/messages")]
    public async Task<ActionResult<List<ChatMessageDto>>> GetMessages(
        Guid chatRoomId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        return await messageBus.InvokeAsync<List<ChatMessageDto>>(
            new GetChatMessagesQuery(chatRoomId, page, pageSize));
    }
    
    [HttpGet("ambassador")]
    public async Task<ActionResult<List<ChatRoomDto>>> GetAmbassadorChats([FromQuery] Guid userId)
    {
        return await messageBus.InvokeAsync<List<ChatRoomDto>>(new GetAmbassadorChatsQuery(userId));
    }
}