namespace TabletopMatchMaker.Dtos;

public class SearchGamesRequest
{
	public string? SystemKey { get; set; }
	public string? City { get; set; }
	public bool OnlyOpen { get; set; } = true;
	public bool OnlyJoinable { get; set; } = false; // freie Plätze
	public DateTime? FromUtc { get; set; }
}