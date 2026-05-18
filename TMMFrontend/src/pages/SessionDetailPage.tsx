import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { getGameById } from "../api/gamesApi";
import { useJoinGame } from "../api/useJoinGame";
import GameCard from "../components/GameCard";
import Message from "../components/Message";
import { useUser } from "../context/UserContext";
import type { GameResponse } from "../types/game";

export default function SessionDetailPage() {
  const { gameId } = useParams();
  const user = useUser();

  const [game, setGame] = useState<GameResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const { join, joiningKey, errorMessage, successMessage, messageByKey } = useJoinGame({
    onGameUpdated: setGame,
  });

  useEffect(() => {
    let cancelled = false;

    async function loadGame() {
      if (!gameId) {
        setError("Session-ID fehlt.");
        setLoading(false);
        return;
      }

      try {
        setError("");
        setLoading(true);
        const loaded = await getGameById(gameId);

        if (!cancelled) {
          setGame(loaded);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : "Session konnte nicht geladen werden.");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void loadGame();

    return () => {
      cancelled = true;
    };
  }, [gameId]);

  return (
    <main className="container">
      <div className="page-header">
        <div>
          <h1>Session</h1>
          <p className="page-subtitle">Detailansicht für genau eine GameSession.</p>
        </div>

        <Link className="nav-create-button" to="/">
          Zur Karte
        </Link>
      </div>

      <Message text={successMessage} type="success" />
      <Message text={errorMessage} type="error" />
      <Message text={error} type="error" />
      {loading && <Message text="Lade Session..." type="info" />}

      {!loading && !error && game && (
        <GameCard
          game={game}
          joiningKey={joiningKey}
          currentUserId={user.userId}
          messageByKey={messageByKey}
          onJoin={join}
          onGameUpdated={setGame}
        />
      )}
    </main>
  );
}