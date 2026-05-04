import { useEffect, useState } from "react";
import type { CreateLocationRequest, LocationResponse } from "../types/game";
import { createLocation, updateLocation } from "../api/gamesService";
import "leaflet/dist/leaflet.css";
import LocationPicker from "./LocationPicker";

type Props = {
  onClose: () => void;
  onCreated: (location: LocationResponse) => void;
  location?: LocationResponse; 
};

export default function LocationModal({ onClose, onCreated, location }: Props) {
  const [name, setName] = useState(location?.name ?? "");
  const [city, setCity] = useState(location?.city ?? "");
  const [address, setAddress] = useState(location?.address ?? "");
  const [latitude, setLatitude] = useState<number | null>(location?.latitude ?? null);
  const [longitude, setLongitude] = useState<number | null>(location?.longitude ?? null);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");


  const isValid =
    name.trim() &&
    city.trim() &&
    address.trim() &&
    latitude != null &&
    longitude != null;



async function handleSubmit(e: React.FormEvent) {
  e.preventDefault();

  if (!name || !city || !address || latitude == null || longitude == null) {
    setError("Bitte Name, Stadt, Adresse und Standort eingeben.");
    return;
  }

  const request: CreateLocationRequest = {
    name,
    city,
    address,
    latitude,
    longitude,
  };

  try {
    setLoading(true);
    setError("");

    if (location) {
      await updateLocation(location.id, request);
      onCreated({ ...location, ...request });
    } else {
      const created = await createLocation(request);
      onCreated(created);
    }

    onClose();
  } catch (err) {
    setError(err instanceof Error ? err.message : "Fehler beim Speichern");
  } finally {
    setLoading(false);
  }
}

async function searchAddress() {
  const query = `${address}, ${city}, Deutschland`.trim();
  if (query.length < 5) return;

  try {
    const res = await fetch(
      `https://nominatim.openstreetmap.org/search?format=json&countrycodes=de&q=${encodeURIComponent(query)}&limit=1`
    );

    const data = await res.json();

    if (!data.length) {
      setError("Adresse nicht gefunden. Bitte Marker manuell auf der Karte setzen.");
      return;
    }

    setError("");
    setLatitude(Number(data[0].lat));
    setLongitude(Number(data[0].lon));
  } catch {
    setError("Adresse konnte nicht gesucht werden.");
  }
}

  useEffect(() => {
    if (!city && !address) return;

    const timeout = window.setTimeout(() => {
      searchAddress();
    }, 800);

    return () => window.clearTimeout(timeout);
  }, [city, address]);

  return (
    <div className="modal-backdrop">
      <div className="modal">
        <h2>{location ? "Location bearbeiten" : "Neue Location"}</h2>

        {error && <div className="error">{error}</div>}

        <form onSubmit={handleSubmit}>
          <input value={name} onChange={(e) => setName(e.target.value)} placeholder="Name der Location" />
          <input value={city} onChange={(e) => setCity(e.target.value)} placeholder="Stadt" />
          <input
            value={address}
            onChange={(e) => {
              setAddress(e.target.value);
              setLatitude(null);
              setLongitude(null);
            }}
            placeholder="Straße, Hausnummer"
          />

          <LocationPicker
            latitude={latitude}
            longitude={longitude}
            onChange={(lat, lng) => {
              setLatitude(lat);
              setLongitude(lng);
            }}
          />          

          <div className="modal-actions">
            <button type="button" onClick={onClose}>
              Abbrechen
            </button>

            <button type="submit" disabled={loading || !isValid}>
              {loading ? "Speichert..." : "Speichern"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}