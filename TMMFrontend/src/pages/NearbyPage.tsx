import { useState } from "react";
import { searchNearbyGames } from "../api/gamesService";
import type { GameResponse } from "../types/game";
import GameList from "../components/GameList";
import { useJoinGame } from "../api/useJoinGame";
import { useUser } from "../context/UserContext";
import LocationPicker from "../components/LocationPicker";

export default function NearbyPage() {
  const [address, setAddress] = useState("");
  const [city, setCity] = useState("Bad Orb");
  const [latitude, setLatitude] = useState<number | null>(50.5558);
  const [longitude, setLongitude] = useState<number | null>(9.6808);
  const [radiusKm, setRadiusKm] = useState("25");
  const [systemKey, setSystemKey] = useState("");

  const [games, setGames] = useState<GameResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const user = useUser();
  const { join, joiningKey, errorMessage, successMessage, messageByKey } = useJoinGame();

  async function searchAddress() {
    const query = `${address}, ${city}, Deutschland`.trim();
    if (query.length < 5) return;

    const res = await fetch(
      `https://nominatim.openstreetmap.org/search?format=json&countrycodes=de&q=${encodeURIComponent(query)}&limit=1`
    );

    const data = await res.json();

    if (!data.length) {
      setError("Adresse nicht gefunden. Bitte Marker manuell setzen.");
      return;
    }

    setError("");
    setLatitude(Number(data[0].lat));
    setLongitude(Number(data[0].lon));
  }

  async function handleSearch(e: React.FormEvent) {
    e.preventDefault();

    if (latitude == null || longitude == null) {
      setError("Bitte Standort auswählen.");
      return;
    }

    try {
      setLoading(true);
      setError("");

      const data = await searchNearbyGames({
        latitude,
        longitude,
        radiusKm: Number(radiusKm), // wird in gamesService.ts bereits * 1000 genommen
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

      {successMessage && <div className="message message-success">{successMessage}</div>}
      {errorMessage && <div className="message message-error">{errorMessage}</div>}
      {error && <div className="message message-error">{error}</div>}

      <form onSubmit={handleSearch} className="form">
        <input value={city} onChange={(e) => setCity(e.target.value)} placeholder="Stadt" />
        <input value={address} onChange={(e) => setAddress(e.target.value)} placeholder="Adresse optional" />

        <button type="button" onClick={searchAddress}>
          Adresse suchen
        </button>

        <LocationPicker
          latitude={latitude}
          longitude={longitude}
          onChange={(lat, lng) => {
            setLatitude(lat);
            setLongitude(lng);
          }}
        />

        <div className="input-with-unit">
          <input
            type="number"
            min={1}
            value={radiusKm}
            onChange={(e) => setRadiusKm(e.target.value)}
            placeholder="Radius"
          />
          <span>km</span>
        </div>

        <input
          value={systemKey}
          onChange={(e) => setSystemKey(e.target.value)}
          placeholder="System Key optional, z.B. tow"
        />

        <button type="submit" disabled={loading}>
          {loading ? "Suche..." : "Suchen"}
        </button>
      </form>

      {!loading && !error && (
        <GameList
          games={games}
          joiningKey={joiningKey}
          messageByKey={messageByKey}
          currentUserId={user.userId}
          onJoin={join}
        />
      )}
    </div>
  );
}