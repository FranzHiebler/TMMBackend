import { NavLink, Route, Routes } from "react-router-dom";
import GamesPage from "./pages/GamesPage";
import NearbyPage from "./pages/NearbyPage";
import LocationsPage from "./pages/LocationPage";
import CreateGamePage from "./pages/CreateGamePage";
import MyGamesPage from "./pages/MyGamesPage";
import ProfilePage from "./pages/ProfilePage";
import UserSwitcher from "./components/UserSwitcher";

function navClass({ isActive }: { isActive: boolean }) {
  return isActive ? "nav-link active" : "nav-link";
}

export default function App() {
  return (
    <>
      <nav className="app-nav">
        <div className="nav-brand">
          <span className="nav-brand-mark">TMM</span>
          <span className="nav-brand-text">Tabletop Matchmaker</span>
        </div>

        <div className="nav-links">
          <NavLink to="/nearby" className={navClass}>
            Entdecken
          </NavLink>

          <NavLink to="/games" className={navClass}>
            Spiele
          </NavLink>

          <NavLink to="/my-games" className={navClass}>
            Meine Spiele
          </NavLink>

          <NavLink to="/locations" className={navClass}>
            Locations
          </NavLink>
        </div>

        <div className="nav-actions">
          <NavLink to="/games/create" className="nav-create-button">
            + Spiel erstellen
          </NavLink>

          <NavLink to="/profile" className={navClass}>
            Profil
          </NavLink>

          <UserSwitcher />
        </div>
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