using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Dtos;

public class UpsertLocationMemberRequest
{
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
	public LocationRole Role { get; set; }
}