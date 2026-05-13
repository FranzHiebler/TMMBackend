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
} from "../api/gamesApi";
import type { CreateChangeProposalRequest, GameResponse, GameTableDto } from "../types/game";
import type { User } from "../context/UserContext";

type MessageState = {
  type: "success" | "error";
  text: string;
} | null;

type Options = {
  game: GameResponse;
  user: User;
  onGameUpdated?: (game: GameResponse) => void;
};

export function useGameCardActions({ game, user, onGameUpdated }: Options) {
  const [busyKey, setBusyKey] = useState<string | null>(null);
  const [message, setMessage] = useState<MessageState>(null);
  const [draggedPlayerId, setDraggedPlayerId] = useState<string | null>(null);

  async function refreshGame() {
    const updated = await getGameById(game.id);
    onGameUpdated?.(updated);
  }

  async function submitProposal(
    table: GameTableDto,
    request: CreateChangeProposalRequest
  ): Promise<boolean> {
    try {
      setBusyKey(`proposal-submit-${table.id}`);
      setMessage(null);

      const updated = await createChangeProposal(game.id, request, user);
      onGameUpdated?.(updated);

      setMessage({ type: "success", text: "Vorschlag gesendet" });
      return true;
    } catch (err) {
      setMessage({
        type: "error",
        text: err instanceof Error ? err.message : "Vorschlag konnte nicht gesendet werden",
      });
      return false;
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

  return {
    busyKey,
    message,
    draggedPlayerId,
    setDraggedPlayerId,
    submitProposal,
    resolveProposal,
    acceptApplication,
    declineApplication,
    removeAssignedPlayer,
    moveAssignedPlayer,
  };
}