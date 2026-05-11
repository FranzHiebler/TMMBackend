import type { GameTableDto, TableApplicationDto } from "../types/game";

type Props = {
  table: GameTableDto;
  isHost: boolean;
  pendingApplications: TableApplicationDto[];
  busyKey: string | null;
  onAcceptApplication: (tableId: string, applicationId: string) => void;
  onRejectApplication: (applicationId: string) => void;
};

export default function ApplicationsList({
  table,
  isHost,
  pendingApplications,
  busyKey,
  onAcceptApplication,
  onRejectApplication,
}: Props) {
  if (!isHost || pendingApplications.length === 0) return null;

  return (
    <div className="proposal-list">
      <h4>Bewerbungen</h4>

      {pendingApplications.map((application) => (
        <div key={application.id} className="proposal-row">
          <div>
            <b>{application.player.displayName}</b>
            {application.systemKey && <span> · System: {application.systemKey}</span>}
            {application.message && <p>{application.message}</p>}
          </div>

          <div className="proposal-actions">
            <button
              type="button"
              disabled={busyKey === `application-accept-${application.id}`}
              onClick={() => onAcceptApplication(table.id, application.id)}
            >
              Annehmen
            </button>

            <button
              type="button"
              disabled={busyKey === `application-reject-${application.id}`}
              onClick={() => onRejectApplication(application.id)}
            >
              Ablehnen
            </button>
          </div>
        </div>
      ))}
    </div>
  );
}