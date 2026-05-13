import type { CreateGameTableRequest, SystemOption } from "../types/game";
import GameTableSystemsPicker from "./GameTableSystemsPicker";

type Props = {
  table: CreateGameTableRequest;
  index: number;
  canRemove: boolean;
  locationSystemKeys: string[];
  locationSystems: SystemOption[];
  onUpdateTable: (index: number, patch: Partial<CreateGameTableRequest>) => void;
  onRemoveTable: (index: number) => void;
  onToggleSystem: (index: number, key: string) => void;
  onCustomSystemsChange: (index: number, value: string) => void;
};

export default function GameTableEditor({
  table,
  index,
  canRemove,
  locationSystemKeys,
  locationSystems,
  onUpdateTable,
  onRemoveTable,
  onToggleSystem,
  onCustomSystemsChange,
}: Props) {
  return (
    <div className="card form">
      <input
        value={table.name}
        onChange={(e) => onUpdateTable(index, { name: e.target.value })}
        placeholder="Tischname"
      />

      <input
        type="number"
        min={1}
        value={table.maxPlayers}
        onChange={(e) => onUpdateTable(index, { maxPlayers: Number(e.target.value) })}
        placeholder="Max Spieler"
      />

      <GameTableSystemsPicker
        table={table}
        index={index}
        locationSystemKeys={locationSystemKeys}
        locationSystems={locationSystems}
        onToggleSystem={onToggleSystem}
        onCustomSystemsChange={onCustomSystemsChange}
      />

      <input
        type="datetime-local"
        value={table.startTimeUtc ? table.startTimeUtc.slice(0, 16) : ""}
        onChange={(e) =>
          onUpdateTable(index, {
            startTimeUtc: e.target.value ? new Date(e.target.value).toISOString() : null,
          })
        }
        placeholder="Abweichende Tisch-Startzeit"
      />

      <input
        value={table.scenario ?? ""}
        onChange={(e) => onUpdateTable(index, { scenario: e.target.value })}
        placeholder="Szenario optional"
      />

      <input
        type="number"
        value={table.points ?? ""}
        onChange={(e) =>
          onUpdateTable(index, {
            points: e.target.value ? Number(e.target.value) : null,
          })
        }
        placeholder="Punkte optional"
      />

      <input
        value={table.notes ?? ""}
        onChange={(e) => onUpdateTable(index, { notes: e.target.value })}
        placeholder="Notizen optional"
      />

      {canRemove && (
        <button type="button" onClick={() => onRemoveTable(index)}>
          Tisch entfernen
        </button>
      )}
    </div>
  );
}