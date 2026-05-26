using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/Games")]
public class GameMessagesController : ControllerBase
{
	private readonly IMessageService _messages;

	public GameMessagesController(IMessageService messages)
	{
		_messages = messages;
	}

	[HttpGet("{id}/messages")]
	public async Task<ActionResult<List<MessageResponse>>> GetMessages(string id)
	{
		return Ok(await _messages.GetGameMessagesAsync(id));
	}

	[HttpPost("{id}/messages")]
	public async Task<ActionResult<MessageResponse>> SendMessage(
		string id,
		[FromBody] SendGameSessionMessageRequest request)
	{
		return Ok(await _messages.SendGameMessageAsync(id, request));
	}

	[HttpGet("{id}/tables/{tableId}/messages")]
	public async Task<ActionResult<List<MessageResponse>>> GetTableMessages(string id, string tableId)
	{
		return Ok(await _messages.GetTableMessagesAsync(id, tableId));
	}

	[HttpPost("{id}/tables/{tableId}/messages")]
	public async Task<ActionResult<MessageResponse>> SendTableMessage(
		string id,
		string tableId,
		[FromBody] SendGameTableMessageRequest request)
	{
		return Ok(await _messages.SendTableMessageAsync(id, tableId, request));
	}
}
