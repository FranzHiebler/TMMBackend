export interface SystemDto {
  key: string;
  name: string;
}

export interface ParticipantDto {
  userId: string;
  displayName: string;
}

export interface LocationSnapshotDto {
  name: string;
  city: string;
}

export interface JoinGameRequest {
  userId: string;
  displayName: string;
}

export interface GameResponse {
  id: string;
  title: string;
  system: SystemDto;
  host: ParticipantDto;
  participants: ParticipantDto[];
  maxPlayers: number;
  openSlots: number;
  status: string;
  locationId: string;
  location: LocationSnapshotDto;
  clubId?: string | null;
  startTimeUtc: string;
  description?: string | null;
}

export interface CreateGameRequest {
  title: string;
  systemKey: string;
  systemName: string;
  hostUserId: string;
  hostDisplayName: string;
  maxPlayers: number;
  locationId: string;
  locationName: string;
  locationCity: string;
  clubId?: string | null;
  startTimeUtc: string;
  description?: string | null;
}

export interface LocationOption { 
  id: string; 
  name: string; 
  city: string; 
}

export interface SystemOption {
  key: string;
  name: string;
}

export interface SearchNearbyGamesRequest {
  latitude: number;
  longitude: number;
  radiusKm: number;
  systemKey?: string;
}

export type LocationResponse = {
  id: string;
  name: string;
  city: string;
};