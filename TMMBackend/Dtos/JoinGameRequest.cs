namespace TabletopMatchMaker.Dtos;

public class JoinGameRequest
{
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
}