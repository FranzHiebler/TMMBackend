import { GameJoinMode, type GameResponse } from "../types/game";

type Props = {
  game: GameResponse;
};

export default function GameCardHeader({ game }: Props) {
  const assignedPlayers = game.tables.reduce((sum, table) => sum + table.assignedPlayers.length, 0);
  const totalSeats = game.tables.reduce((sum, table) => sum + table.maxPlayers, 0);
  const freeSeats = Math.max(totalSeats - assignedPlayers, 0);

  return (
    <div className="game-session-header">
      <h3>{game.title}</h3>

      <div className="game-session-meta">
        <div className="game-session-meta-item">
          <span className="meta-label">Host</span>
          <span>{game.host?.displayName ?? "-"}</span>
        </div>

        <div className="game-session-meta-item">
          <span className="meta-label">Ort</span>
          <span>
            {game.location?.name}
            {game.location?.city ? `, ${game.location.city}` : ""}
          </span>
        </div>

        <div className="game-session-meta-item">
          <span className="meta-label">Start</span>
          <span>{new Date(game.startTimeUtc).toLocaleString("de-DE")}</span>
        </div>

        <div className="game-session-meta-item">
          <span className="meta-label">Modus</span>
          <span>
            {game.joinMode === GameJoinMode.ApprovalRequired
              ? "Bewerbung erforderlich"
              : "Direkter Beitritt"}
          </span>
        </div>

        <div className="game-session-meta-item">
          <span className="meta-label">Plätze</span>
          <span>
            {assignedPlayers}/{totalSeats} · {freeSeats} frei
          </span>
        </div>
      </div>

      {game.description && <p className="game-session-description">{game.description}</p>}
    </div>
  );
}