import { useEffect, useState } from "react";
import type { GameResponse } from "../types/game";
import GameList from "../components/GameList";
import { useJoinGame } from "../api/useJoinGame";

type Props = {
  title: string;
  loadGamesFn: () => Promise<GameResponse[]>;
  refreshKey?: number;
};

function Message({ text, type }: { text: string; type: "success" | "error" | "info" }) {
  if (!text) return null;
  return <div className={`message message-${type}`}>{text}</div>;
}

export default function GameFeedPage({ title, loadGamesFn, refreshKey }: Props) {
  const [games, setGames] = useState<GameResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  async function loadGames() {
    try {
      setLoading(true);
      setError("");
      setGames(await loadGamesFn());
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unbekannter Fehler");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadGames();
  }, [refreshKey]);

  const { join, joiningKey, errorMessage, successMessage } = useJoinGame(loadGames);

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
          joiningKey={joiningKey}
          onJoin={join}
        />
      )}
    </div>
  );
}