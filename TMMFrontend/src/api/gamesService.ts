import type { JoinGameRequest, GameResponse, CreateGameRequest, LocationOption, SystemOption, SearchNearbyGamesRequest } from "../types/game";

const API = import.meta.env.VITE_API_BASE_URL;

export async function getAllGames(): Promise<GameResponse[]> {
  const res = await fetch(`${API}/Games/search?OnlyOpen=false`);
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

export async function createGame(request: CreateGameRequest): Promise<void> {
  const res = await fetch(`${API}/Games`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  if (!res.ok) {
    throw new Error(`Create fehlgeschlagen: HTTP ${res.status}`);
  }
}

export async function getLocations(): Promise<LocationOption[]> {
  const res = await fetch(`${API}/Locations`);
  if (!res.ok) throw new Error(`Locations fehlgeschlagen: HTTP ${res.status}`);
  return res.json();
}


export async function getSystems(): Promise<SystemOption[]> {
  const res = await fetch(`${API}/Systems`);
  if (!res.ok) throw new Error(`Systems fehlgeschlagen: HTTP ${res.status}`);
  return res.json();
}

export async function searchNearbyGames(
  request: SearchNearbyGamesRequest
): Promise<GameResponse[]> {
  const params = new URLSearchParams({
    latitude: request.latitude.toString(),
    longitude: request.longitude.toString(),
    radiusKm: request.radiusKm.toString(),
  });

  if (request.systemKey) {
    params.append("systemKey", request.systemKey);
  }

  const res = await fetch(`${API}/Games/nearby?${params.toString()}`);
  if (!res.ok) throw new Error(`Nearby fehlgeschlagen: HTTP ${res.status}`);
  return res.json();
}


export async function joinGame(gameId: string, request: JoinGameRequest) {
  const res = await fetch(`/api/Games/${gameId}/join`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || "Join fehlgeschlagen");
  }
}