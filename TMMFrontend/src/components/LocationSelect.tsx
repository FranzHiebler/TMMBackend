import type { LocationResponse } from "../types/game";

type Props = {
  locations: LocationResponse[];
  value: string;
  onChange: (id: string) => void;
  onCreateClick: () => void;
};

export default function LocationSelect({
  locations,
  value,
  onChange,
  onCreateClick,
}: Props) {
  return (
    <div>
      <select value={value} onChange={(e) => onChange(e.target.value)}>
        <option value="">Location wählen</option>

        {locations.map((l) => (
          <option key={l.id} value={l.id}>
            {l.name} ({l.city})
            {l.role ? ` - ${l.role}` : ""}
            {l.isOpen ? " - Open" : ""}
          </option>
        ))}
      </select>

      <button type="button" onClick={onCreateClick}>
        + Neue Location
      </button>
    </div>
  );
}