import { useEffect, useState } from "react";
import type { CreateGameRequest, LocationResponse } from "../types/game";
import { createGame, getMyLocations } from "../api/gamesService";

export default function CreateGamePage() {
  const [locations, setLocations] = useState<LocationResponse[]>([]);
  const [locationId, setLocationId] = useState("");

  const [title, setTitle] = useState("");
  const [systemKey, setSystemKey] = useState("warhammer-old-world");
  const [systemName, setSystemName] = useState("Warhammer: The Old World");
  const [maxPlayers, setMaxPlayers] = useState(2);
  const [startTime, setStartTime] = useState("");
  const [description, setDescription] = useState("");

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    loadLocations();
  }, []);

  async function loadLocations() {
    try {
      setError("");
      const data = await getMyLocations();
      setLocations(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Locations konnten nicht geladen werden");
    }
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    if (!title || !systemKey || !systemName || !locationId || !startTime) {
      setError("Bitte alle Pflichtfelder ausfüllen.");
      return;
    }

    const request: CreateGameRequest = {
      title,
      systemKey,
      systemName,
      locationId,
      maxPlayers,
      startTimeUtc: new Date(startTime).toISOString(),
      description: description || null,
    };

    try {
      setLoading(true);
      setError("");
      await createGame(request);
      alert("Game wurde erstellt.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Game konnte nicht erstellt werden");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="container">
      <h1>Neues Game erstellen</h1>

      {error && <div className="message message-error">{error}</div>}

      <form onSubmit={handleSubmit} className="form">
        <input value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Titel" />

        <input value={systemKey} onChange={(e) => setSystemKey(e.target.value)} placeholder="System Key" />

        <input value={systemName} onChange={(e) => setSystemName(e.target.value)} placeholder="System Name" />

        <select value={locationId} onChange={(e) => setLocationId(e.target.value)}>
          <option value="">Location wählen</option>
          {locations.map((loc) => (
            <option key={loc.id} value={loc.id}>
              {loc.name} ({loc.city}) {loc.role ? `- ${loc.role}` : loc.isOpen ? "- Open" : ""}
            </option>
          ))}
        </select>

        <input
          type="number"
          min={2}
          value={maxPlayers}
          onChange={(e) => setMaxPlayers(Number(e.target.value))}
        />

        <input
          type="datetime-local"
          value={startTime}
          onChange={(e) => setStartTime(e.target.value)}
        />

        <textarea
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="Beschreibung"
        />

        <button disabled={loading}>{loading ? "Speichert..." : "Game erstellen"}</button>
      </form>
    </div>
  );
}