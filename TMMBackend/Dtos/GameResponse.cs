using TMMBackend.Domain;

namespace TabletopMatchMaker.Dtos;

public class GameResponse
{
	public string Id { get; set; } = default!;

	public string Title { get; set; } = default!;

	public SystemDto System { get; set; } = new();

	public ParticipantDto Host { get; set; } = new();

	public List<ParticipantDto> Participants { get; set; } = new();

	public int MaxPlayers { get; set; }

	public int OpenSlots => MaxPlayers - Participants.Count;

	public GameSessionState Status { get; set; } = default!;

	public string LocationId { get; set; } = default!;

	public LocationSnapshotDto Location { get; set; } = new();

	public string? ClubId { get; set; }

	public DateTime StartTimeUtc { get; set; }

	public string? Description { get; set; }
}

public class SystemDto
{
	public string Key { get; set; } = default!;
	public string Name { get; set; } = default!;
}

public class ParticipantDto
{
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
}

public class LocationSnapshotDto
{
	public string Name { get; set; } = default!;
	public string City { get; set; } = default!;
}