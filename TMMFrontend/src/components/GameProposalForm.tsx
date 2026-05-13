type Props = {
  proposalStartTime: string;
  proposalSystems: string;
  proposalPoints: string;
  proposalMessage: string;
  isBusy: boolean;
  onProposalStartTimeChange: (value: string) => void;
  onProposalSystemsChange: (value: string) => void;
  onProposalPointsChange: (value: string) => void;
  onProposalMessageChange: (value: string) => void;
  onSubmit: () => void;
};

export default function GameProposalForm({
  proposalStartTime,
  proposalSystems,
  proposalPoints,
  proposalMessage,
  isBusy,
  onProposalStartTimeChange,
  onProposalSystemsChange,
  onProposalPointsChange,
  onProposalMessageChange,
  onSubmit,
}: Props) {
  return (
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

      <button type="button" disabled={isBusy} onClick={onSubmit}>
        Vorschlag senden
      </button>
    </div>
  );
}