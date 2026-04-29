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
  locationId: string;
  maxPlayers: number;
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

export interface LocationResponse {
  id: string;
  name: string;
  city: string;
  address?: string | null;
  role?: "Owner" | "Admin" | "Member" | null;
  isOpen?: boolean;
}

export interface CreateLocationRequest {
  name: string;
  city: string;
  address?: string | null;
  latitude?: number | null;
  longitude?: number | null;
}