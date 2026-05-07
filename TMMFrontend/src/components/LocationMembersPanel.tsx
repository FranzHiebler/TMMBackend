import { useEffect, useMemo, useState } from "react";
import {
    getLocationMembers,
    removeLocationMember,
    upsertLocationMember,
} from "../api/gamesService";
import type { LocationMemberResponse, LocationResponse, LocationRole } from "../types/game";

const testUsers = [
    { userId: "64f1a2b3c4d5e6f7890abc12", displayName: "Franz" },
    { userId: "69f900000000000000000001", displayName: "Max Bauer" },
    { userId: "69f900000000000000000002", displayName: "Anna Keller" },
    { userId: "69f900000000000000000003", displayName: "Sophie Wagner" },
    { userId: "69f900000000000000000004", displayName: "Jonas Becker" },
];

type Props = {
    location?: LocationResponse;
};

export default function LocationMembersPanel({ location }: Props) {
    const [members, setMembers] = useState<LocationMemberResponse[]>([]);
    const [selectedUserId, setSelectedUserId] = useState("");
    const [role, setRole] = useState<LocationRole>("Member");
    const [error, setError] = useState("");

    const canEdit = location?.role === "Owner";

    const availableUsers = useMemo(() => {
        return testUsers.filter(
            (u) => !members.some((m) => m.userId === u.userId)
        );
    }, [members]);

    async function load() {
        if (!location) return;

        const data = await getLocationMembers(location.id);
        setMembers(data);
    }

    useEffect(() => {
        if (!location) return;

        load().catch((err) =>
            setError(err instanceof Error ? err.message : "Fehler beim Laden")
        );
    }, [location?.id]);

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

        const user = testUsers.find((x) => x.userId === selectedUserId);
        if (!user) return;

        await upsertLocationMember(location.id, {
            userId: user.userId,
            displayName: user.displayName,
            role,
        });

        setRole("Member");
        await load();
    }

    async function changeRole(member: LocationMemberResponse, newRole: LocationRole) {
        if (!location) return;

        await upsertLocationMember(location.id, {
            userId: member.userId,
            displayName: member.displayName,
            role: newRole,
        });

        await load();
    }

    async function remove(userId: string) {
        if (!location) return;

        await removeLocationMember(location.id, userId);
        await load();
    }

    if (!location) {
        return null;
    }

    return (
        <div className="card">
            <h4>Mitglieder</h4>

            {error && <div className="message message-error">{error}</div>}

            <div className="member-list">
                {members.length === 0 && (
                    <div className="message message-info">
                        Keine Mitglieder hinterlegt.
                    </div>
                )}
                {members.map((m) => (
                    <div key={m.userId} className="member-row">
                        <span>{m.displayName || m.userId}</span>

                        {canEdit && m.role !== "Owner" ? (
                            <>
                                <select
                                    value={m.role}
                                    onChange={(e) => changeRole(m, e.target.value as LocationRole)}
                                >
                                    <option value="Member">Member</option>
                                    <option value="Manager">Manager</option>
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

            {canEdit && availableUsers.length > 0 && (
                <div className="member-edit-row">
                    <select
                        value={selectedUserId}
                        onChange={(e) => setSelectedUserId(e.target.value)}
                    >
                        {availableUsers.map((u) => (
                            <option key={u.userId} value={u.userId}>
                                {u.displayName}
                            </option>
                        ))}
                    </select>

                    <select value={role} onChange={(e) => setRole(e.target.value as LocationRole)}>
                        <option value="Member">Member</option>
                        <option value="Manager">Manager</option>
                    </select>

                    <button type="button" onClick={addMember}>
                        Mitglied hinzufügen
                    </button>
                </div>
            )}
        </div>
    );
}