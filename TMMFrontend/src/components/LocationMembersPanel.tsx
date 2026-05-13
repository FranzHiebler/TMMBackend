import { useCallback, useEffect, useMemo, useState } from "react";
import {
  acceptLocationJoinRequest,
  getLocationJoinRequests,
  getLocationMembers,
  rejectLocationJoinRequest,
  removeLocationMember,
  searchUsers,
  upsertLocationMember,
} from "../api/gamesService";
import type {
  LocationJoinRequestResponse,
  LocationMemberResponse,
  LocationResponse,
  LocationRole,
  UserSearchResponse,
} from "../types/game";
import { useUser } from "../context/UserContext";

type Props = {
  location?: LocationResponse;
};

const ownerAssignableRoles: LocationRole[] = ["Admin", "Manager", "Member", "Applicant"];
const adminAssignableRoles: LocationRole[] = ["Manager", "Member", "Applicant"];

function roleLabel(role: LocationRole | string) {
  if (role === "Owner") return "Besitzer";
  if (role === "Admin") return "Admin";
  if (role === "Manager") return "Verwalter";
  if (role === "Member") return "Mitglied";
  if (role === "Applicant") return "Bewerber";
  return role;
}

export default function LocationMembersPanel({ location }: Props) {
  const user = useUser();

  const [members, setMembers] = useState<LocationMemberResponse[]>([]);
  const [joinRequests, setJoinRequests] = useState<LocationJoinRequestResponse[]>([]);
  const [users, setUsers] = useState<UserSearchResponse[]>([]);
  const [selectedUserId, setSelectedUserId] = useState("");
  const [role, setRole] = useState<LocationRole>("Member");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [busyKey, setBusyKey] = useState<string | null>(null);

  const canEdit = location?.role === "Owner" || location?.role === "Admin";

  const assignableRoles = useMemo<LocationRole[]>(() => {
    if (location?.role === "Owner") return ownerAssignableRoles;
    if (location?.role === "Admin") return adminAssignableRoles;
    return [];
  }, [location?.role]);

  const availableUsers = useMemo(() => {
    return users.filter((u) => !members.some((m) => m.userId === u.userId));
  }, [users, members]);

  const effectiveSelectedUserId =
    selectedUserId && availableUsers.some((u) => u.userId === selectedUserId)
      ? selectedUserId
      : availableUsers[0]?.userId ?? "";

  const loadMembers = useCallback(async () => {
    if (!location) return;
    setMembers(await getLocationMembers(location.id, user));
  }, [location, user]);

  const loadJoinRequests = useCallback(async () => {
    if (!location || !canEdit) return;
    setJoinRequests(await getLocationJoinRequests(location.id, user));
  }, [location, user, canEdit]);

  const loadUsers = useCallback(async () => {
    setUsers(await searchUsers(""));
  }, []);

  useEffect(() => {
    if (!success) return;

    const timeout = window.setTimeout(() => setSuccess(""), 3500);
    return () => window.clearTimeout(timeout);
  }, [success]);

  useEffect(() => {
    if (!location) return;

    async function init() {
      setError("");
      setSuccess("");
      await loadMembers();

      if (canEdit) {
        await Promise.all([loadUsers(), loadJoinRequests()]);
      }
    }

    init().catch((err) =>
      setError(err instanceof Error ? err.message : "Daten konnten nicht geladen werden")
    );
  }, [canEdit, loadMembers, loadUsers, loadJoinRequests, location]);

  async function acceptRequest(requestId: string) {
    if (!location) return;

    try {
      setBusyKey(`accept-${requestId}`);
      setError("");
      await acceptLocationJoinRequest(location.id, requestId, user);
      await Promise.all([loadMembers(), loadJoinRequests(), loadUsers()]);
      setSuccess("Beitrittsanfrage angenommen.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Beitrittsanfrage konnte nicht angenommen werden");
    } finally {
      setBusyKey(null);
    }
  }

  async function rejectRequest(requestId: string) {
    if (!location) return;

    try {
      setBusyKey(`reject-${requestId}`);
      setError("");
      await rejectLocationJoinRequest(location.id, requestId, user);
      await loadJoinRequests();
      setSuccess("Beitrittsanfrage abgelehnt.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Beitrittsanfrage konnte nicht abgelehnt werden");
    } finally {
      setBusyKey(null);
    }
  }

  async function addMember() {
    if (!location) return;

    const selected = users.find((x) => x.userId === effectiveSelectedUserId);
    if (!selected) return;

    try {
      setBusyKey("add-member");
      setError("");

      await upsertLocationMember(
        location.id,
        { userId: selected.userId, displayName: selected.displayName, role },
        user
      );

      setRole("Member");
      await Promise.all([loadMembers(), loadUsers()]);
      setSuccess("Mitglied hinzugefügt.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Mitglied konnte nicht hinzugefügt werden");
    } finally {
      setBusyKey(null);
    }
  }

  async function changeRole(member: LocationMemberResponse, newRole: LocationRole) {
    if (!location) return;

    try {
      setBusyKey(`role-${member.userId}`);
      setError("");

      await upsertLocationMember(
        location.id,
        {
          userId: member.userId,
          displayName: member.displayName || member.userId,
          role: newRole,
        },
        user
      );

      await loadMembers();
      setSuccess("Rolle geändert.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Rolle konnte nicht geändert werden");
    } finally {
      setBusyKey(null);
    }
  }

  async function remove(userId: string) {
    if (!location) return;

    try {
      setBusyKey(`remove-${userId}`);
      setError("");

      await removeLocationMember(location.id, userId, user);
      await Promise.all([loadMembers(), loadUsers()]);
      setSuccess("Mitglied entfernt.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Mitglied konnte nicht entfernt werden");
    } finally {
      setBusyKey(null);
    }
  }

  function canModify(member: LocationMemberResponse) {
    if (member.role === "Owner") return false;
    if (location?.role === "Owner") return true;
    if (location?.role === "Admin") return member.role !== "Admin";
    return false;
  }

  if (!location) return null;

  return (
    <div className="card">
      <h4>Mitglieder</h4>

      {error && <div className="message message-error">{error}</div>}
      {success && <div className="message message-success">{success}</div>}

      {canEdit && joinRequests.length > 0 && (
        <div className="proposal-list">
          <h4>Beitrittsanfragen</h4>

          {joinRequests.map((request) => (
            <div key={request.id} className="proposal-row">
              <div>
                <b>{request.displayName}</b>
                {request.message && <p>{request.message}</p>}
              </div>

              <div className="proposal-actions">
                <button
                  type="button"
                  disabled={busyKey === `accept-${request.id}`}
                  onClick={() => acceptRequest(request.id)}
                >
                  Annehmen
                </button>

                <button
                  type="button"
                  disabled={busyKey === `reject-${request.id}`}
                  onClick={() => rejectRequest(request.id)}
                >
                  Ablehnen
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      <div className="member-list">
        {members.length === 0 && (
          <div className="message message-info">Keine Mitglieder hinterlegt.</div>
        )}

        {members.map((m) => (
          <div key={m.userId} className="member-row">
            <span>{m.displayName || m.userId}</span>

            {canEdit && canModify(m) ? (
              <>
                <select
                  value={m.role}
                  disabled={busyKey === `role-${m.userId}`}
                  onChange={(e) => changeRole(m, e.target.value as LocationRole)}
                >
                  {assignableRoles.map((r) => (
                    <option key={r} value={r}>
                      {roleLabel(r)}
                    </option>
                  ))}
                </select>

                <button
                  type="button"
                  disabled={busyKey === `remove-${m.userId}`}
                  onClick={() => remove(m.userId)}
                >
                  Entfernen
                </button>
              </>
            ) : (
              <strong>{roleLabel(m.role)}</strong>
            )}
          </div>
        ))}
      </div>

      {canEdit && (
        <div className="member-edit-block">
          {availableUsers.length > 0 ? (
            <div className="member-edit-row">
              <select
                value={effectiveSelectedUserId}
                onChange={(e) => setSelectedUserId(e.target.value)}
              >
                {availableUsers.map((u) => (
                  <option key={u.userId} value={u.userId}>
                    {u.displayName}
                    {u.email ? ` (${u.email})` : ""}
                  </option>
                ))}
              </select>

              <select value={role} onChange={(e) => setRole(e.target.value as LocationRole)}>
                {assignableRoles.map((r) => (
                  <option key={r} value={r}>
                    {roleLabel(r)}
                  </option>
                ))}
              </select>

              <button
                type="button"
                onClick={addMember}
                disabled={!effectiveSelectedUserId || busyKey === "add-member"}
              >
                Mitglied hinzufügen
              </button>
            </div>
          ) : (
            <div className="message message-info">Keine weiteren User verfügbar.</div>
          )}
        </div>
      )}
    </div>
  );
}