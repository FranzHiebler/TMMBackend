import { useState } from "react";
import {
  acceptChangeProposal,
  assignApplicationToTable,
  createChangeProposal,
  getGameById,
  movePlayerToTable,
  rejectApplication,
  rejectChangeProposal,
  removePlayerFromTable,
} from "../api/gamesService";
import { useUser } from "../context/UserContext";
import {
  type GameChangeProposalDto,
  type GameResponse,
  type GameTableDto,
  GameJoinMode,
} from "../types/game";
import GameCardHeader from "./GameCardHeader";
import GameTableCard from "./GameTableCard";
import ChangeProposalsList from "./ChangeProposalsList";

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

  const [busyKey, setBusyKey] = useState<string | null>(null);
  const [message, setMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);
  const [draggedPlayerId, setDraggedPlayerId] = useState<string | null>(null);

  const isApproval = game.joinMode === GameJoinMode.ApprovalRequired;
  const isHost = game.host?.userId === currentUserId;

  const alreadyInGame = game.tables.some((t) =>
    t.assignedPlayers.some((p) => p.userId === currentUserId)
  );

  const pendingProposals = (game.changeProposals ?? []).filter((p) => p.status === "Pending");

  async function refreshGame() {
    const updated = await getGameById(game.id);
    onGameUpdated?.(updated);
  }

  function resetProposalForm() {
    setProposalStartTime("");
    setProposalSystems("");
    setProposalPoints("");
    setProposalMessage("");
  }

  async function submitProposal(table: GameTableDto) {
    const systems = proposalSystems
      .split(",")
      .map((x) => x.trim())
      .filter(Boolean);

    const request = {
      tableId: table.id,
      proposedStartTimeUtc: proposalStartTime ? new Date(proposalStartTime).toISOString() : null,
      proposedSystems: systems.length ? systems : null,
      proposedPoints: proposalPoints ? Number(proposalPoints) : null,
      message: proposalMessage.trim() || null,
    };

    try {
      setBusyKey(`proposal-submit-${table.id}`);
      setMessage(null);

      const updated = await createChangeProposal(game.id, request, user);
      onGameUpdated?.(updated);

      setOpenProposalTableId(null);
      resetProposalForm();
      setMessage({ type: "success", text: "Vorschlag gesendet" });
    } catch (err) {
      setMessage({
        type: "error",
        text: err instanceof Error ? err.message : "Vorschlag konnte nicht gesendet werden",
      });
    } finally {
      setBusyKey(null);
    }
  }

  async function resolveProposal(proposalId: string, action: "accept" | "reject") {
    try {
      setBusyKey(`proposal-${action}-${proposalId}`);
      setMessage(null);

      const updated =
        action === "accept"
          ? await acceptChangeProposal(game.id, proposalId, user)
          : await rejectChangeProposal(game.id, proposalId, user);

      onGameUpdated?.(updated);
      setMessage({
        type: "success",
        text: action === "accept" ? "Vorschlag angenommen" : "Vorschlag abgelehnt",
      });
    } catch (err) {
      setMessage({
        type: "error",
        text: err instanceof Error ? err.message : "Vorschlag konnte nicht bearbeitet werden",
      });
    } finally {
      setBusyKey(null);
    }
  }

  async function acceptApplication(tableId: string, applicationId: string) {
    try {
      setBusyKey(`application-accept-${applicationId}`);
      setMessage(null);

      await assignApplicationToTable(game.id, tableId, applicationId, user);
      await refreshGame();

      setMessage({ type: "success", text: "Bewerbung angenommen" });
    } catch (err) {
      setMessage({
        type: "error",
        text: err instanceof Error ? err.message : "Bewerbung konnte nicht angenommen werden",
      });
    } finally {
      setBusyKey(null);
    }
  }

  async function declineApplication(applicationId: string) {
    try {
      setBusyKey(`application-reject-${applicationId}`);
      setMessage(null);

      await rejectApplication(game.id, applicationId, user);
      await refreshGame();

      setMessage({ type: "success", text: "Bewerbung abgelehnt" });
    } catch (err) {
      setMessage({
        type: "error",
        text: err instanceof Error ? err.message : "Bewerbung konnte nicht abgelehnt werden",
      });
    } finally {
      setBusyKey(null);
    }
  }

  async function removeAssignedPlayer(tableId: string, userId: string) {
    try {
      setBusyKey(`player-remove-${userId}`);
      setMessage(null);

      await removePlayerFromTable(game.id, tableId, userId, user);
      await refreshGame();

      setMessage({ type: "success", text: "Spieler entfernt" });
    } catch (err) {
      setMessage({
        type: "error",
        text: err instanceof Error ? err.message : "Spieler konnte nicht entfernt werden",
      });
    } finally {
      setBusyKey(null);
    }
  }

  async function moveAssignedPlayer(targetTableId: string) {
    if (!draggedPlayerId) return;

    try {
      setBusyKey(`player-move-${draggedPlayerId}`);
      setMessage(null);

      await movePlayerToTable(game.id, draggedPlayerId, targetTableId, user);
      await refreshGame();
    } catch (err) {
      setMessage({
        type: "error",
        text: err instanceof Error ? err.message : "Spieler konnte nicht verschoben werden",
      });
    } finally {
      setBusyKey(null);
      setDraggedPlayerId(null);
    }
  }

  return (
    <div className="card">
      <GameCardHeader game={game} isApproval={isApproval} />

      {message && (
        <div className={`message message-${message.type}`}>
          {message.text}
        </div>
      )}

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
            onSubmitProposal={submitProposal}
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