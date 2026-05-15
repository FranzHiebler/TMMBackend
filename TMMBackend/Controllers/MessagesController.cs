using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
	private readonly IMessageService _service;

	public MessagesController(IMessageService service)
	{
		_service = service;
	}

	[HttpGet("conversations")]
	public async Task<ActionResult<List<ConversationResponse>>> GetConversations()
	{
		return Ok(await _service.GetConversationsAsync());
	}

	[HttpGet("conversations/{conversationId}")]
	public async Task<ActionResult<ConversationDetailResponse>> GetConversation(string conversationId)
	{
		return Ok(await _service.GetConversationAsync(conversationId));
	}

	[HttpPost("conversations/{conversationId}/read")]
	public async Task<IActionResult> MarkConversationRead(string conversationId)
	{
		await _service.MarkConversationReadAsync(conversationId);
		return NoContent();
	}

	[HttpPost("direct")]
	public async Task<ActionResult<MessageResponse>> SendDirect(SendDirectMessageRequest request)
	{
		return Ok(await _service.SendDirectAsync(request));
	}
}
