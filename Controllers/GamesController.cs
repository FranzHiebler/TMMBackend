using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
	private readonly IGameService _service;
	private readonly IMessageService _messages;
	private readonly IDiscoveryService _discovery;

	public GamesController(
		IGameService service,
		IMessageService messages,
		IDiscoveryService discovery)
	{
		_service = service;
		_messages = messages;
		_discovery = discovery;
	}

	[HttpPost]
	public async Task<ActionResult<GameResponse>> Create(CreateGameRequest request)
	{
		var result = await _service.CreateAsync(request);
		return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<GameResponse>> GetById(string id)
	{
		var game = await _service.GetByIdAsync(id);

		if (game == null)
			return NotFound(new { error = "Game Session wurde nicht gefunden." });

		return Ok(game);
	}

	[HttpGet("public/{slugOrId}")]
	public async Task<ActionResult<PublicGameResponse>> GetPublic(string slugOrId)
	{
		var game = await _service.GetPublicAsync(slugOrId);
		return game == null ? NotFound(new { error = "Session wurde nicht gefunden." }) : Ok(game);
	}

	[HttpGet("calendar")]
	public async Task<ActionResult<List<CalendarItemResponse>>> Calendar()
	{
		return Ok(await _service.GetCalendarAsync());
	}

	[HttpPost("{id}/tables/{tableId}/join")]
	public async Task<IActionResult> JoinTable(
		string id,
		string tableId,
		[FromBody] JoinTableRequest request)
	{
		await _service.JoinTableAsync(id, tableId, request);
		return NoContent();
	}

	[HttpPost("{id}/apply")]
	public async Task<IActionResult> Apply(
		string id,
		[FromBody] ApplyToGameRequest request)
	{
		await _service.ApplyAsync(id, request);
		return NoContent();
	}

	[HttpPost("{id}/tables/{tableId}/assign")]
	public async Task<IActionResult> AssignPlayerToTable(
		string id,
		string tableId,
		[FromBody] AssignPlayerToTableRequest request)
	{
		await _service.AssignPlayerToTableAsync(id, tableId, request);
		return NoContent();
	}

	[HttpGet("search")]
	public async Task<ActionResult<List<GameResponse>>> Search([FromQuery] SearchGamesRequest request)
	{
		return Ok(await _service.SearchAsync(request));
	}

	[HttpGet("nearby")]
	public async Task<ActionResult<List<GameResponse>>> Nearby([FromQuery] SearchNearbyGamesRequest request)
	{
		return Ok(await _service.SearchNearbyAsync(request));
	}

	[HttpGet("discovery")]
	public async Task<ActionResult<List<GameDiscoveryResponse>>> Discovery([FromQuery] DiscoveryGamesRequest request)
	{
		return Ok(await _discovery.GetGamesAsync(request));
	}

	[HttpPost("{id}/applications/{applicationId}/reject")]
	public async Task<IActionResult> RejectApplication(string id, string applicationId)
	{
		await _service.RejectApplicationAsync(id, applicationId);
		return NoContent();
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

	[HttpPost("{id}/change-proposals")]
	public async Task<ActionResult<GameResponse>> CreateChangeProposal(
		string id,
		[FromBody] CreateChangeProposalRequest request)
	{
		return Ok(await _service.CreateChangeProposalAsync(id, request));
	}

	[HttpPost("{id}/change-proposals/{proposalId}/accept")]
	public async Task<ActionResult<GameResponse>> AcceptChangeProposal(string id, string proposalId)
	{
		return Ok(await _service.AcceptChangeProposalAsync(id, proposalId));
	}

	[HttpPost("{id}/change-proposals/{proposalId}/reject")]
	public async Task<ActionResult<GameResponse>> RejectChangeProposal(string id, string proposalId)
	{
		return Ok(await _service.RejectChangeProposalAsync(id, proposalId));
	}

	[HttpPost("{id}/tables/{tableId}/players/{userId}/remove")]
	public async Task<IActionResult> RemovePlayerFromTable(
		string id,
		string tableId,
		string userId)
	{
		await _service.RemovePlayerFromTableAsync(id, tableId, userId);
		return NoContent();
	}

	[HttpPost("{id}/players/{userId}/move")]
	public async Task<IActionResult> MovePlayerToTable(
		string id,
		string userId,
		[FromBody] MovePlayerToTableRequest request)
	{
		await _service.MovePlayerToTableAsync(id, userId, request);
		return NoContent();
	}

	[HttpPut("{id}")]
	public async Task<ActionResult<GameResponse>> UpdateSession(
	string id,
	[FromBody] UpdateGameSessionRequest request)
	{
		return Ok(await _service.UpdateSessionAsync(id, request));
	}

	[HttpPut("{id}/tables/{tableId}")]
	public async Task<ActionResult<GameResponse>> UpdateTable(
		string id,
		string tableId,
		[FromBody] UpdateGameTableRequest request)
	{
		return Ok(await _service.UpdateTableAsync(id, tableId, request));
	}

	[HttpPost("{id}/date-options")]
	public async Task<ActionResult<GameResponse>> AddDateOption(string id, AddDateOptionRequest request)
	{
		return Ok(await _service.AddDateOptionAsync(id, request));
	}

	[HttpPost("{id}/date-options/{optionId}/vote")]
	public async Task<ActionResult<GameResponse>> VoteDateOption(string id, string optionId)
	{
		return Ok(await _service.VoteDateOptionAsync(id, optionId));
	}

	[HttpPost("{id}/date-options/{optionId}/select")]
	public async Task<ActionResult<GameResponse>> SelectDateOption(string id, string optionId)
	{
		return Ok(await _service.SelectDateOptionAsync(id, optionId));
	}

	[HttpPost("{id}/invitations")]
	public async Task<ActionResult<GameResponse>> InviteFriend(string id, InviteFriendToSessionRequest request)
	{
		return Ok(await _service.InviteFriendAsync(id, request));
	}

	[HttpPost("{id}/invitations/{invitationId}/accept")]
	public async Task<ActionResult<GameResponse>> AcceptInvitation(string id, string invitationId)
	{
		return Ok(await _service.RespondInvitationAsync(id, invitationId, true));
	}

	[HttpPost("{id}/invitations/{invitationId}/reject")]
	public async Task<ActionResult<GameResponse>> RejectInvitation(string id, string invitationId)
	{
		return Ok(await _service.RespondInvitationAsync(id, invitationId, false));
	}

	[HttpPost("{id}/waitlist")]
	public async Task<ActionResult<GameResponse>> JoinWaitlist(string id, JoinWaitlistRequest request)
	{
		return Ok(await _service.JoinWaitlistAsync(id, request));
	}

	[HttpPost("{id}/waitlist/{entryId}/promote")]
	public async Task<ActionResult<GameResponse>> PromoteWaitlist(string id, string entryId, [FromQuery] string tableId)
	{
		return Ok(await _service.PromoteWaitlistEntryAsync(id, entryId, tableId));
	}

	[HttpPost("{id}/close")]
	public async Task<ActionResult<GameResponse>> Close(string id, CloseGameRequest request)
	{
		return Ok(await _service.CloseGameAsync(id, request));
	}
}
