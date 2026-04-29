namespace TMMBackend.Dtos
{
	public class CreateGameRequest
	{
		public string Title { get; set; } = default!;

		public string SystemKey { get; set; } = default!;
		public string SystemName { get; set; } = default!;

		public string HostUserId { get; set; } = default!;
		public string HostDisplayName { get; set; } = default!;

		public int MaxPlayers { get; set; }

		public string LocationId { get; set; } = default!;
		public string LocationName { get; set; } = default!;
		public string LocationCity { get; set; } = default!;

		public string? ClubId { get; set; }

		public DateTime StartTimeUtc { get; set; }

		public string? Description { get; set; }
	}
}