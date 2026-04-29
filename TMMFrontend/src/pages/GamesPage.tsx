import { useEffect, useState } from "react";
import { createGame, getAllGames } from "../api/gamesService";
import type { CreateGameRequest } from "../types/game";
import GameFeedPage from "./GameFeedPage";

function Message({ text, type }: { text: string; type: "success" | "error" }) {
  if (!text) return null;
  return <div className={`message message-${type}`}>{text}</div>;
}

export default function GamesPage() {
  const [createMessage, setCreateMessage] = useState("");
  const [createError, setCreateError] = useState("");
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    if (!createMessage) return;
    const timer = setTimeout(() => setCreateMessage(""), 3000);
    return () => clearTimeout(timer);
  }, [createMessage]);

  async function handleCreate(request: CreateGameRequest) {
    try {
      setCreateError("");
      setCreateMessage("");

      await createGame(request);

      setCreateMessage("Game erfolgreich erstellt");
      setRefreshKey((prev) => prev + 1);
    } catch (err) {
      setCreateError(
        err instanceof Error ? err.message : "Fehler beim Erstellen"
      );
    }
  }

  return (
    <div className="container">
      <Message text={createMessage} type="success" />
      <Message text={createError} type="error" />
      <GameFeedPage
        title="Alle GameSessions"
        loadGamesFn={getAllGames}
        refreshkey={refreshKey}
      />
    </div>
  );
}