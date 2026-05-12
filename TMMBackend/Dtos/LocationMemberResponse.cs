namespace TabletopMatchMaker.Dtos;

public class LocationMemberResponse
{
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
	public string Role { get; set; } = default!;
}