import { useState } from "react";
import { applyToGame, joinTable } from "./gamesService";
import type { GameJoinMode } from "../types/game";

export function useJoinGame(loadGames: () => Promise<void>) {
  const [joiningKey, setJoiningKey] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  async function join(gameId: string, tableId: string, joinMode: GameJoinMode, systemKey?: string) {
    const key = `${gameId}_${tableId}`;

    try {
      setErrorMessage("");
      setSuccessMessage("");
      setJoiningKey(key);

      if (joinMode === 1) {
        await joinTable(gameId, tableId, { systemKey: systemKey || null });
        setSuccessMessage("Erfolgreich beigetreten");
      } else {
        await applyToGame(gameId, { tableId, systemKey: systemKey || null });
        setSuccessMessage("Bewerbung gesendet");
      }

      await loadGames();
    } catch (err) {
      setErrorMessage(err instanceof Error ? err.message : "Aktion fehlgeschlagen");
    } finally {
      setJoiningKey(null);
    }
  }

  return { join, joiningKey, errorMessage, successMessage };
}