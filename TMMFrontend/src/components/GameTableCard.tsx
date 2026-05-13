import type { GameResponse, GameTableDto, GameJoinMode } from "../types/game";
import AssignedPlayersList from "./AssignedPlayersList";
import ApplicationsList from "./ApplicationsList";
import GameTableInfo from "./GameTableInfo";
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
      <GameTableInfo table={table} />

      <AssignedPlayersList
        table={table}
        isHost={isHost}
        busyKey={busyKey}
        onRemovePlayer={onRemovePlayer}
        onDragPlayerStart={onDragPlayerStart}
        onDragPlayerEnd={onDragPlayerEnd}
      />

      <GameTableActions
        isFull={isFull}
        isJoining={isJoining}
        alreadyInGame={alreadyInGame}
        isApproval={isApproval}
        isAssignedToTable={isAssignedToTable}
        onJoin={() => onJoin(game.id, table.id, game.joinMode, systemKey)}
        onToggleProposal={() =>
          onOpenProposalTableIdChange(openProposalTableId === table.id ? null : table.id)
        }
      />

      {openProposalTableId === table.id && (
        <GameProposalForm
          proposalStartTime={proposalStartTime}
          proposalSystems={proposalSystems}
          proposalPoints={proposalPoints}
          proposalMessage={proposalMessage}
          isBusy={busyKey === `proposal-submit-${table.id}`}
          onProposalStartTimeChange={onProposalStartTimeChange}
          onProposalSystemsChange={onProposalSystemsChange}
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