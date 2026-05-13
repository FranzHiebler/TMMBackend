import { useCallback, useEffect, useState } from "react";
import {
  GameJoinMode,
  type CreateGameRequest,
  type CreateGameTableRequest,
  type LocationResponse,
  type SystemOption,
} from "../types/game";
import { createGame } from "../api/gamesApi";
import { getMyLocations } from "../api/locationsApi";
import { getSystems } from "../api/systemsApi";
import LocationSelect from "./LocationSelect";
import LocationModal from "./LocationModal";
import GameTableEditor from "./GameTableEditor";
import { useUser } from "../context/UserContext";
import Message from "./Message";

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
  const user = useUser();

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
  const [joinMode, setJoinMode] = useState<GameJoinMode>(GameJoinMode.FirstComeFirstServe);

  const selectedLocation = locations.find((location) => location.id === locationId);
  const locationSystemKeys = selectedLocation?.systemKeys ?? [];
  const locationSystems = systems.filter((system) => locationSystemKeys.includes(system.key));

  const loadInitialData = useCallback(async () => {
    try {
      const [locationData, systemData] = await Promise.all([
        getMyLocations(user),
        getSystems(),
      ]);

      setLocations(locationData);
      setSystems(systemData);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Daten konnten nicht geladen werden");
    }
  }, [user]);

  useEffect(() => {
    void loadInitialData();
  }, [loadInitialData]);

  function updateTable(index: number, patch: Partial<CreateGameTableRequest>) {
    setTables((prev) => prev.map((table, i) => (i === index ? { ...table, ...patch } : table)));
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

  function updateCustomSystems(index: number, value: string) {
    const table = tables[index];
    const selectedKnownSystems = table.systems.filter((key) =>
      key === "egal" || locationSystemKeys.includes(key)
    );

    const customSystems = value
      .split(",")
      .map((system) => system.trim())
      .filter(Boolean);

    updateTable(index, {
      systems: [...selectedKnownSystems.filter((key) => key !== "egal"), ...customSystems],
    });
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    if (!title || !locationId || !startTime) {
      setError("Bitte Titel, Location und Startzeit ausfüllen.");
      return;
    }

    if (tables.length === 0 || tables.some((t) => !t.name || t.maxPlayers < 1)) {
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
        startTimeUtc: t.startTimeUtc || null,
      })),
    };

    try {
      setLoading(true);
      setError("");
      await createGame(request, user);
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
      <Message text={error} type="error" />

      <form onSubmit={handleSubmit} className="form">
        <input value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Titel" />

        <LocationSelect
          locations={locations}
          value={locationId}
          onChange={setLocationId}
          onCreateClick={() => setShowLocationModal(true)}
        />

        <select value={joinMode} onChange={(e) => setJoinMode(e.target.value as GameJoinMode)}>
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
          <GameTableEditor
            key={index}
            table={table}
            index={index}
            canRemove={tables.length > 1}
            locationSystemKeys={locationSystemKeys}
            locationSystems={locationSystems}
            onUpdateTable={updateTable}
            onRemoveTable={removeTable}
            onToggleSystem={toggleSystem}
            onCustomSystemsChange={updateCustomSystems}
          />
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