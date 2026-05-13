import type { GameResponse, GameTableDto, GameJoinMode } from "../types/game";
import AssignedPlayersList from "./AssignedPlayersList";
import ApplicationsList from "./ApplicationsList";
import GameTableActions from "./GameTableActions";
import GameProposalForm from "./GameProposalForm";
import Message from "./Message";

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
  proposalSystems: string[];
  proposalCustomSystems: string;
  proposalPoints: string;
  proposalMessage: string;

  onJoin: (gameId: string, tableId: string, joinMode: GameJoinMode, systemKey?: string) => void;
  onOpenProposalTableIdChange: (tableId: string | null) => void;
  onOpenProposalTable: (table: GameTableDto) => void;
  onProposalStartTimeChange: (value: string) => void;
  onToggleProposalSystem: (system: string) => void;
  onProposalCustomSystemsChange: (value: string) => void;
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
  proposalCustomSystems,
  proposalPoints,
  proposalMessage,
  onJoin,
  onOpenProposalTableIdChange,
  onOpenProposalTable,
  onProposalStartTimeChange,
  onToggleProposalSystem,
  onProposalCustomSystemsChange,
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

  const availableSystems = table.systems.length ? table.systems : ["egal"];

  return (
    <div
      className="card game-table-card"
      onDragOver={(e) => e.preventDefault()}
      onDrop={() => onDropPlayer(table.id)}
    >
      <div className="game-table-header">
        <div>
          <h4>{table.name}</h4>
          <div className="game-table-subtitle">
            {table.assignedPlayers.length}/{table.maxPlayers} Spieler · {table.openSlots} frei
          </div>
        </div>

        <GameTableActions
          isFull={isFull}
          isJoining={isJoining}
          alreadyInGame={alreadyInGame}
          isApproval={isApproval}
          isAssignedToTable={isAssignedToTable}
          onJoin={() => onJoin(game.id, table.id, game.joinMode, systemKey)}
          onToggleProposal={() => {
            if (openProposalTableId === table.id) {
              onOpenProposalTableIdChange(null);
              return;
            }

            onOpenProposalTable(table);
          }}
        />
      </div>

      <div className="game-table-meta">
        <div>
          <b>System:</b> {systemText(table)}
        </div>

        {table.startTimeUtc && (
          <div>
            <b>Start:</b>{" "}
            {new Date(table.startTimeUtc).toLocaleTimeString("de-DE", {
              hour: "2-digit",
              minute: "2-digit",
            })}
          </div>
        )}

        {table.points != null && (
          <div>
            <b>Punkte:</b> {table.points}
          </div>
        )}

        {table.scenario && (
          <div>
            <b>Szenario:</b> {table.scenario}
          </div>
        )}
      </div>

      {table.notes && <div className="game-table-notes">{table.notes}</div>}

      <AssignedPlayersList
        table={table}
        isHost={isHost}
        busyKey={busyKey}
        onRemovePlayer={onRemovePlayer}
        onDragPlayerStart={onDragPlayerStart}
        onDragPlayerEnd={onDragPlayerEnd}
      />

      {openProposalTableId === table.id && (
        <GameProposalForm
          proposalStartTime={proposalStartTime}
          proposalSystems={proposalSystems}
          proposalCustomSystems={proposalCustomSystems}
          proposalPoints={proposalPoints}
          proposalMessage={proposalMessage}
          availableSystems={availableSystems}
          isBusy={busyKey === `proposal-submit-${table.id}`}
          onProposalStartTimeChange={onProposalStartTimeChange}
          onToggleProposalSystem={onToggleProposalSystem}
          onProposalCustomSystemsChange={onProposalCustomSystemsChange}
          onProposalPointsChange={onProposalPointsChange}
          onProposalMessageChange={onProposalMessageChange}
          onSubmit={() => onSubmitProposal(table)}
        />
      )}

      <Message text={messageByKey[key]} type="info" />

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