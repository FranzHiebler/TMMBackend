import type { GameResponse, GameTableDto, GameJoinMode } from "../types/game";
import AssignedPlayersList from "./AssignedPlayersList";
import ApplicationsList from "./ApplicationsList";

type Props = {
  game: GameResponse;
  table: GameTableDto;
  isHost: boolean;
  isApproval: boolean;
  alreadyInGame: boolean;
  joiningKey: string | null;
  messageByKey: Record<string, string>;
  currentUserId: string;
  busyKey: string | null;

  openProposalTableId: string | null;
  proposalStartTime: string;
  proposalSystems: string;
  proposalPoints: string;
  proposalMessage: string;

  onJoin: (gameId: string, tableId: string, joinMode: GameJoinMode, systemKey?: string) => void;
  onOpenProposalTableIdChange: (tableId: string | null) => void;
  onProposalStartTimeChange: (value: string) => void;
  onProposalSystemsChange: (value: string) => void;
  onProposalPointsChange: (value: string) => void;
  onProposalMessageChange: (value: string) => void;
  onSubmitProposal: (table: GameTableDto) => void;

  onAcceptApplication: (tableId: string, applicationId: string) => void;
  onRejectApplication: (applicationId: string) => void;

  onRemovePlayer: (tableId: string, userId: string) => void;
  onDragPlayerStart: (userId: string) => void;
  onDragPlayerEnd: () => void;
  onDropPlayer: (targetTableId: string) => void;
};

function systemText(table: GameTableDto) {
  if (!table.systems || table.systems.length === 0) return "Egal";
  if (table.systems.some((x) => x.toLowerCase() === "egal")) return "Egal";
  return table.systems.join(", ");
}

export default function GameTableCard({
  game,
  table,
  isHost,
  isApproval,
  alreadyInGame,
  joiningKey,
  messageByKey,
  currentUserId,
  busyKey,
  openProposalTableId,
  proposalStartTime,
  proposalSystems,
  proposalPoints,
  proposalMessage,
  onJoin,
  onOpenProposalTableIdChange,
  onProposalStartTimeChange,
  onProposalSystemsChange,
  onProposalPointsChange,
  onProposalMessageChange,
  onSubmitProposal,
  onAcceptApplication,
  onRejectApplication,
  onRemovePlayer,
  onDragPlayerStart,
  onDragPlayerEnd,
  onDropPlayer,
}: Props) {
  const key = `${game.id}_${table.id}`;
  const isFull = table.openSlots <= 0;
  const isJoining = joiningKey === key;
  const isAssignedToTable = table.assignedPlayers.some((p) => p.userId === currentUserId);
  const pendingApplications = table.applications.filter((a) => a.status === "Pending");

  const systemKey =
    !table.systems.length || table.systems.some((x) => x.toLowerCase() === "egal")
      ? undefined
      : table.systems[0];

  return (
    <div
      className="card"
      style={{ marginTop: 10 }}
      onDragOver={(e) => e.preventDefault()}
      onDrop={() => onDropPlayer(table.id)}
    >
      <h4>{table.name}</h4>

      <div><b>System:</b> {systemText(table)}</div>
      <div><b>Spieler:</b> {table.assignedPlayers.length}/{table.maxPlayers}</div>

      {table.startTimeUtc && (
        <div><b>Start am Tisch:</b> {new Date(table.startTimeUtc).toLocaleString("de-DE")}</div>
      )}

      {table.points && <div><b>Punkte:</b> {table.points}</div>}
      {table.scenario && <div><b>Szenario:</b> {table.scenario}</div>}
      {table.notes && <div><b>Notizen:</b> {table.notes}</div>}

      <AssignedPlayersList
        table={table}
        isHost={isHost}
        busyKey={busyKey}
        onRemovePlayer={onRemovePlayer}
        onDragPlayerStart={onDragPlayerStart}
        onDragPlayerEnd={onDragPlayerEnd}
      />

      <div className="table-actions">
        <button
          disabled={isFull || isJoining || alreadyInGame}
          onClick={() => onJoin(game.id, table.id, game.joinMode, systemKey)}
        >
          {isJoining
            ? "Bitte warten..."
            : isFull
              ? "Voll"
              : alreadyInGame
                ? "Bereits angemeldet"
                : isApproval
                  ? "Bewerben"
                  : "Beitreten"}
        </button>

        {isAssignedToTable && (
          <button
            type="button"
            onClick={() =>
              onOpenProposalTableIdChange(openProposalTableId === table.id ? null : table.id)
            }
          >
            Änderung vorschlagen
          </button>
        )}
      </div>

      {openProposalTableId === table.id && (
        <div className="proposal-form">
          <input
            type="datetime-local"
            value={proposalStartTime}
            onChange={(e) => onProposalStartTimeChange(e.target.value)}
          />

          <input
            value={proposalSystems}
            onChange={(e) => onProposalSystemsChange(e.target.value)}
            placeholder="Systeme, z.B. tow, wh40k"
          />

          <input
            type="number"
            min={0}
            value={proposalPoints}
            onChange={(e) => onProposalPointsChange(e.target.value)}
            placeholder="Punkte"
          />

          <textarea
            value={proposalMessage}
            onChange={(e) => onProposalMessageChange(e.target.value)}
            placeholder="Nachricht optional"
          />

          <button
            type="button"
            disabled={busyKey === `proposal-submit-${table.id}`}
            onClick={() => onSubmitProposal(table)}
          >
            Vorschlag senden
          </button>
        </div>
      )}

      {messageByKey[key] && (
        <div className="message message-info" style={{ marginTop: 8 }}>
          {messageByKey[key]}
        </div>
      )}

      <ApplicationsList
        table={table}
        isHost={isHost}
        pendingApplications={pendingApplications}
        busyKey={busyKey}
        onAcceptApplication={onAcceptApplication}
        onRejectApplication={onRejectApplication}
      />
    </div>
  );
}