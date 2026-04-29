import { useEffect, useState } from "react";
import type { GameResponse } from "../types/game";
import GameList from "../components/GameList";
import { useJoinGame } from "../api/usejoinGame";

type GameFeedPageProps = {
  title: string;
  loadGamesFn: () => Promise<GameResponse[]>;
  refreshkey?: number;
};
type MessageProps = {
  text: string;
  type: "success" | "error" | "info";
};

function Message({ text, type }: MessageProps) {
  if (!text) return null;
  return <div className={`message message-${type}`}>{text}</div>;
}

export default function GameFeedPage({
  title,
  loadGamesFn,
  refreshkey,
}: GameFeedPageProps) {
  const [games, setGames] = useState<GameResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  async function loadGames() {
    try {
      setLoading(true);
      setError("");
      const data = await loadGamesFn();
      setGames(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unbekannter Fehler");
    } finally {
      setLoading(false);
    }
  }

useEffect(() => {
  loadGames();
}, [refreshkey]);

  const { join, joiningGameId, errorMessage, successMessage } =
    useJoinGame(loadGames);

  function handleJoin(gameId: string) {
    join(gameId);
  }

  return (
    <div className="container">
      <Message text={successMessage} type="success" />
      <Message text={errorMessage} type="error" />
      <Message text={error} type="error" />
      {loading && <Message text="Lade Games..." type="info" />}

      <h1>{title} ({games.length})</h1>

      {!loading && !error && (
        <GameList
          games={games}
          joiningGameId={joiningGameId}
          onJoin={handleJoin}
        />
      )}
    </div>
  );
}