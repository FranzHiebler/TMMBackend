import { Link, Route, Routes } from "react-router-dom";
import GamesPage from "./pages/GamesPage";
import NearbyPage from "./pages/NearbyPage";
import LocationsPage from "./pages/LocationPage";

export default function App() {
  return (
    <>
      <nav className="nav">
        <Link to="/games">Games</Link>
        <Link to="/nearby">Nearby</Link>
        <Link to="/locations">Locations</Link>
        <Link to="/feed">Feed</Link>
      </nav>

      <Routes>
        <Route path="/" element={<GamesPage />} />

        <Route path="/games" element={<GamesPage />} />
        <Route path="/nearby" element={<NearbyPage />} />
        <Route path="/locations" element={<LocationsPage />} />

        
      </Routes>
    </>
  );
}