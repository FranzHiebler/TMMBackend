import { Link, Route, Routes } from "react-router-dom";
import GamesPage from "./pages/GamesPage";
import NearbyPage from "./pages/NearbyPage";
import LocationsPage from "./pages/LocationPage";
import CreateGamePage from "./pages/CreateGamePage";

export default function App() {
  return (
    <>
      <nav className="nav">
        <Link to="/games">Games</Link>
        <Link to="/nearby">Nearby</Link>
        <Link to="/locations">Locations</Link>
        <Link to="/games/create">Game erstellen</Link>
      </nav>

      <Routes>
        <Route path="/" element={<GamesPage />} />
        <Route path="/games" element={<GamesPage />} />
        <Route path="/nearby" element={<NearbyPage />} />
        <Route path="/locations" element={<LocationsPage />} />
        <Route path="/games/create" element={<CreateGamePage />} />
      </Routes>
    </>
  );
}