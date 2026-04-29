import { useEffect, useState } from "react";
import { getLocations } from "../api/gamesService";
import type { LocationResponse } from "../types/game";
import LocationList from "../components/LocationList";

export default function LocationsPage() {
    const [locations, setLocations] = useState<LocationResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        console.log("useEffect läuft");
        loadLocations();
    }, []);

    async function loadLocations() {
        try {
            setLoading(true);
            setError("");
            const data = await getLocations();
            setLocations(data);
        } catch (err) {
            setError(err instanceof Error ? err.message : "Fehler");
        } finally {
            setLoading(false);
        }
    }

    return (
        <div className="container">
            <h1>Locations ({locations.length})</h1>

            {loading && <div className="message message-info">Lade Locations...</div>}
            {error && <div className="message message-error">{error}</div>}

            {!loading && !error && <LocationList locations={locations} />}
        </div>
    );
}