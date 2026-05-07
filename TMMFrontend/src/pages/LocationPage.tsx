import { useEffect, useState } from "react";
import { getMyLocations } from "../api/gamesService";
import type { LocationResponse } from "../types/game";
import LocationList from "../components/LocationList";
import LocationModal from "../components/LocationModal";
import { useUser } from "../context/UserContext";

export default function LocationsPage() {
  const user = useUser();

  const [locations, setLocations] = useState<LocationResponse[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [editLocation, setEditLocation] = useState<LocationResponse | null>(null);

  useEffect(() => {
    loadLocations();
  }, [user.userId]);

  async function loadLocations() {
    try {
      setLoading(true);
      setError("");

      const data = await getMyLocations(user);
      setLocations(Array.isArray(data) ? data : []);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Locations konnten nicht geladen werden.");
      setLocations([]);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="container">
      <div className="page-header">
        <h1>Meine Locations ({locations.length})</h1>

        <button type="button" onClick={() => setShowModal(true)}>
          + Neue Location
        </button>
      </div>

      {loading && <div className="message message-info">Lade Locations...</div>}
      {error && <div className="message message-error">{error}</div>}

      {!loading && !error && (
        <LocationList
          locations={locations}
          onEdit={(loc) => setEditLocation(loc)}
        />
      )}

      {(showModal || editLocation) && (
        <LocationModal
          location={editLocation ?? undefined}
          onClose={() => {
            setShowModal(false);
            setEditLocation(null);
          }}
          onCreated={(loc) => {
            setLocations((prev) =>
              prev.some((l) => l.id === loc.id)
                ? prev.map((l) => (l.id === loc.id ? loc : l))
                : [...prev, loc]
            );

            setShowModal(false);
            setEditLocation(null);
          }}
        />
      )}
    </div>
  );
}