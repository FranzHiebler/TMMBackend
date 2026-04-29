import { useState } from "react";
import type { CreateLocationRequest, LocationResponse } from "../types/game";
import { createLocation } from "../api/gamesService";

type Props = {
  onClose: () => void;
  onCreated: (location: LocationResponse) => void;
};

export default function LocationModal({ onClose, onCreated }: Props) {
  const [name, setName] = useState("");
  const [city, setCity] = useState("");
  const [address, setAddress] = useState("");
  const [latitude, setLatitude] = useState("");
  const [longitude, setLongitude] = useState("");

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    if (!name || !city) {
      setError("Name und Stadt sind Pflicht.");
      return;
    }

    const request: CreateLocationRequest = {
      name,
      city,
      address: address || null,
      latitude: latitude ? Number(latitude) : null,
      longitude: longitude ? Number(longitude) : null,
    };

    try {
      setLoading(true);
      setError("");
      const created = await createLocation(request);
      onCreated(created);
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Fehler beim Erstellen");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="modal-backdrop">
      <div className="modal">
        <h2>Neue Location</h2>

        {error && <div className="error">{error}</div>}

        <form onSubmit={handleSubmit}>
          <input value={name} onChange={(e) => setName(e.target.value)} placeholder="Name" />
          <input value={city} onChange={(e) => setCity(e.target.value)} placeholder="Stadt" />
          <input value={address} onChange={(e) => setAddress(e.target.value)} placeholder="Adresse" />

          <input
            type="number"
            placeholder="Latitude"
            value={latitude}
            onChange={(e) => setLatitude(e.target.value)}
          />

          <input
            type="number"
            placeholder="Longitude"
            value={longitude}
            onChange={(e) => setLongitude(e.target.value)}
          />

          <div className="modal-actions">
            <button type="button" onClick={onClose}>
              Abbrechen
            </button>

            <button type="submit" disabled={loading}>
              {loading ? "Speichert..." : "Erstellen"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}