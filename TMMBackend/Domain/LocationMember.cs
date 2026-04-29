namespace TMMBackend.Domain
{
	public class LocationMember
	{
		public string? UserId { get; set; } = default!;
		public LocationRole Role { get; set; }
	}
}
