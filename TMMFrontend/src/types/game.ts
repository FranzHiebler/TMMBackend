export type GameSessionState = "Open" | "Full" | "Closed" | "Cancelled";
export type ApplicationStatus = "Pending" | "Accepted" | "Rejected" | "Withdrawn";
export type ChangeProposalStatus = "Pending" | "Accepted" | "Rejected";
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
  startTimeUtc?: string | null;
  notes?: string | null;
  assignedPlayers: ParticipantDto[];
  applications: TableApplicationDto[];
  openSlots: number;
}

export interface GameChangeProposalDto {
  id: string;
  tableId?: string | null;
  proposedBy: ParticipantDto;
  proposedStartTimeUtc?: string | null;
  proposedSystems?: string[] | null;
  proposedPoints?: number | null;
  message?: string | null;
  status: ChangeProposalStatus;
  createdAt: string;
  resolvedAt?: string | null;
}

export interface GameResponse {
  id: string;
  title: string;
  host: ParticipantDto;
  status: GameSessionState;
  joinMode: GameJoinMode;
  locationId: string;
  location: LocationSnapshotDto;
  clubId?: string | null;
  startTimeUtc: string;
  description?: string | null;
  tables: GameTableDto[];
  changeProposals: GameChangeProposalDto[];
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
  startTimeUtc?: string | null;
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

export interface CreateChangeProposalRequest {
  tableId?: string | null;
  proposedStartTimeUtc?: string | null;
  proposedSystems?: string[] | null;
  proposedPoints?: number | null;
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

export interface SearchNearbyLocationsRequest {
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
  systemKeys: string[];
  hasPendingJoinRequest?: boolean;
}

export interface CreateLocationRequest {
  name: string;
  city: string;
  address?: string | null;
  latitude: number;
  longitude: number;
  systemKeys: string[];
}

export const GameJoinMode = {
  ApprovalRequired: "ApprovalRequired",
  FirstComeFirstServe: "FirstComeFirstServe",
} as const;

export type GameJoinMode = typeof GameJoinMode[keyof typeof GameJoinMode];

export interface UserSearchResponse {
  userId: string;
  displayName: string;
  email?: string | null;
}

export interface LocationJoinRequestResponse {
  id: string;
  userId: string;
  displayName: string;
  message?: string | null;
  status: string;
  createdAt: string;
}

export interface UserProfileResponse {
  userId: string;
  displayName: string;
  email?: string | null;
  defaultLocationId?: string | null;
}

export interface UpdateUserProfileRequest {
  displayName: string;
  defaultLocationId?: string | null;
}