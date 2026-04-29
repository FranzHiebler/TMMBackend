import { getAllGames } from "../api/gamesService";
import GameFeedPage from "./GameFeedPage";

export default function NearbyPage() {
  return (
    <GameFeedPage
      title="Spiele in der Nähe"
      loadGamesFn={() => getAllGames()}
    />
  );
}