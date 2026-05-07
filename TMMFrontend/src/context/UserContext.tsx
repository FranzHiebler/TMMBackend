/* eslint-disable react-refresh/only-export-components */
import { createContext, useContext, useState } from "react";

export type User = {
  userId: string;
  displayName: string;
};

type UserContextValue = User & {
  setUser: (user: User) => void;
};

const defaultUser: User = {
  userId: "64f1a2b3c4d5e6f7890abc12",
  displayName: "Franz",
};

const UserContext = createContext<UserContextValue>({
  ...defaultUser,
  setUser: () => {},
});

type Props = {
  children: React.ReactNode;
};

export function UserProvider({ children }: Props) {
  const [user, setUser] = useState<User>(defaultUser);

  return (
    <UserContext.Provider value={{ ...user, setUser }}>
      {children}
    </UserContext.Provider>
  );
}

export function useUser() {
  return useContext(UserContext);
}
