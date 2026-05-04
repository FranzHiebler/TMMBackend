import type { GameJoinMode, GameResponse } from "../types/game";
import GameCard from "./GameCard";

type Props = {
  games: GameResponse[];
  joiningKey: string | null;
  currentUserId: string;
  messageByKey: Record<string, string>;
  onJoin: (gameId: string, tableId: string, joinMode: GameJoinMode, systemKey?: string) => void;
};

export default function GameList({ games, joiningKey, currentUserId, messageByKey, onJoin }: Props) {
  if (games.length === 0) return <p>Keine Spiele gefunden.</p>;

  return (
    <>
      {games.map((game) => (
        <GameCard
          key={game.id}
          game={game}
          joiningKey={joiningKey}
          currentUserId={currentUserId}
          messageByKey={messageByKey}
          onJoin={onJoin}
        />
      ))}
    </>
  );
}