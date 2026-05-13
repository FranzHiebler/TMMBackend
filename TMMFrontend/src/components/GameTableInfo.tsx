import type { GameTableDto } from "../types/game";

type Props = {
  table: GameTableDto;
};

function systemText(table: GameTableDto) {
  if (!table.systems || table.systems.length === 0) return "Egal";
  if (table.systems.some((x) => x.toLowerCase() === "egal")) return "Egal";
  return table.systems.join(", ");
}

export default function GameTableInfo({ table }: Props) {
  return (
    <>
      <h4>{table.name}</h4>

      <div><b>System:</b> {systemText(table)}</div>
      <div><b>Spieler:</b> {table.assignedPlayers.length}/{table.maxPlayers}</div>

      {table.startTimeUtc && (
        <div><b>Start am Tisch:</b> {new Date(table.startTimeUtc).toLocaleString("de-DE")}</div>
      )}

      {table.points && <div><b>Punkte:</b> {table.points}</div>}
      {table.scenario && <div><b>Szenario:</b> {table.scenario}</div>}
      {table.notes && <div><b>Notizen:</b> {table.notes}</div>}
    </>
  );
}