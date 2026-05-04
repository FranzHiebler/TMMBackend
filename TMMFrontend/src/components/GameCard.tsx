import { type GameResponse, type GameTableDto, GameJoinMode } from "../types/game";

type Props = {
  game: GameResponse;
  joiningKey: string | null;
  onJoin: (gameId: string, tableId: string, systemKey?: string) => void;
};

function systemText(table: GameTableDto) {
  if (!table.systems || table.systems.length === 0) return "Egal";
  if (table.systems.some((x) => x.toLowerCase() === "egal")) return "Egal";
  return table.systems.join(", ");
}

export default function GameCard({ game, joiningKey, onJoin }: Props) {
  const isApproval = game.joinMode === GameJoinMode.ApprovalRequired;

  return (
    <div className="card">
      <h3>{game.title}</h3>

      <div><b>Host:</b> {game.host?.displayName}</div>
      <div><b>Ort:</b> {game.location?.name}, {game.location?.city}</div>
      <div><b>Start:</b> {new Date(game.startTimeUtc).toLocaleString("de-DE")}</div>
      <div><b>Modus:</b> {isApproval ? "Bewerbung erforderlich" : "Direkt beitreten"}</div>
      <div><b>Plätze:</b> {game.assignedPlayers}/{game.maxPlayers} | Frei: {game.openSlots}</div>

      {game.description && <p>{game.description}</p>}

      <div style={{ marginTop: 12 }}>
        {game.tables.map((table) => {
          const key = `${game.id}_${table.id}`;
          const isFull = table.openSlots <= 0;
          const isJoining = joiningKey === key;
          const systemKey =
            !table.systems.length || table.systems.some(x => x.toLowerCase() === "egal")
              ? undefined
              : table.systems[0];
          return (
            <div key={table.id} className="card" style={{ marginTop: 10 }}>
              <h4>{table.name}</h4>

              <div><b>System:</b> {systemText(table)}</div>
              <div><b>Spieler:</b> {table.assignedPlayers.length}/{table.maxPlayers}</div>

              {table.points && <div><b>Punkte:</b> {table.points}</div>}
              {table.scenario && <div><b>Szenario:</b> {table.scenario}</div>}
              {table.notes && <div><b>Notizen:</b> {table.notes}</div>}

              {table.assignedPlayers.length > 0 && (
                <div>
                  <b>Zugewiesen:</b>{" "}
                  {table.assignedPlayers.map((p) => p.displayName).join(", ")}
                </div>
              )}

              <button
                style={{ marginTop: 8 }}
                disabled={isFull || isJoining}
                onClick={() => onJoin(game.id, table.id, systemKey)}
              >
                {isJoining
                  ? "Bitte warten..."
                  : isFull
                    ? "Voll"
                    : isApproval
                      ? "Bewerben"
                      : "Beitreten"}
              </button>
            </div>
          );
        })}
      </div>
    </div>
  );
}