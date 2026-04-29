import { useEffect, useState } from "react";
import { useUser } from "../context/UserContext";

import type {
  CreateGameRequest,
  LocationOption,
  SystemOption,
} from "../types/game";
import { getLocations, getSystems } from "../api/gamesService";

type Props = {
  onCreate: (request: CreateGameRequest) => void;
};

export default function CreateGameForm({ onCreate }: Props) {
  const [title, setTitle] = useState("");
  const [maxPlayers, setMaxPlayers] = useState(2);
  const [selectedLocationId, setSelectedLocationId] = useState("");
  const [selectedSystemKey, setSelectedSystemKey] = useState("");
  const [startTimeUtc, setStartTimeUtc] = useState("");
  const [description, setDescription] = useState("");

  const [locations, setLocations] = useState<LocationOption[]>([]);
  const [systems, setSystems] = useState<SystemOption[]>([]);

  const [loadingLocations, setLoadingLocations] = useState(true);
  const [loadingSystems, setLoadingSystems] = useState(true);

  const [locationError, setLocationError] = useState("");
  const [systemError, setSystemError] = useState("");

  const user = useUser();

  useEffect(() => {
    async function loadLocations() {
      try {
        setLoadingLocations(true);
        setLocationError("");
        const data = await getLocations();
        setLocations(data);
      } catch (err) {
        setLocationError(
          err instanceof Error
            ? err.message
            : "Locations konnten nicht geladen werden"
        );
      } finally {
        setLoadingLocations(false);
      }
    }

    async function loadSystems() {
      try {
        setLoadingSystems(true);
        setSystemError("");
        const data = await getSystems();
        setSystems(data);

        if (data.length > 0) {
          setSelectedSystemKey(data[0].key);
        }
      } catch (err) {
        setSystemError(
          err instanceof Error
            ? err.message
            : "Systeme konnten nicht geladen werden"
        );
      } finally {
        setLoadingSystems(false);
      }
    }

    loadLocations();
    loadSystems();
  }, []);

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    const selectedLocation = locations.find((x) => x.id === selectedLocationId);
    const selectedSystem = systems.find((x) => x.key === selectedSystemKey);

    if (
      !title ||
      !user ||
      !startTimeUtc ||
      !selectedLocation ||
      !selectedSystem
    ) {
      alert("Bitte Pflichtfelder ausfüllen");
      return;
    }

    onCreate({
      title,
      systemKey: selectedSystem.key,
      systemName: selectedSystem.name,
      hostUserId: user.userId,
      hostDisplayName: user.displayName,
      maxPlayers,
      locationId: selectedLocation.id,
      locationName: selectedLocation.name,
      locationCity: selectedLocation.city,
      startTimeUtc: new Date(startTimeUtc).toISOString(),
      description,
    });
  }

  return (
    <form onSubmit={handleSubmit} className="form">
      <h2>Neues Game erstellen</h2>

      <div className="form-group">
        <input
          type="text"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="Titel"
        />
      </div>

      <div className="form-group">
        <input
          type="number"
          value={maxPlayers}
          onChange={(e) => setMaxPlayers(Number(e.target.value))}
          placeholder="Max Players"
          min={2}
        />
      </div>

      <div className="form-group">
        {loadingSystems ? (
          <div>Lade Systeme...</div>
        ) : systemError ? (
          <div style={{ color: "red" }}>Fehler bei Systemen: {systemError}</div>
        ) : (
          <select
            value={selectedSystemKey}
            onChange={(e) => setSelectedSystemKey(e.target.value)}
          >
            <option value="">Bitte System wählen</option>
            {systems.map((system) => (
              <option key={system.key} value={system.key}>
                {system.name}
              </option>
            ))}
          </select>
        )}
      </div>

      <div className="form-group">
        {loadingLocations ? (
          <div>Lade Locations...</div>
        ) : locationError ? (
          <div style={{ color: "red" }}>
            Fehler bei Locations: {locationError}
          </div>
        ) : (
          <select
            value={selectedLocationId}
            onChange={(e) => setSelectedLocationId(e.target.value)}
          >
            <option value="">Bitte Location wählen</option>
            {locations.map((location) => (
              <option key={location.id} value={location.id}>
                {location.name} ({location.city})
              </option>
            ))}
          </select>
        )}
      </div>

      <div className="form-group">
        <input
          type="datetime-local"
          value={startTimeUtc}
          onChange={(e) => setStartTimeUtc(e.target.value)}
        />
      </div>

      <div className="form-group">
        <textarea
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="Beschreibung"
          rows={4}
        />
      </div>

      <button type="submit" disabled={loadingLocations || loadingSystems}>
        Game erstellen
      </button>
    </form>
  );
}