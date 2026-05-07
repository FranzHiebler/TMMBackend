export type ApplicationStatus = "Pending" | "Accepted" | "Rejected" | "Withdrawn";
export type LocationRole = "Owner" | "Admin" | "Manager" | "Member" | "Applicant";

export interface ParticipantDto {
  userId: string;
  displayName: string;
}

export interface LocationMemberResponse {
  userId: string;
  displayName: string;
  role: LocationRole;
}

export interface UpsertLocationMemberRequest {
  userId: string;
  displayName: string;
  role: LocationRole;
}

export interface LocationSnapshotDto {
  name: string;
  city: string;
}

export interface TableApplicationDto {
  id: string;
  tableId?: string | null;
  player: ParticipantDto;
  systemKey?: string | null;
  message?: string | null;
  status: ApplicationStatus;
  createdAt: string;
}

export interface GameTableDto {
  id: string;
  name: string;
  maxPlayers: number;
  systems: string[];
  scenario?: string | null;
  points?: number | null;
  notes?: string | null;
  assignedPlayers: ParticipantDto[];
  applications: TableApplicationDto[];
  openSlots: number;
}

export interface GameResponse {
  id: string;
  title: string;
  host: ParticipantDto;
  status: string | number;
  joinMode: GameJoinMode;
  locationId: string;
  location: LocationSnapshotDto;
  clubId?: string | null;
  startTimeUtc: string;
  description?: string | null;
  tables: GameTableDto[];
  maxPlayers: number;
  assignedPlayers: number;
  openSlots: number;
}

export interface CreateGameTableRequest {
  name: string;
  maxPlayers: number;
  systems: string[];
  scenario?: string | null;
  points?: number | null;
  notes?: string | null;
}

export interface CreateGameRequest {
  title: string;
  locationId: string;
  clubId?: string | null;
  startTimeUtc: string;
  description?: string | null;
  joinMode: GameJoinMode;
  tables: CreateGameTableRequest[];
}

export interface JoinTableRequest {
  systemKey?: string | null;
}

export interface ApplyToGameRequest {
  tableId?: string | null;
  systemKey?: string | null;
  message?: string | null;
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
  latitude?: number | null;
  longitude?: number | null;
  role?: LocationRole | null;
  isOpen?: boolean;
}

export interface CreateLocationRequest {
  name: string;
  city: string;
  address?: string | null;
  latitude?: number | null;
  longitude?: number | null;
}

export const GameJoinMode = {
  ApprovalRequired: "ApprovalRequired",
  FirstComeFirstServe: "FirstComeFirstServe",
} as const;

export interface UserSearchResponse {
  userId: string;
  displayName: string;
  email?: string | null;
}

export type GameJoinMode =
  typeof GameJoinMode[keyof typeof GameJoinMode];
