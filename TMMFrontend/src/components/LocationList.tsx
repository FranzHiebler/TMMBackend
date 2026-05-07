import type { LocationResponse } from "../types/game";
import LocationMembersPanel from "./LocationMembersPanel";

type Props = {
  locations: LocationResponse[];
  onEdit: (location: LocationResponse) => void;
};

export default function LocationList({ locations, onEdit }: Props) {
  return (
    <div className="location-list">
      {locations.map((loc) => (
        <div key={loc.id} className="card location-card-grid">
          <div className="location-main">
            <h3>{loc.name}</h3>
            <p>{loc.city}</p>
            {loc.address && <p>{loc.address}</p>}

            <small>{loc.role ? loc.role : loc.isOpen ? "Open" : "Keine Rolle"}</small>

            <div className="location-actions">
              <button type="button" onClick={() => onEdit(loc)}>
                Bearbeiten
              </button>
            </div>
          </div>

          <div className="location-members-side">
            <LocationMembersPanel location={loc} />
          </div>
        </div>
      ))}
    </div>
  );
}