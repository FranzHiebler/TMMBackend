import { useEffect, useState } from "react";
import {
  acceptChangeProposal,
  createChangeProposal,
  rejectChangeProposal,
} from "../api/gamesService";
import { useUser } from "../context/UserContext";
import {
  type GameChangeProposalDto,
  type GameResponse,
  type GameTableDto,
  GameJoinMode,
} from "../types/game";

type Props = {
  game: GameResponse;
  joiningKey: string | null;
  messageByKey: Record<string, string>;
  currentUserId: string;
  onJoin: (gameId: string, tableId: string, joinMode: GameJoinMode, systemKey?: string) => void;
  onGameUpdated?: (game: GameResponse) => void;
};

function systemText(table: GameTableDto) {
  if (!table.systems || table.systems.length === 0) return "Egal";
  if (table.systems.some((x) => x.toLowerCase() === "egal")) return "Egal";
  return table.systems.join(", ");
}

function proposalSummary(proposal: GameChangeProposalDto, table?: GameTableDto) {
  const parts: string[] = [];

  if (proposal.proposedStartTimeUtc) {
    parts.push(`Uhrzeit: ${new Date(proposal.proposedStartTimeUtc).toLocaleString("de-DE")}`);
  }

  if (proposal.proposedSystems?.length) {
    parts.push(`System: ${proposal.proposedSystems.join(", ")}`);
  }

  if (proposal.proposedPoints != null) {
    parts.push(`Punkte: ${proposal.proposedPoints}`);
  }

  const tablePrefix = table ? `${table.name}: ` : "";
  return `${tablePrefix}${parts.join(" | ")}`;
}

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
  const [proposalBusyKey, setProposalBusyKey] = useState<string | null>(null);
  const [proposalError, setProposalError] = useState("");
  const [proposalSuccess, setProposalSuccess] = useState("");

  const isApproval = game.joinMode === GameJoinMode.ApprovalRequired;
  const alreadyInGame = game.tables.some((t) =>
    t.assignedPlayers.some((p) => p.userId === currentUserId)
  );
  const isHost = game.host?.userId === currentUserId;
  const pendingProposals = (game.changeProposals ?? []).filter((p) => p.status === "Pending");

  useEffect(() => {
    if (!proposalError && !proposalSuccess) return;

    const timeout = window.setTimeout(() => {
      setProposalError("");
      setProposalSuccess("");
    }, 4500);

    return () => window.clearTimeout(timeout);
  }, [proposalError, proposalSuccess]);

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
      setProposalBusyKey(`submit-${table.id}`);
      setProposalError("");
      setProposalSuccess("");
      const updated = await createChangeProposal(game.id, request, user);
      onGameUpdated?.(updated);
      setOpenProposalTableId(null);
      resetProposalForm();
      setProposalSuccess("Vorschlag gesendet");
    } catch (err) {
      setProposalError(err instanceof Error ? err.message : "Vorschlag konnte nicht gesendet werden");
    } finally {
      setProposalBusyKey(null);
    }
  }

  async function resolveProposal(proposalId: string, action: "accept" | "reject") {
    try {
      setProposalBusyKey(`${action}-${proposalId}`);
      setProposalError("");
      setProposalSuccess("");
      const updated =
        action === "accept"
          ? await acceptChangeProposal(game.id, proposalId, user)
          : await rejectChangeProposal(game.id, proposalId, user);
      onGameUpdated?.(updated);
      setProposalSuccess(action === "accept" ? "Vorschlag angenommen" : "Vorschlag abgelehnt");
    } catch (err) {
      setProposalError(err instanceof Error ? err.message : "Vorschlag konnte nicht bearbeitet werden");
    } finally {
      setProposalBusyKey(null);
    }
  }

  return (
    <div className="card">
      <h3>{game.title}</h3>

      <div><b>Host:</b> {game.host?.displayName}</div>
      <div><b>Ort:</b> {game.location?.name}, {game.location?.city}</div>
      <div><b>Start:</b> {new Date(game.startTimeUtc).toLocaleString("de-DE")}</div>
      <div><b>Modus:</b> {isApproval ? "Bewerbung erforderlich" : "Direkt beitreten"}</div>
      <div><b>Plätze:</b> {game.assignedPlayers}/{game.maxPlayers} | Frei: {game.openSlots}</div>

      {game.description && <p>{game.description}</p>}
      {proposalSuccess && <div className="message message-success">{proposalSuccess}</div>}
      {proposalError && <div className="message message-error">{proposalError}</div>}

      <div style={{ marginTop: 12 }}>
        {game.tables.map((table) => {
          const key = `${game.id}_${table.id}`;
          const isFull = table.openSlots <= 0;
          const isJoining = joiningKey === key;
          const isAssignedToTable = table.assignedPlayers.some((p) => p.userId === currentUserId);
          const systemKey =
            !table.systems.length || table.systems.some((x) => x.toLowerCase() === "egal")
              ? undefined
              : table.systems[0];

          return (
            <div key={table.id} className="card" style={{ marginTop: 10 }}>
              <h4>{table.name}</h4>

              <div><b>System:</b> {systemText(table)}</div>
              <div><b>Spieler:</b> {table.assignedPlayers.length}/{table.maxPlayers}</div>
              {table.startTimeUtc && (
                <div><b>Start am Tisch:</b> {new Date(table.startTimeUtc).toLocaleString("de-DE")}</div>
              )}

              {table.points && <div><b>Punkte:</b> {table.points}</div>}
              {table.scenario && <div><b>Szenario:</b> {table.scenario}</div>}
              {table.notes && <div><b>Notizen:</b> {table.notes}</div>}

              {table.assignedPlayers.length > 0 && (
                <div>
                  <b>Zugewiesen:</b>{" "}
                  {table.assignedPlayers.map((p) => p.displayName).join(", ")}
                </div>
              )}

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
                    onClick={() => {
                      setOpenProposalTableId(openProposalTableId === table.id ? null : table.id);
                      setProposalError("");
                      setProposalSuccess("");
                    }}
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
                    onChange={(e) => setProposalStartTime(e.target.value)}
                  />
                  <input
                    value={proposalSystems}
                    onChange={(e) => setProposalSystems(e.target.value)}
                    placeholder="Systeme, z.B. tow, wh40k"
                  />
                  <input
                    type="number"
                    min={0}
                    value={proposalPoints}
                    onChange={(e) => setProposalPoints(e.target.value)}
                    placeholder="Punkte"
                  />
                  <textarea
                    value={proposalMessage}
                    onChange={(e) => setProposalMessage(e.target.value)}
                    placeholder="Nachricht optional"
                  />
                  <button
                    type="button"
                    disabled={proposalBusyKey === `submit-${table.id}`}
                    onClick={() => submitProposal(table)}
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
            </div>
          );
        })}
      </div>

      {pendingProposals.length > 0 && (
        <div className="proposal-list">
          <h4>Offene Änderungsvorschläge</h4>
          {pendingProposals.map((proposal) => {
            const table = game.tables.find((t) => t.id === proposal.tableId);
            return (
              <div key={proposal.id} className="proposal-row">
                <div>
                  <b>{proposal.proposedBy.displayName}</b>{" "}
                  <span>{proposalSummary(proposal, table)}</span>
                  {proposal.message && <p>{proposal.message}</p>}
                </div>
                {isHost && (
                  <div className="proposal-actions">
                    <button
                      type="button"
                      disabled={proposalBusyKey === `accept-${proposal.id}`}
                      onClick={() => resolveProposal(proposal.id, "accept")}
                    >
                      Annehmen
                    </button>
                    <button
                      type="button"
                      disabled={proposalBusyKey === `reject-${proposal.id}`}
                      onClick={() => resolveProposal(proposal.id, "reject")}
                    >
                      Ablehnen
                    </button>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
