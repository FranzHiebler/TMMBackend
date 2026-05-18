import { useEffect, useState } from "react";
import { searchUsers } from "../api/usersApi";
import { useUser, type User } from "../context/UserContext";

export default function UserSwitcher() {
  const user = useUser();
  const [users, setUsers] = useState<User[]>([user]);

  useEffect(() => {
    let cancelled = false;

    async function loadUsers() {
      try {
        const result = await searchUsers("");

        if (cancelled) return;

        const loadedUsers = result.map((u) => ({
          userId: u.userId,
          displayName: u.displayName,
        }));

        const containsCurrentUser = loadedUsers.some((u) => u.userId === user.userId);

        setUsers(containsCurrentUser ? loadedUsers : [user, ...loadedUsers]);
      } catch {
        if (!cancelled) {
          setUsers([user]);
        }
      }
    }

    void loadUsers();

    return () => {
      cancelled = true;
    };
  }, [user]);

  return (
    <select
      className="user-switcher"
      value={user.userId}
      onChange={(e) => {
        const selected = users.find((u) => u.userId === e.target.value);
        if (selected) user.setUser(selected);
      }}
    >
      {users.map((u) => (
        <option key={u.userId} value={u.userId}>
          {u.displayName}
        </option>
      ))}
    </select>
  );
}