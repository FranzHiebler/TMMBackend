namespace TabletopMatchMaker.Dtos;

public class CreateSystemRequest
{
	public string Key { get; set; } = default!;
	public string Name { get; set; } = default!;
	public string? ShortCode { get; set; }
	public string? Color { get; set; }
	public string? MarkerColor { get; set; }
}

public class SystemResponse
{
	public string Key { get; set; } = default!;
	public string Name { get; set; } = default!;
	public string? ShortCode { get; set; }
	public string? Color { get; set; }
	public string? MarkerColor { get; set; }
}