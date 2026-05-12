using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services.Interfaces;

public interface ILocationService : ILocationLookupService
{
	Task<List<LocationOptionResponse>> GetAllAsync();
	Task<LocationResponse> CreateAsync(CreateLocationRequest request);
	Task<List<LocationResponse>> GetMineAsync();
	Task UpdateAsync(string id, CreateLocationRequest request);
	Task<List<LocationMemberResponse>> GetMembersAsync(string id);
	Task<List<LocationResponse>> SearchNearbyAsync(SearchNearbyLocationsRequest request);
	Task RequestMembershipAsync(string id, RequestLocationMembershipRequest request);
	Task UpsertMemberAsync(string id, UpsertLocationMemberRequest request);
	Task RemoveMemberAsync(string id, string userId);
}