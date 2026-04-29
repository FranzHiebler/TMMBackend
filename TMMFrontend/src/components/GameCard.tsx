import type { GameResponse } from "../types/game";

type Props = {
  game: GameResponse;
  isJoining: boolean;
  onJoin: (gameId: string) => void;
};

export default function GameCard({ game, isJoining, onJoin }: Props) {
  return (
    <div className="card">
      <h3>{game.title}</h3>
      <div><b>System:</b> {game.system?.name}</div>
      <div><b>Host:</b> {game.host?.displayName}</div>
      <div><b>Spieler:</b> {game.participants?.length}/{game.maxPlayers} | Frei: {game.openSlots}</div>
      <div><b>Ort:</b> {game.location?.name}, {game.location?.city}</div>
      <div><b>Status:</b> {game.status}</div>
      <div><b>Start:</b> {new Date(game.startTimeUtc).toLocaleString("de-DE")}</div>
      {game.description && <div><b>Beschreibung:</b> {game.description}</div>}

      <div style={{ marginTop: 12 }}>
        <button
          onClick={() => onJoin(game.id)}
          disabled={game.openSlots === 0 || isJoining}
        >
          {isJoining ? "Trete bei..." : "Beitreten"}
        </button>
      </div>
    </div>
  );
}