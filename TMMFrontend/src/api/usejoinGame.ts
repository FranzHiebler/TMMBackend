import { useState } from "react";
import { joinGame } from "../api/gamesService";
import { useUser } from "../context/UserContext";

export function useJoinGame(loadGames: () => Promise<void>) {
  const user = useUser();

  const [joiningGameId, setJoiningGameId] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const join = async (gameId: string) => {
    if (!user) return;

    try {
      setErrorMessage("");
      setSuccessMessage("");
      setJoiningGameId(gameId);

      await joinGame(gameId, {
        userId: user.userId,
        displayName: user.displayName,
      });

      setSuccessMessage("Erfolgreich beigetreten");
      await loadGames();
    } catch (err) {
      setErrorMessage(
        err instanceof Error ? err.message : "Fehler beim Join"
      );
    } finally {
      setJoiningGameId(null);
    }
  };

  return {
    join,
    joiningGameId,
    errorMessage,
    successMessage,
  };
}