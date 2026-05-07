import { useEffect, useState } from "react";
import { applyToGame, joinTable } from "./gamesService";
import { GameJoinMode } from "../types/game";
import { useUser } from "../context/UserContext";

type UseJoinGameOptions = {
  onJoined?: (gameId: string, tableId: string, systemKey?: string) => void;
  onApplied?: (gameId: string, tableId: string, systemKey?: string) => void;
};

export function useJoinGame(options: UseJoinGameOptions = {}) {
  const [joiningKey, setJoiningKey] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [messageByKey, setMessageByKey] = useState<Record<string, string>>({});
  const user = useUser();

  useEffect(() => {
    if (!errorMessage && !successMessage) return;

    const timeout = window.setTimeout(() => {
      setErrorMessage("");
      setSuccessMessage("");
    }, 4500);

    return () => window.clearTimeout(timeout);
  }, [errorMessage, successMessage]);

  async function join(
    gameId: string,
    tableId: string,
    joinMode: GameJoinMode,
    systemKey?: string
  ) {
    const key = `${gameId}_${tableId}`;

    try {
      setErrorMessage("");
      setSuccessMessage("");
      setJoiningKey(key);
      setMessageByKey((prev) => ({ ...prev, [key]: "" }));

      if (joinMode === GameJoinMode.FirstComeFirstServe) {
        await joinTable(gameId, tableId, { systemKey: systemKey || null }, user);
        options.onJoined?.(gameId, tableId, systemKey);
        setMessageByKey((prev) => ({
          ...prev,
          [key]: joinMode === GameJoinMode.FirstComeFirstServe
            ? "Erfolgreich beigetreten"
            : "Bewerbung gesendet",
        }));
        setSuccessMessage("Erfolgreich beigetreten");
      } else {
        await applyToGame(gameId, { tableId, systemKey: systemKey || null }, user);
        options.onApplied?.(gameId, tableId, systemKey);

        setMessageByKey((prev) => ({
          ...prev,
          [key]: "Bewerbung gesendet",
        }));

        setSuccessMessage("Bewerbung gesendet");
      }
    } catch (err) {
      setErrorMessage(err instanceof Error ? err.message : "Aktion fehlgeschlagen");
      setMessageByKey((prev) => ({
        ...prev,
        [key]: err instanceof Error ? err.message : "Aktion fehlgeschlagen",
      }));
    } finally {
      setJoiningKey(null);
    }
  }
  return { join, joiningKey, errorMessage, successMessage, messageByKey };
}
