import { Link, Route, Routes } from "react-router-dom";
import GamesPage from "./pages/GamesPage";
import NearbyPage from "./pages/NearbyPage";
import LocationsPage from "./pages/LocationPage";
import CreateGamePage from "./pages/CreateGamePage";
import MyGamesPage from "./pages/MyGamesPage";
import ProfilePage from "./pages/ProfilePage";
import UserSwitcher from "./components/UserSwitcher";

export default function App() {
  return (
    <>
      <nav className="nav">
        <Link to="/games">Games</Link>
        <Link to="/my-games">Meine Spiele</Link>
        <Link to="/nearby">Nearby</Link>
        <Link to="/locations">Locations</Link>
        <Link to="/games/create">Game erstellen</Link>
        <Link to="/profile">Profil</Link>
        <UserSwitcher />
      </nav>

      <Routes>
        <Route path="/" element={<GamesPage />} />
        <Route path="/games" element={<GamesPage />} />
        <Route path="/my-games" element={<MyGamesPage />} />
        <Route path="/nearby" element={<NearbyPage />} />
        <Route path="/locations" element={<LocationsPage />} />
        <Route path="/games/create" element={<CreateGamePage />} />
        <Route path="/profile" element={<ProfilePage />} />
      </Routes>
    </>
  );
}