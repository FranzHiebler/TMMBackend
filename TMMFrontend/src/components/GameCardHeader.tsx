import type { GameResponse } from "../types/game";

type Props = {
  game: GameResponse;
  isApproval: boolean;
};

export default function GameCardHeader({ game, isApproval }: Props) {
  return (
    <>
      <h3>{game.title}</h3>

      <div><b>Host:</b> {game.host?.displayName}</div>
      <div><b>Ort:</b> {game.location?.name}, {game.location?.city}</div>
      <div><b>Start:</b> {new Date(game.startTimeUtc).toLocaleString("de-DE")}</div>
      <div><b>Modus:</b> {isApproval ? "Bewerbung erforderlich" : "Direkt beitreten"}</div>
      <div><b>Plätze:</b> {game.assignedPlayers}/{game.maxPlayers} | Frei: {game.openSlots}</div>

      {game.description && <p>{game.description}</p>}
    </>
  );
}