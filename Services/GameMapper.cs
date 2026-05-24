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
			TimingMode = game.TimingMode,
			TimeLabel = game.TimeLabel,
			Description = game.Description,
			Tables = game.Tables.Select(ToTableDto).ToList(),
			ChangeProposals = game.ChangeProposals.Select(ToProposalDto).ToList(),
			DateOptions = game.DateOptions.Select(ToDateOptionDto).ToList(),
			Invitations = game.Invitations.Select(ToInvitationDto).ToList(),
			Waitlist = game.Waitlist.Select(ToWaitlistDto).ToList(),
			Result = game.Result == null ? null : ToResultDto(game.Result),
			PublicSlug = game.PublicSlug,
			SeriesId = game.SeriesId
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

	public static PublicGameResponse ToPublicResponse(GameSession game)
	{
		var response = ToResponse(game);
		return new PublicGameResponse
		{
			Id = response.Id,
			Title = response.Title,
			Status = response.Status,
			StartTimeUtc = response.StartTimeUtc,
			TimingMode = response.TimingMode,
			TimeLabel = response.TimeLabel,
			Location = response.Location,
			Description = response.Description,
			Tables = response.Tables.Select(table => new GameTableDto
			{
				Id = table.Id,
				Name = table.Name,
				MaxPlayers = table.MaxPlayers,
				Systems = table.Systems,
				Scenario = table.Scenario,
				Points = table.Points,
				StartTimeUtc = table.StartTimeUtc,
				Notes = table.Notes
			}).ToList(),
			OpenSlots = response.OpenSlots
		};
	}

	private static SessionDateOptionDto ToDateOptionDto(SessionDateOption option)
	{
		return new SessionDateOptionDto
		{
			Id = option.Id,
			StartTimeUtc = option.StartTimeUtc,
			Label = option.Label,
			Votes = option.Votes.Select(ToParticipantDto).ToList(),
			CreatedAtUtc = option.CreatedAtUtc
		};
	}

	private static SessionInvitationDto ToInvitationDto(SessionInvitation invitation)
	{
		return new SessionInvitationDto
		{
			Id = invitation.Id,
			User = ToParticipantDto(invitation.User),
			Status = invitation.Status,
			CreatedAtUtc = invitation.CreatedAtUtc,
			RespondedAtUtc = invitation.RespondedAtUtc
		};
	}

	private static WaitlistEntryDto ToWaitlistDto(WaitlistEntry entry)
	{
		return new WaitlistEntryDto
		{
			Id = entry.Id,
			TableId = entry.TableId,
			Player = ToParticipantDto(entry.Player),
			SystemKey = entry.SystemKey,
			Message = entry.Message,
			CreatedAtUtc = entry.CreatedAtUtc
		};
	}

	private static GameResultDto ToResultDto(GameResult result)
	{
		return new GameResultDto
		{
			Kind = result.Kind,
			Value = result.Value,
			Notes = result.Notes,
			RecordedBy = ToParticipantDto(result.RecordedBy),
			RecordedAtUtc = result.RecordedAtUtc
		};
	}
}
