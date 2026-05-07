using TMMBackend.Domain;

namespace TMMBackend.Dtos;

public class UpsertLocationMemberRequest
{
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!; // erstmal für später/UI
	public LocationRole Role { get; set; }
}