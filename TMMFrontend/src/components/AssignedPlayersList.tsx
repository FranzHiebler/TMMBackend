import type { GameTableDto } from "../types/game";

type Props = {
  table: GameTableDto;
  isHost: boolean;
  busyKey: string | null;
  onRemovePlayer: (tableId: string, userId: string) => void;
  onDragPlayerStart: (userId: string) => void;
  onDragPlayerEnd: () => void;
};

export default function AssignedPlayersList({
  table,
  isHost,
  busyKey,
  onRemovePlayer,
  onDragPlayerStart,
  onDragPlayerEnd,
}: Props) {
  if (table.assignedPlayers.length === 0) return null;

  return (
    <div className="assigned-player-list">
      <b>Zugewiesen:</b>

      {table.assignedPlayers.map((player) => (
        <div
          key={player.userId}
          className="assigned-player-row"
          draggable={isHost}
          onDragStart={() => onDragPlayerStart(player.userId)}
          onDragEnd={onDragPlayerEnd}
        >
          <div className="assigned-player-name">
            {player.displayName}
          </div>

          {isHost && (
            <button
              type="button"
              className="assigned-player-remove"
              disabled={busyKey === `player-remove-${player.userId}`}
              onClick={() => onRemovePlayer(table.id, player.userId)}
            >
              Entfernen
            </button>
          )}
        </div>
      ))}
    </div>
  );
}