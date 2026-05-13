import type { LocationRole, UserSearchResponse } from "../types/game";

type Props = {
  availableUsers: UserSearchResponse[];
  selectedUserId: string;
  role: LocationRole;
  assignableRoles: LocationRole[];
  busyKey: string | null;
  onSelectedUserIdChange: (userId: string) => void;
  onRoleChange: (role: LocationRole) => void;
  onAddMember: () => void;
};

function roleLabel(role: LocationRole | string) {
  if (role === "Owner") return "Besitzer";
  if (role === "Admin") return "Admin";
  if (role === "Manager") return "Verwalter";
  if (role === "Member") return "Mitglied";
  if (role === "Applicant") return "Bewerber";
  return role;
}

export default function LocationMemberAddForm({
  availableUsers,
  selectedUserId,
  role,
  assignableRoles,
  busyKey,
  onSelectedUserIdChange,
  onRoleChange,
  onAddMember,
}: Props) {
  if (availableUsers.length === 0) {
    return <div className="message message-info">Keine weiteren User verfügbar.</div>;
  }

  return (
    <div className="member-edit-row">
      <select
        value={selectedUserId}
        onChange={(e) => onSelectedUserIdChange(e.target.value)}
      >
        {availableUsers.map((u) => (
          <option key={u.userId} value={u.userId}>
            {u.displayName}
            {u.email ? ` (${u.email})` : ""}
          </option>
        ))}
      </select>

      <select value={role} onChange={(e) => onRoleChange(e.target.value as LocationRole)}>
        {assignableRoles.map((r) => (
          <option key={r} value={r}>
            {roleLabel(r)}
          </option>
        ))}
      </select>

      <button
        type="button"
        onClick={onAddMember}
        disabled={!selectedUserId || busyKey === "add-member"}
      >
        Mitglied hinzufügen
      </button>
    </div>
  );
}