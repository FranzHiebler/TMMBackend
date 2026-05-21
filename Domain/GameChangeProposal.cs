using MongoDB.Bson.Serialization.Attributes;

namespace TabletopMatchMaker.Domain;

[BsonIgnoreExtraElements]
public class GameChangeProposal
{
	[BsonElement("proposalId")]
	public string ProposalId { get; set; } = default!;

	[BsonElement("tableId")]
	public string? TableId { get; set; }

	[BsonElement("proposedBy")]
	public ParticipantInfo ProposedBy { get; set; } = new();

	[BsonElement("proposedStartTimeUtc")]
	public DateTime? ProposedStartTimeUtc { get; set; }

	[BsonElement("proposedSystems")]
	public List<string>? ProposedSystems { get; set; }

	[BsonElement("proposedPoints")]
	public int? ProposedPoints { get; set; }

	[BsonElement("message")]
	public string? Message { get; set; }

	[BsonElement("status")]
	public ChangeProposalStatus Status { get; set; } = ChangeProposalStatus.Pending;

	[BsonElement("createdAt")]
	public DateTime CreatedAt { get; set; }

	[BsonElement("resolvedAt")]
	public DateTime? ResolvedAt { get; set; }
}