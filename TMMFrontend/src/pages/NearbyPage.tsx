import { useState } from "react";
import { searchNearbyGames } from "../api/gamesService";
import type { GameResponse } from "../types/game";
import GameList from "../components/GameList";


export default function NearbyPage() {
  const [latitude, setLatitude] = useState("50.5558");
  const [longitude, setLongitude] = useState("9.6808");
  const [radiusKm, setRadiusKm] = useState("25");
  const [systemKey, setSystemKey] = useState("");

  const [games, setGames] = useState<GameResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  
  async function handleSearch(e: React.FormEvent) {
    e.preventDefault();

    try {
      setLoading(true);
      setError("");

      const data = await searchNearbyGames({
        latitude: Number(latitude),
        longitude: Number(longitude),
        radiusKm: Number(radiusKm),
        systemKey: systemKey || undefined,
      });

      setGames(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Fehler bei Nearby Search");
    } finally {
      setLoading(false);
    }
  }
  


  return (
    <div className="container">
      <h1>Nearby Games</h1>

      <form onSubmit={handleSearch} className="form">
        <div className="form-group">
          <input value={latitude} onChange={(e) => setLatitude(e.target.value)} placeholder="Latitude" />
        </div>
        <div className="form-group">
          <input value={longitude} onChange={(e) => setLongitude(e.target.value)} placeholder="Longitude" />
        </div>
        <div className="form-group">
          <input value={radiusKm} onChange={(e) => setRadiusKm(e.target.value)} placeholder="Radius km" />
        </div>
        <div className="form-group">
          <input value={systemKey} onChange={(e) => setSystemKey(e.target.value)} placeholder="System Key optional" />
        </div>
        <button type="submit" disabled={loading}>
          {loading ? "Suche..." : "Suchen"}
        </button>
      </form>

      {error && <div className="message message-error">{error}</div>}

      {!loading && !error && (
        <GameList games={games} joiningKey={null} onJoin={() => {}} />
      )}
    </div>
  );
} 