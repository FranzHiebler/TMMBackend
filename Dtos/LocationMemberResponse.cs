namespace TabletopMatchMaker.Dtos;

public class LocationMemberResponse
{
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
	public string Role { get; set; } = default!;
}

public class LocationJoinRequestResponse
{
	public string Id { get; set; } = default!;
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
	public string? Message { get; set; }
	public string Status { get; set; } = default!;
	public DateTime CreatedAt { get; set; }
}