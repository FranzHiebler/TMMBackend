using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services;

public static class GameMapper
{
	public static GameResponse ToResponse(GameSession game)
	{
		return new GameResponse
		{
			Id = game.Id!,
			Title = game.Title,
			Host = ToParticipantDto(game.Host),
			Status = game.Status,
			JoinMode = game.JoinMode,
			LocationId = game.LocationId,
			Location = new LocationSnapshotDto
			{
				Name = game.LocationSnapshot.Name,
				City = game.LocationSnapshot.City
			},
			ClubId = game.ClubId,
			StartTimeUtc = game.StartTimeUtc,
			Description = game.Description,
			Tables = game.Tables.Select(ToTableDto).ToList(),
			ChangeProposals = game.ChangeProposals.Select(ToProposalDto).ToList()
		};
	}

	private static GameTableDto ToTableDto(GameTable table)
	{
		return new GameTableDto
		{
			Id = table.TableId,
			Name = table.Name,
			MaxPlayers = table.MaxPlayers,
			Systems = table.Systems,
			Scenario = table.Scenario,
			Points = table.Points,
			StartTimeUtc = table.StartTimeUtc,
			Notes = table.Notes,
			AssignedPlayers = table.AssignedPlayers.Select(ToParticipantDto).ToList(),
			Applications = table.Applications.Select(ToApplicationDto).ToList()
		};
	}

	private static TableApplicationDto ToApplicationDto(TableApplication application)
	{
		return new TableApplicationDto
		{
			Id = application.ApplicationId,
			TableId = application.TableId,
			Player = ToParticipantDto(application.Player),
			SystemKey = application.SystemKey,
			Message = application.Message,
			Status = application.Status,
			CreatedAt = application.CreatedAt
		};
	}

	private static GameChangeProposalDto ToProposalDto(GameChangeProposal proposal)
	{
		return new GameChangeProposalDto
		{
			Id = proposal.ProposalId,
			TableId = proposal.TableId,
			ProposedBy = ToParticipantDto(proposal.ProposedBy),
			ProposedStartTimeUtc = proposal.ProposedStartTimeUtc,
			ProposedSystems = proposal.ProposedSystems,
			ProposedPoints = proposal.ProposedPoints,
			Message = proposal.Message,
			Status = proposal.Status,
			CreatedAt = proposal.CreatedAt,
			ResolvedAt = proposal.ResolvedAt
		};
	}

	private static ParticipantDto ToParticipantDto(ParticipantInfo participant)
	{
		return new ParticipantDto
		{
			UserId = participant.UserId,
			DisplayName = participant.DisplayName
		};
	}
}