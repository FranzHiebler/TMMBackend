import type { UserSearchResponse } from "../types/game";
import { API, handleResponse } from "./apiClient";

export async function searchUsers(query: string): Promise<UserSearchResponse[]> {
  const params = new URLSearchParams();

  if (query.trim()) {
    params.append("query", query.trim());
  }

  const res = await fetch(`${API}/Users/search?${params.toString()}`);
  return handleResponse<UserSearchResponse[]>(res, "User-Suche fehlgeschlagen");
}