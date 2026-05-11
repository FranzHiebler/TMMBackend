import type {
  ApplyToGameRequest,
  CreateChangeProposalRequest,
  CreateGameRequest,
  CreateLocationRequest,
  GameResponse,
  JoinTableRequest,
  LocationOption,
  LocationResponse,
  SearchNearbyGamesRequest,
  SearchNearbyLocationsRequest,
  LocationMemberResponse,
  UpsertLocationMemberRequest,
  SystemOption,
  UserSearchResponse,
} from "../types/game";
import type { User } from "../context/UserContext";

const API = import.meta.env.VITE_API_BASE_URL;

function authHeaders(user?: User): HeadersInit {
  return {
    "Content-Type": "application/json",
    ...(user
      ? {
          "x-user-id": user.userId,
          "x-display-name": user.displayName,
        }
      : {}),
  };
}

export async function getAllGames(): Promise<GameResponse[]> {
  const res = await fetch(`${API}/Games/search?OnlyOpen=false`);
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

export async function createGame(
  request: CreateGameRequest,
  user: User
): Promise<GameResponse> {
  const res = await fetch(`${API}/Games`, {
    method: "POST",
    headers: authHeaders(user),
    body: JSON.stringify(request),
  });

  if (!res.ok) throw new Error((await res.text()) || `HTTP ${res.status}`);
  return res.json();
}

export async function getGameById(gameId: string): Promise<GameResponse> {
  const res = await fetch(`${API}/Games/${gameId}`);

  if (!res.ok) {
    throw new Error((await res.text()) || `Game laden fehlgeschlagen: HTTP ${res.status}`);
  }

  return res.json();
}

export async function getLocations(): Promise<LocationOption[]> {
  const res = await fetch(`${API}/Locations`);
  if (!res.ok) throw new Error(`Locations fehlgeschlagen: HTTP ${res.status}`);
  return res.json();
}

export async function getMyLocations(user: User): Promise<LocationResponse[]> {
  const res = await fetch(`${API}/Locations/mine`, {
    headers: authHeaders(user),
  });

  if (!res.ok) throw new Error(`Locations fehlgeschlagen: HTTP ${res.status}`);
  return res.json();
}

export async function getSystems(): Promise<SystemOption[]> {
  const res = await fetch(`${API}/Systems`);
  if (!res.ok) throw new Error(`Systems fehlgeschlagen: HTTP ${res.status}`);
  return res.json();
}

export async function joinTable(
  gameId: string,
  tableId: string,
  request: JoinTableRequest,
  user: User
): Promise<void> {
  const res = await fetch(`${API}/Games/${gameId}/tables/${tableId}/join`, {
    method: "POST",
    headers: authHeaders(user),
    body: JSON.stringify(request),
  });

  if (!res.ok) throw new Error((await res.text()) || "Join fehlgeschlagen");
}

export async function applyToGame(
  gameId: string,
  request: ApplyToGameRequest,
  user: User
): Promise<void> {
  const res = await fetch(`${API}/Games/${gameId}/apply`, {
    method: "POST",
    headers: authHeaders(user),
    body: JSON.stringify(request),
  });

  if (!res.ok) throw new Error((await res.text()) || "Bewerbung fehlgeschlagen");
}

export async function createChangeProposal(
  gameId: string,
  request: CreateChangeProposalRequest,
  user: User
): Promise<GameResponse> {
  const res = await fetch(`${API}/Games/${gameId}/change-proposals`, {
    method: "POST",
    headers: authHeaders(user),
    body: JSON.stringify(request),
  });

  if (!res.ok) throw new Error((await res.text()) || "Vorschlag konnte nicht gesendet werden");
  return res.json();
}

export async function createSystem(
  request: SystemOption,
  user: User
): Promise<SystemOption> {
  const res = await fetch(`${API}/Systems`, {
    method: "POST",
    headers: authHeaders(user),
    body: JSON.stringify(request),
  });

  if (!res.ok) throw new Error((await res.text()) || "System konnte nicht angelegt werden");
  return res.json();
}

export async function acceptChangeProposal(
  gameId: string,
  proposalId: string,
  user: User
): Promise<GameResponse> {
  const res = await fetch(`${API}/Games/${gameId}/change-proposals/${proposalId}/accept`, {
    method: "POST",
    headers: authHeaders(user),
  });

  if (!res.ok) throw new Error((await res.text()) || "Vorschlag konnte nicht angenommen werden");
  return res.json();
}

export async function rejectChangeProposal(
  gameId: string,
  proposalId: string,
  user: User
): Promise<GameResponse> {
  const res = await fetch(`${API}/Games/${gameId}/change-proposals/${proposalId}/reject`, {
    method: "POST",
    headers: authHeaders(user),
  });

  if (!res.ok) throw new Error((await res.text()) || "Vorschlag konnte nicht abgelehnt werden");
  return res.json();
}

export async function searchNearbyGames(
  request: SearchNearbyGamesRequest
): Promise<GameResponse[]> {
  const params = new URLSearchParams({
    latitude: request.latitude.toString(),
    longitude: request.longitude.toString(),
    radiusInMeters: (request.radiusKm * 1000).toString(),
  });

  if (request.systemKey) params.append("systemKey", request.systemKey);

  const res = await fetch(`${API}/Games/nearby?${params.toString()}`);
  if (!res.ok) throw new Error(`Nearby fehlgeschlagen: HTTP ${res.status}`);
  return res.json();
}

export async function searchNearbyLocations(
  request: SearchNearbyLocationsRequest
): Promise<LocationResponse[]> {
  const params = new URLSearchParams({
    latitude: request.latitude.toString(),
    longitude: request.longitude.toString(),
    radiusInMeters: (request.radiusKm * 1000).toString(),
  });

  if (request.systemKey) params.append("systemKey", request.systemKey);

  const res = await fetch(`${API}/Locations/nearby?${params.toString()}`);
  if (!res.ok) throw new Error(`Nearby Locations fehlgeschlagen: HTTP ${res.status}`);
  return res.json();
}

export async function requestLocationMembership(
  locationId: string,
  message: string | null,
  user: User
): Promise<void> {
  const res = await fetch(`${API}/Locations/${locationId}/join-requests`, {
    method: "POST",
    headers: authHeaders(user),
    body: JSON.stringify({ message }),
  });

  if (!res.ok) throw new Error((await res.text()) || "Anfrage konnte nicht gesendet werden");
}

export async function createLocation(
  request: CreateLocationRequest,
  user: User
): Promise<LocationResponse> {
  const res = await fetch(`${API}/Locations`, {
    method: "POST",
    headers: authHeaders(user),
    body: JSON.stringify(request),
  });

  if (!res.ok) throw new Error(`Location erstellen fehlgeschlagen: HTTP ${res.status}`);
  return res.json();
}

export async function updateLocation(
  id: string,
  request: CreateLocationRequest,
  user: User
): Promise<void> {
  const res = await fetch(`${API}/Locations/${id}`, {
    method: "PUT",
    headers: authHeaders(user),
    body: JSON.stringify(request),
  });

  if (!res.ok) throw new Error(`Location aktualisieren fehlgeschlagen: HTTP ${res.status}`);
}

export async function getLocationMembers(
  locationId: string,
  user: User
): Promise<LocationMemberResponse[]> {
  const res = await fetch(`${API}/Locations/${locationId}/members`, {
    headers: authHeaders(user),
  });

  if (!res.ok) throw new Error(`Mitglieder laden fehlgeschlagen: HTTP ${res.status}`);
  return res.json();
}

export async function upsertLocationMember(
  locationId: string,
  request: UpsertLocationMemberRequest,
  user: User
): Promise<void> {
  const res = await fetch(`${API}/Locations/${locationId}/members`, {
    method: "POST",
    headers: authHeaders(user),
    body: JSON.stringify(request),
  });

  if (!res.ok) throw new Error((await res.text()) || "Mitglied speichern fehlgeschlagen");
}

export async function removeLocationMember(
  locationId: string,
  userId: string,
  user: User
): Promise<void> {
  const res = await fetch(`${API}/Locations/${locationId}/members/${userId}`, {
    method: "DELETE",
    headers: authHeaders(user),
  });

  if (!res.ok) throw new Error((await res.text()) || "Mitglied entfernen fehlgeschlagen");
}

export async function searchUsers(query: string): Promise<UserSearchResponse[]> {
  const params = new URLSearchParams();

  if (query.trim()) {
    params.append("query", query.trim());
  }

  const res = await fetch(`${API}/Users/search?${params.toString()}`);

  if (!res.ok) {
    throw new Error(`User-Suche fehlgeschlagen: HTTP ${res.status}`);
  }

  return res.json();
}

export async function assignApplicationToTable(
  gameId: string,
  tableId: string,
  applicationId: string,
  user: User
): Promise<void> {
  const res = await fetch(`${API}/Games/${gameId}/tables/${tableId}/assign`, {
    method: "POST",
    headers: authHeaders(user),
    body: JSON.stringify({ applicationId }),
  });

  if (!res.ok) throw new Error((await res.text()) || "Zuweisung fehlgeschlagen");
}

export async function rejectApplication(
  gameId: string,
  applicationId: string,
  user: User
): Promise<void> {
  const res = await fetch(`${API}/Games/${gameId}/applications/${applicationId}/reject`, {
    method: "POST",
    headers: authHeaders(user),
  });

  if (!res.ok) throw new Error((await res.text()) || "Ablehnen fehlgeschlagen");
}

export async function removePlayerFromTable(
  gameId: string,
  tableId: string,
  userId: string,
  user: User
): Promise<void> {
  const res = await fetch(`${API}/Games/${gameId}/tables/${tableId}/players/${userId}/remove`, {
    method: "POST",
    headers: authHeaders(user),
  });

  if (!res.ok) throw new Error((await res.text()) || "Spieler entfernen fehlgeschlagen");
}

export async function movePlayerToTable(
  gameId: string,
  userId: string,
  targetTableId: string,
  user: User
): Promise<void> {
  const res = await fetch(`${API}/Games/${gameId}/players/${userId}/move`, {
    method: "POST",
    headers: authHeaders(user),
    body: JSON.stringify({ targetTableId }),
  });

  if (!res.ok) throw new Error((await res.text()) || "Spieler verschieben fehlgeschlagen");
}