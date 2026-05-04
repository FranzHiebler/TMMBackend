import { useState } from "react";
import { searchNearbyGames } from "../api/gamesService";
import type { GameResponse } from "../types/game";
import GameList from "../components/GameList";
import { useJoinGame } from "../api/useJoinGame";
import { useUser } from "../context/UserContext";

export default function NearbyPage() {
  const [latitude, setLatitude] = useState("50.5558");
  const [longitude, setLongitude] = useState("9.6808");
  const [radiusKm, setRadiusKm] = useState("25");
  const [systemKey, setSystemKey] = useState("");

  const [games, setGames] = useState<GameResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function loadNearbyGames() {
    const data = await searchNearbyGames({
      latitude: Number(latitude),
      longitude: Number(longitude),
      radiusKm: Number(radiusKm),
      systemKey: systemKey || undefined,
    });

    setGames(data);
  }
  const user = useUser();
  const { join, joiningKey, errorMessage, successMessage, messageByKey  } =
    useJoinGame();

  async function handleSearch(e: React.FormEvent) {
    e.preventDefault();

    try {
      setLoading(true);
      setError("");
      await loadNearbyGames();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Fehler bei Nearby Search");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="container">
      <h1>Nearby Games</h1>

      {successMessage && <div className="message message-success">{successMessage}</div>}
      {errorMessage && <div className="message message-error">{errorMessage}</div>}

      <form onSubmit={handleSearch} className="form">
        <input value={latitude} onChange={(e) => setLatitude(e.target.value)} placeholder="Latitude" />
        <input value={longitude} onChange={(e) => setLongitude(e.target.value)} placeholder="Longitude" />
        <input value={radiusKm} onChange={(e) => setRadiusKm(e.target.value)} placeholder="Radius km" />
        <input value={systemKey} onChange={(e) => setSystemKey(e.target.value)} placeholder="System Key optional" />

        <button type="submit" disabled={loading}>
          {loading ? "Suche..." : "Suchen"}
        </button>
      </form>

      {error && <div className="message message-error">{error}</div>}

      {!loading && !error && (
        <GameList
          games={games}
          joiningKey={joiningKey}
          messageByKey={messageByKey}
          currentUserId={user.userId}
          onJoin={join} />
      )}
    </div>
  );
}