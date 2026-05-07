import { useEffect, useMemo, useState } from "react";
import {
  getLocationMembers,
  removeLocationMember,
  searchUsers,
  upsertLocationMember,
} from "../api/gamesService";
import type {
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

export default function LocationMembersPanel({ location }: Props) {
  const user = useUser();

  const [members, setMembers] = useState<LocationMemberResponse[]>([]);
  const [users, setUsers] = useState<UserSearchResponse[]>([]);
  const [selectedUserId, setSelectedUserId] = useState("");
  const [role, setRole] = useState<LocationRole>("Member");
  const [error, setError] = useState("");

  const canEdit = location?.role === "Owner" || location?.role === "Admin";

  const assignableRoles = useMemo<LocationRole[]>(() => {
    if (location?.role === "Owner") return ownerAssignableRoles;
    if (location?.role === "Admin") return adminAssignableRoles;
    return [];
  }, [location?.role]);

  const availableUsers = useMemo(() => {
    return users.filter((u) => !members.some((m) => m.userId === u.userId));
  }, [users, members]);

  async function loadMembers() {
    if (!location) return;
    setMembers(await getLocationMembers(location.id, user));
  }

  async function loadUsers() {
    setUsers(await searchUsers(""));
  }

  useEffect(() => {
    if (!location) return;

    async function init() {
      setError("");
      await loadMembers();

      if (canEdit) {
        await loadUsers();
      }
    }

    init().catch((err) =>
      setError(err instanceof Error ? err.message : "Daten konnten nicht geladen werden")
    );
  }, [location?.id, location?.role, user.userId]);

  useEffect(() => {
    if (!selectedUserId && availableUsers.length > 0) {
      setSelectedUserId(availableUsers[0].userId);
      return;
    }

    if (selectedUserId && !availableUsers.some((u) => u.userId === selectedUserId)) {
      setSelectedUserId(availableUsers[0]?.userId ?? "");
    }
  }, [availableUsers, selectedUserId]);

  async function addMember() {
    if (!location) return;

    const selected = users.find((x) => x.userId === selectedUserId);
    if (!selected) return;

    await upsertLocationMember(
      location.id,
      {
        userId: selected.userId,
        displayName: selected.displayName,
        role,
      },
      user
    );

    setRole("Member");
    await loadMembers();
    await loadUsers();
  }

  async function changeRole(member: LocationMemberResponse, newRole: LocationRole) {
    if (!location) return;

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
  }

  async function remove(userId: string) {
    if (!location) return;

    await removeLocationMember(location.id, userId, user);
    await loadMembers();
    await loadUsers();
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
                  onChange={(e) => changeRole(m, e.target.value as LocationRole)}
                >
                  {assignableRoles.map((r) => (
                    <option key={r} value={r}>
                      {r}
                    </option>
                  ))}
                </select>

                <button type="button" onClick={() => remove(m.userId)}>
                  Entfernen
                </button>
              </>
            ) : (
              <strong>{m.role}</strong>
            )}
          </div>
        ))}
      </div>

      {canEdit && (
        <div className="member-edit-block">
          {availableUsers.length > 0 ? (
            <div className="member-edit-row">
              <select
                value={selectedUserId}
                onChange={(e) => setSelectedUserId(e.target.value)}
              >
                {availableUsers.map((u) => (
                  <option key={u.userId} value={u.userId}>
                    {u.displayName}
                    {u.email ? ` (${u.email})` : ""}
                  </option>
                ))}
              </select>

              <select
                value={role}
                onChange={(e) => setRole(e.target.value as LocationRole)}
              >
                {assignableRoles.map((r) => (
                  <option key={r} value={r}>
                    {r}
                  </option>
                ))}
              </select>

              <button type="button" onClick={addMember} disabled={!selectedUserId}>
                Mitglied hinzufügen
              </button>
            </div>
          ) : (
            <div className="message message-info">
              Keine weiteren User verfügbar.
            </div>
          )}
        </div>
      )}
    </div>
  );
}