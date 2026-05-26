using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/Games")]
public class GamePlanningController : ControllerBase
{
	private readonly IGameService _service;

	public GamePlanningController(IGameService service)
	{
		_service = service;
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

	[HttpPost("{id}/close")]
	public async Task<ActionResult<GameResponse>> Close(string id, CloseGameRequest request)
	{
		return Ok(await _service.CloseGameAsync(id, request));
	}

	[HttpPost("{id}/cancel")]
	public async Task<ActionResult<GameResponse>> Cancel(string id)
	{
		return Ok(await _service.CancelGameAsync(id));
	}
}
