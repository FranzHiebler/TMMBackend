namespace TabletopMatchMaker.Dtos;

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
