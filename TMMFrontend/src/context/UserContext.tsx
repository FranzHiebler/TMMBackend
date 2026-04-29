import { createContext, useContext } from "react";

type User = {
  userId: string;
  displayName: string;
};

const fakeUser: User = {
  userId: "64f1a2b3c4d5e6f7890abc12",
  displayName: "Franz",
};

export const UserContext = createContext<User>(fakeUser);

type Props = {
  children: React.ReactNode;
};

export function UserProvider({ children }: Props) {
  return (
    <UserContext.Provider value={fakeUser}>
      {children}
    </UserContext.Provider>
  );
}

export function useUser() {
  return useContext(UserContext);
}