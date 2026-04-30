import type { GameResponse } from "../types/game";
import GameCard from "./GameCard";

type Props = {
  games: GameResponse[];
  joiningKey: string | null;
  onJoin: (gameId: string, tableId: string, systemKey?: string) => void;
};

export default function GameList({ games, joiningKey, onJoin }: Props) {
  if (games.length === 0) return <p>Keine Spiele gefunden.</p>;

  return (
    <>
      {games.map((game) => (
        <GameCard
          key={game.id}
          game={game}
          joiningKey={joiningKey}
          onJoin={onJoin}
        />
      ))}
    </>
  );
}