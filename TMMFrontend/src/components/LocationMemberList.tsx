import type { LocationMemberResponse, LocationRole } from "../types/game";

type Props = {
  members: LocationMemberResponse[];
  canEdit: boolean;
  assignableRoles: LocationRole[];
  busyKey: string | null;
  canModify: (member: LocationMemberResponse) => boolean;
  onChangeRole: (member: LocationMemberResponse, role: LocationRole) => void;
  onRemove: (userId: string) => void;
};

function roleLabel(role: LocationRole | string) {
  if (role === "Owner") return "Besitzer";
  if (role === "Admin") return "Admin";
  if (role === "Manager") return "Verwalter";
  if (role === "Member") return "Mitglied";
  if (role === "Applicant") return "Bewerber";
  return role;
}

export default function LocationMemberList({
  members,
  canEdit,
  assignableRoles,
  busyKey,
  canModify,
  onChangeRole,
  onRemove,
}: Props) {
  return (
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
                onChange={(e) => onChangeRole(m, e.target.value as LocationRole)}
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
                onClick={() => onRemove(m.userId)}
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
  );
}