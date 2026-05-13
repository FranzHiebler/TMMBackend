import { useUser, type User } from "../context/UserContext";

const testUsers: User[] = [
  { userId: "64f1a2b3c4d5e6f7890abc12", displayName: "Franz" },
  { userId: "69f900000000000000000001", displayName: "Max Bauer" },
  { userId: "69f900000000000000000002", displayName: "Anna Keller" },
  { userId: "69f900000000000000000003", displayName: "Sophie Wagner" },
  { userId: "69f900000000000000000004", displayName: "Jonas Becker" },
];

export default function UserSwitcher() {
  const user = useUser();

  const users = testUsers.some((u) => u.userId === user.userId)
    ? testUsers.map((u) => (u.userId === user.userId ? user : u))
    : [user, ...testUsers];

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