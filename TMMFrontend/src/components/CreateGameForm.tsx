import { useEffect, useState } from "react";
import {
  GameJoinMode,
  type CreateGameRequest,
  type CreateGameTableRequest,
  type LocationResponse,
  type SystemOption,
} from "../types/game";
import { createGame, getMyLocations, getSystems } from "../api/gamesService";
import LocationSelect from "./LocationSelect";
import LocationModal from "./LocationModal";

function newTable(index: number): CreateGameTableRequest {
  return {
    name: `Tisch ${index}`,
    maxPlayers: 2,
    systems: [],
    scenario: "",
    points: null,
    notes: "",
  };
}

export default function CreateGameForm() {
  const [locations, setLocations] = useState<LocationResponse[]>([]);
  const [systems, setSystems] = useState<SystemOption[]>([]);

  const [locationId, setLocationId] = useState("");
  const [showLocationModal, setShowLocationModal] = useState(false);

  const [title, setTitle] = useState("");
  const [startTime, setStartTime] = useState("");
  const [description, setDescription] = useState("");
  const [tables, setTables] = useState<CreateGameTableRequest[]>([newTable(1)]);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const [joinMode, setJoinMode] = useState<GameJoinMode>(GameJoinMode.FirstComeFirstServe)

  useEffect(() => {
    loadInitialData();
  }, []);

  async function loadInitialData() {
    try {
      setError("");
      const [locationData, systemData] = await Promise.all([
        getMyLocations(),
        getSystems(),
      ]);
      setLocations(locationData);
      setSystems(systemData);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Daten konnten nicht geladen werden");
    }
  }

  function updateTable(index: number, patch: Partial<CreateGameTableRequest>) {
    setTables((prev) =>
      prev.map((table, i) => (i === index ? { ...table, ...patch } : table))
    );
  }

  function addTable() {
    setTables((prev) => [...prev, newTable(prev.length + 1)]);
  }

  function removeTable(index: number) {
    setTables((prev) => prev.filter((_, i) => i !== index));
  }

  function toggleSystem(index: number, key: string) {
    const table = tables[index];

    if (key === "egal") {
      updateTable(index, { systems: table.systems.includes("egal") ? [] : ["egal"] });
      return;
    }

    const withoutEgal = table.systems.filter((x) => x !== "egal");
    const next = withoutEgal.includes(key)
      ? withoutEgal.filter((x) => x !== key)
      : [...withoutEgal, key];

    updateTable(index, { systems: next });
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    if (!title || !locationId || !startTime) {
      setError("Bitte Titel, Location und Startzeit ausfüllen.");
      return;
    }

    if (tables.length === 0) {
      setError("Mindestens ein Tisch ist erforderlich.");
      return;
    }

    if (tables.some((t) => !t.name || t.maxPlayers < 1)) {
      setError("Jeder Tisch braucht Name und mindestens 1 Spieler.");
      return;
    }

    const request: CreateGameRequest = {
      title,
      locationId,
      clubId: null,
      startTimeUtc: new Date(startTime).toISOString(),
      description: description || null,
      joinMode,
      tables: tables.map((t) => ({
        ...t,
        scenario: t.scenario || null,
        notes: t.notes || null,
        points: t.points || null,
      })),
    };

    try {
      setLoading(true);
      setError("");
      await createGame(request);
      alert("Game Session wurde erstellt.");
      setTitle("");
      setDescription("");
      setTables([newTable(1)]);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Game konnte nicht erstellt werden");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="container">
      {error && <div className="message message-error">{error}</div>}

      <form onSubmit={handleSubmit} className="form">
        <input value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Titel" />

        <LocationSelect
          locations={locations}
          value={locationId}
          onChange={setLocationId}
          onCreateClick={() => setShowLocationModal(true)}
        />

        <select value={joinMode} onChange={(e) => setJoinMode(Number(e.target.value) as GameJoinMode)}>
          <option value={GameJoinMode.FirstComeFirstServe}>First Come First Serve</option>
          <option value={GameJoinMode.ApprovalRequired}>Approval Required</option>
        </select>

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

        <h2>Tische</h2>

        {tables.map((table, index) => (
          <div key={index} className="card">
            <input
              value={table.name}
              onChange={(e) => updateTable(index, { name: e.target.value })}
              placeholder="Tischname"
            />

            <input
              type="number"
              min={1}
              value={table.maxPlayers}
              onChange={(e) => updateTable(index, { maxPlayers: Number(e.target.value) })}
              placeholder="Max Spieler"
            />

            <div>
              <b>Systeme:</b>
              <label style={{ marginLeft: 8 }}>
                <input
                  type="checkbox"
                  checked={table.systems.includes("egal")}
                  onChange={() => toggleSystem(index, "egal")}
                />
                Egal
              </label>

              {systems.map((s) => (
                <label key={s.key} style={{ marginLeft: 8 }}>
                  <input
                    type="checkbox"
                    checked={table.systems.includes(s.key)}
                    disabled={table.systems.includes("egal")}
                    onChange={() => toggleSystem(index, s.key)}
                  />
                  {s.name}
                </label>
              ))}
            </div>

            <input
              value={table.scenario ?? ""}
              onChange={(e) => updateTable(index, { scenario: e.target.value })}
              placeholder="Szenario optional"
            />

            <input
              type="number"
              value={table.points ?? ""}
              onChange={(e) =>
                updateTable(index, {
                  points: e.target.value ? Number(e.target.value) : null,
                })
              }
              placeholder="Punkte optional"
            />

            <input
              value={table.notes ?? ""}
              onChange={(e) => updateTable(index, { notes: e.target.value })}
              placeholder="Notizen optional"
            />

            {tables.length > 1 && (
              <button type="button" onClick={() => removeTable(index)}>
                Tisch entfernen
              </button>
            )}
          </div>
        ))}

        <button type="button" onClick={addTable}>
          + Tisch hinzufügen
        </button>

        <button type="submit" disabled={loading}>
          {loading ? "Speichere..." : "Game Session erstellen"}
        </button>
      </form>

      {showLocationModal && (
        <LocationModal
          onClose={() => setShowLocationModal(false)}
          onCreated={(loc) => {
            setLocations((prev) => [...prev, loc]);
            setLocationId(loc.id);
            setShowLocationModal(false);
          }}
        />
      )}
    </div>
  );
}