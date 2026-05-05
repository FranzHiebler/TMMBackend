import type {
  ApplyToGameRequest,
  CreateGameRequest,
  CreateLocationRequest,
  GameResponse,
  JoinTableRequest,
  LocationOption,
  LocationResponse,
  SearchNearbyGamesRequest,
  SystemOption,
} from "../types/game";
import type { User } from "../context/UserContext";

const API = import.meta.env.VITE_API_BASE_URL;

export async function getAllGames(): Promise<GameResponse[]> {
  const res = await fetch(`${API}/Games/search?OnlyOpen=false`);
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

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

export async function createGame(request: CreateGameRequest): Promise<GameResponse> {
  const res = await fetch(`${API}/Games`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });

  if (!res.ok) throw new Error(await res.text() || `HTTP ${res.status}`);
  return res.json();
}

export async function getLocations(): Promise<LocationOption[]> {
  const res = await fetch(`${API}/Locations`);
  if (!res.ok) throw new Error(`Locations fehlgeschlagen: HTTP ${res.status}`);
  return res.json();
}

export async function getMyLocations(): Promise<LocationResponse[]> {
  const res = await fetch(`${API}/Locations/mine`);
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

  if (!res.ok) throw new Error(await res.text() || "Join fehlgeschlagen");
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

  if (!res.ok) throw new Error(await res.text() || "Bewerbung fehlgeschlagen");
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

export async function createLocation(
  request: CreateLocationRequest
): Promise<LocationResponse> {
  const res = await fetch(`${API}/Locations`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });

  if (!res.ok) throw new Error(`Location erstellen fehlgeschlagen: HTTP ${res.status}`);
  return res.json();
}

export async function updateLocation(id: string, request: CreateLocationRequest) {
  const res = await fetch(`${API}/Locations/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });

  if (!res.ok) throw new Error(`Location aktualisieren fehlgeschlagen: HTTP ${res.status}`);
}