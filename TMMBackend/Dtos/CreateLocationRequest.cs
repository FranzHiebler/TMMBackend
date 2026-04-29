namespace TMMBackend.Dtos
{
	public class CreateLocationRequest
	{
		public string Name { get; set; } = default!;
		public string City { get; set; } = default!;
		public string? Address { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
	}
}
