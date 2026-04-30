import type { LocationResponse } from "../types/game";

type Props = {
  locations: LocationResponse[];
  onEdit: (location: LocationResponse) => void;
};

export default function LocationList({ locations, onEdit }: Props) {
  if (locations.length === 0) {
    return <div className="message message-info">Du hast noch keine Locations.</div>;
  }

  return (
    <div className="location-list">
      {locations.map((loc) => (
        <div key={loc.id} className="card">
          <h3>{loc.name}</h3>

          <p>{loc.city}</p>

          {loc.address && <p>{loc.address}</p>}

          <small>
            {loc.role ? loc.role : loc.isOpen ? "Open" : "Keine Rolle"}
          </small>
          <button type="button" onClick={() => onEdit(loc)}>Bearbeiten</button>
        </div>
      ))}
    </div>
  );
}