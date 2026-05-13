import { useState } from "react";
import { useUser } from "../context/UserContext";
import {
  type GameChangeProposalDto,
  type GameResponse,
  type GameTableDto,
  GameJoinMode,
} from "../types/game";
import { useGameCardActions } from "../hooks/useGameCardActions";
import GameCardHeader from "./GameCardHeader";
import GameTableCard from "./GameTableCard";
import ChangeProposalsList from "./ChangeProposalsList";
import Message from "./Message";

type Props = {
  game: GameResponse;
  joiningKey: string | null;
  messageByKey: Record<string, string>;
  currentUserId: string;
  onJoin: (gameId: string, tableId: string, joinMode: GameJoinMode, systemKey?: string) => void;
  onGameUpdated?: (game: GameResponse) => void;
};

export default function GameCard({
  game,
  joiningKey,
  currentUserId,
  messageByKey,
  onJoin,
  onGameUpdated,
}: Props) {
  const user = useUser();

  const [openProposalTableId, setOpenProposalTableId] = useState<string | null>(null);
  const [proposalStartTime, setProposalStartTime] = useState("");
  const [proposalSystems, setProposalSystems] = useState("");
  const [proposalPoints, setProposalPoints] = useState("");
  const [proposalMessage, setProposalMessage] = useState("");

  const {
    busyKey,
    message,
    setDraggedPlayerId,
    submitProposal,
    resolveProposal,
    acceptApplication,
    declineApplication,
    removeAssignedPlayer,
    moveAssignedPlayer,
  } = useGameCardActions({ game, user, onGameUpdated });

  const isApproval = game.joinMode === GameJoinMode.ApprovalRequired;
  const isHost = game.host?.userId === currentUserId;

  const alreadyInGame = game.tables.some((t) =>
    t.assignedPlayers.some((p) => p.userId === currentUserId)
  );

  const pendingProposals = (game.changeProposals ?? []).filter((p) => p.status === "Pending");

  function resetProposalForm() {
    setProposalStartTime("");
    setProposalSystems("");
    setProposalPoints("");
    setProposalMessage("");
  }

  async function handleSubmitProposal(table: GameTableDto) {
    const systems = proposalSystems
      .split(",")
      .map((x) => x.trim())
      .filter(Boolean);

    const success = await submitProposal(table, {
      tableId: table.id,
      proposedStartTimeUtc: proposalStartTime ? new Date(proposalStartTime).toISOString() : null,
      proposedSystems: systems.length ? systems : null,
      proposedPoints: proposalPoints ? Number(proposalPoints) : null,
      message: proposalMessage.trim() || null,
    });

    if (success) {
      setOpenProposalTableId(null);
      resetProposalForm();
    }
  }

  return (
    <div className="card">
      <GameCardHeader game={game} isApproval={isApproval} />

      <Message text={message?.text} type={message?.type} />

      <div style={{ marginTop: 12 }}>
        {game.tables.map((table) => (
          <GameTableCard
            key={table.id}
            game={game}
            table={table}
            isHost={isHost}
            isApproval={isApproval}
            alreadyInGame={alreadyInGame}
            joiningKey={joiningKey}
            messageByKey={messageByKey}
            currentUserId={currentUserId}
            busyKey={busyKey}
            openProposalTableId={openProposalTableId}
            proposalStartTime={proposalStartTime}
            proposalSystems={proposalSystems}
            proposalPoints={proposalPoints}
            proposalMessage={proposalMessage}
            onJoin={onJoin}
            onOpenProposalTableIdChange={setOpenProposalTableId}
            onProposalStartTimeChange={setProposalStartTime}
            onProposalSystemsChange={setProposalSystems}
            onProposalPointsChange={setProposalPoints}
            onProposalMessageChange={setProposalMessage}
            onSubmitProposal={handleSubmitProposal}
            onAcceptApplication={acceptApplication}
            onRejectApplication={declineApplication}
            onRemovePlayer={removeAssignedPlayer}
            onDragPlayerStart={setDraggedPlayerId}
            onDragPlayerEnd={() => setDraggedPlayerId(null)}
            onDropPlayer={moveAssignedPlayer}
          />
        ))}
      </div>

      <ChangeProposalsList
        proposals={pendingProposals as GameChangeProposalDto[]}
        tables={game.tables}
        isHost={isHost}
        busyKey={busyKey}
        onResolveProposal={resolveProposal}
      />
    </div>
  );
}