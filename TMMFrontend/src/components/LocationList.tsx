import type { LocationResponse } from "../types/game";

type Props = {
  locations: LocationResponse[];
};

export default function LocationList({ locations }: Props) {
  return (
    <div>
      {locations.map((loc) => (
        <div key={loc.id} className="card">
          <h3>{loc.name}</h3>
          <p>{loc.city}</p>
        </div>
      ))}
    </div>
  );
}