namespace TabletopMatchMaker.Dtos
{
	public class CreateLocationRequest
	{
		public string Name { get; set; } = default!;
		public string City { get; set; } = default!;
		public string? Address { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public List<string> SystemKeys { get; set; } = new();
	}

	public class SearchNearbyLocationsRequest
	{
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public double RadiusInMeters { get; set; } = 20000;
		public string? SystemKey { get; set; }
	}

	public class RequestLocationMembershipRequest
	{
		public string? Message { get; set; }
	}
}
