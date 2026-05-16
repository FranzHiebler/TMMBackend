import { useCallback, useEffect, useMemo, useState } from "react";
import L from "leaflet";
import "leaflet/dist/leaflet.css";
import { Link, useNavigate } from "react-router-dom";
import { MapContainer, Marker, Popup, TileLayer } from "react-leaflet";
import { getDiscoveryGames, getGameById } from "../api/gamesApi";
import { getDiscoveryLocations } from "../api/locationsApi";
import { useJoinGame } from "../api/useJoinGame";
import { useUser } from "../context/UserContext";
import type { GameDiscoveryResponse, LocationDiscoveryResponse } from "../types/game";

type RangePreset = "today" | "7" | "30";
type Selection =
  | { type: "game"; id: string }
  | { type: "location"; id: string }
  | null;

const DEFAULT_CENTER: [number, number] = [50.5558, 9.6808];

function startOfToday(offsetDays = 0) {
  const date = new Date();
  date.setHours(0, 0, 0, 0);
  date.setDate(date.getDate() + offsetDays);
  return date;
}

function endOfDay(date: Date) {
  const next = new Date(date);
  next.setHours(23, 59, 59, 999);
  return next;
}

function rangeToDates(range: RangePreset, dayOffset: number) {
  const from = startOfToday(dayOffset);
  const to = new Date(from);

  if (range === "today") return { from, to: endOfDay(from) };

  to.setDate(to.getDate() + (range === "7" ? 7 : 30));
  to.setHours(23, 59, 59, 999);
  return { from, to };
}

function timeHint(startTimeUtc: string) {
  const start = new Date(startTimeUtc);
  const today = startOfToday();
  const startDay = new Date(start);
  startDay.setHours(0, 0, 0, 0);
  const diffDays = Math.round((startDay.getTime() - today.getTime()) / 86400000);

  if (diffDays === 0) return "Heute";
  if (diffDays === 1) return "Morgen";
  if (diffDays > 1 && diffDays < 7) return start.toLocaleDateString("de-DE", { weekday: "short" });
  return `in ${diffDays} Tagen`;
}

function gameMarkerState(game: GameDiscoveryResponse) {
  if (game.isHost) return "host";
  if (game.isParticipant) return "participant";
  if (game.isOwnLocation) return "own-location";
  return "event";
}

function gameMarkerIcon(game: GameDiscoveryResponse, indexAtLocation: number) {
  const state = gameMarkerState(game);
  const offset = Math.min(indexAtLocation, 3) * 8;
  return L.divIcon({
    className: "",
    html: `<div class="discovery-marker discovery-marker-${state}" style="transform: translate(${offset}px, -${offset}px)"><span>${timeHint(game.startTimeUtc)}</span></div>`,
    iconSize: [78, 38],
    iconAnchor: [39, 19],
  });
}

function locationMarkerIcon(location: LocationDiscoveryResponse) {
  const state = location.isOwnLocation ? "own-location-base" : "location";
  return L.divIcon({
    className: "",
    html: `<div class="location-marker location-marker-${state}"><span>${location.upcomingGameCount || ""}</span></div>`,
    iconSize: [30, 30],
    iconAnchor: [15, 15],
  });
}

function statusText(game: GameDiscoveryResponse) {
  if (game.isHost) return "Du hostest";
  if (game.isParticipant) return "Du nimmst teil";
  if (game.applicationStatus) return `Bewerbung: ${game.applicationStatus}`;
  if (game.isOwnLocation) return "Eigene Location";
  return "GameSession";
}

export default function MapDiscoveryPage() {
  const user = useUser();
  const navigate = useNavigate();
  const [range, setRange] = useState<RangePreset>("7");
  const [dayOffset, setDayOffset] = useState(0);
  const [games, setGames] = useState<GameDiscoveryResponse[]>([]);
  const [locations, setLocations] = useState<LocationDiscoveryResponse[]>([]);
  const [selection, setSelection] = useState<Selection>(null);
  const [loadingGames, setLoadingGames] = useState(true);
  const [loadingLocations, setLoadingLocations] = useState(true);
  const [banner, setBanner] = useState("");
  const [center] = useState<[number, number]>(DEFAULT_CENTER);
  const [radiusKm, setRadiusKm] = useState(80);

  const { from, to } = useMemo(() => rangeToDates(range, dayOffset), [range, dayOffset]);

  const loadDiscovery = useCallback(async () => {
    setBanner("");
    setLoadingGames(true);
    setLoadingLocations(true);

    try {
      const [locationData, gameData] = await Promise.all([
        getDiscoveryLocations({ latitude: center[0], longitude: center[1], radiusKm }, user),
        getDiscoveryGames({
          fromUtc: from.toISOString(),
          toUtc: to.toISOString(),
          latitude: center[0],
          longitude: center[1],
          radiusKm,
        }, user),
      ]);

      setLocations(locationData);
      setGames(gameData);
      setSelection((current) => {
        if (current?.type === "game" && gameData.some((game) => game.gameId === current.id)) return current;
        if (current?.type === "location" && locationData.some((location) => location.locationId === current.id)) return current;
        if (gameData[0]) return { type: "game", id: gameData[0].gameId };
        if (locationData[0]) return { type: "location", id: locationData[0].locationId };
        return null;
      });
    } catch (err) {
      setBanner(err instanceof Error ? err.message : "Discovery konnte nicht geladen werden.");
    } finally {
      setLoadingGames(false);
      setLoadingLocations(false);
    }
  }, [center, from, radiusKm, to, user]);

  const { join, joiningKey } = useJoinGame({
    onGameUpdated: () => void loadDiscovery(),
  });

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void loadDiscovery();
  }, [loadDiscovery]);

  const gamesByLocation = useMemo(() => {
    const counts = new Map<string, number>();
    return games.map((game) => {
      const count = counts.get(game.locationId) ?? 0;
      counts.set(game.locationId, count + 1);
      return { game, indexAtLocation: count };
    });
  }, [games]);

  const selectedGame =
    selection?.type === "game" ? games.find((game) => game.gameId === selection.id) ?? null : null;
  const selectedLocation =
    selection?.type === "location"
      ? locations.find((location) => location.locationId === selection.id) ?? null
      : null;

  const isLoading = loadingGames || loadingLocations;

  async function quickJoin(game: GameDiscoveryResponse) {
    const fullGame = await getGameById(game.gameId);
    const table = fullGame.tables.find((candidate) => candidate.openSlots > 0);
    if (!table) return;
    const systemKey = table.systems.length === 0 || table.systems.some((system) => system.toLowerCase() === "egal")
      ? undefined
      : table.systems[0];
    await join(fullGame.id, table.id, fullGame.joinMode, systemKey);
  }

  function createAtLocation(locationId: string) {
    navigate(`/games/create?locationId=${encodeURIComponent(locationId)}`);
  }

  return (
    <div className="discovery-page">
      <section className="discovery-map-shell">
        <MapContainer center={center} zoom={10} className="discovery-map">
          <TileLayer
            attribution="&copy; OpenStreetMap"
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />

          {locations
            .filter((location) => location.latitude != null && location.longitude != null)
            .map((location) => (
              <Marker
                key={location.locationId}
                position={[location.latitude!, location.longitude!]}
                icon={locationMarkerIcon(location)}
                zIndexOffset={location.isOwnLocation ? 180 : 80}
                eventHandlers={{ click: () => setSelection({ type: "location", id: location.locationId }) }}
              >
                <Popup>{location.name}</Popup>
              </Marker>
            ))}

          {gamesByLocation
            .filter(({ game }) => game.latitude != null && game.longitude != null)
            .map(({ game, indexAtLocation }) => (
              <Marker
                key={game.gameId}
                position={[game.latitude!, game.longitude!]}
                icon={gameMarkerIcon(game, indexAtLocation)}
                zIndexOffset={600 + indexAtLocation}
                eventHandlers={{ click: () => setSelection({ type: "game", id: game.gameId }) }}
              >
                <Popup>{game.title}</Popup>
              </Marker>
            ))}
        </MapContainer>

        <aside className="discovery-panel">
          <div>
            <p className="panel-kicker">Tabletop Matchmaker</p>
            <h1>Entdecke Locations und Spielrunden</h1>
          </div>

          <div className="discovery-segments">
            <button type="button" className={range === "today" ? "active" : ""} onClick={() => setRange("today")}>Heute</button>
            <button type="button" className={range === "7" ? "active" : ""} onClick={() => setRange("7")}>7 Tage</button>
            <button type="button" className={range === "30" ? "active" : ""} onClick={() => setRange("30")}>30 Tage</button>
          </div>

          <label className="day-slider">
            <span>Starttag: {dayOffset === 0 ? "heute" : `in ${dayOffset} Tagen`}</span>
            <input type="range" min={0} max={30} value={dayOffset} onChange={(event) => setDayOffset(Number(event.target.value))} />
          </label>

          <label className="day-slider">
            <span>Radius: {radiusKm} km</span>
            <input type="range" min={10} max={250} step={10} value={radiusKm} onChange={(event) => setRadiusKm(Number(event.target.value))} />
          </label>

          <div className="discovery-legend">
            <span><i className="legend-dot location" /> Location</span>
            <span><i className="legend-dot own-location-base" /> Eigene Location</span>
            <span><i className="legend-dot event" /> GameSession</span>
            <span><i className="legend-dot participant" /> Teilnahme</span>
            <span><i className="legend-dot host" /> Host</span>
          </div>

          {banner && <div className="message message-error">{banner}</div>}
          {isLoading && <div className="discovery-skeleton" />}
          {!isLoading && !banner && (
            <p>{locations.length} Locations · {games.length} Sessions im Zeitraum</p>
          )}
        </aside>

        {!isLoading && !banner && games.length === 0 && locations.length > 0 && (
          <div className="discovery-empty-state">
            Keine kommenden Sessions im Zeitraum. Hier sind Locations in deiner Umgebung.
          </div>
        )}

        {!isLoading && !banner && games.length === 0 && locations.length === 0 && (
          <div className="discovery-empty-state">
            <b>Keine Locations oder Sessions gefunden.</b>
            <div className="preview-actions">
              <Link to="/locations">Location erstellen</Link>
              <button type="button" onClick={() => setRadiusKm((value) => Math.min(value + 50, 250))}>Radius erhöhen</button>
              <button type="button" onClick={() => setRange("30")}>Zeitraum erhöhen</button>
            </div>
          </div>
        )}

        {selectedGame && (
          <article className="session-preview">
            <button className="preview-close" type="button" onClick={() => setSelection(null)}>×</button>
            <p className="panel-kicker">{statusText(selectedGame)}</p>
            <h2>{selectedGame.title}</h2>
            <p>{new Date(selectedGame.startTimeUtc).toLocaleString("de-DE")}</p>
            <p>{selectedGame.locationName}, {selectedGame.city}</p>
            <p>{selectedGame.tablesSummary}</p>
            <p>{selectedGame.availableSeats} freie Plätze · {selectedGame.status}</p>
            <div className="preview-actions">
              <Link to="/games">Details</Link>
              {!selectedGame.isHost && !selectedGame.isParticipant && (
                <button type="button" disabled={joiningKey?.startsWith(selectedGame.gameId)} onClick={() => quickJoin(selectedGame)}>
                  {selectedGame.joinMode === "ApprovalRequired" ? "Bewerben" : "Beitreten"}
                </button>
              )}
              {selectedGame.canEdit && <Link to="/games">Bearbeiten</Link>}
            </div>
          </article>
        )}

        {selectedLocation && (
          <article className="session-preview location-preview">
            <button className="preview-close" type="button" onClick={() => setSelection(null)}>×</button>
            <p className="panel-kicker">{selectedLocation.isOwnLocation ? `Eigene Location${selectedLocation.role ? ` · ${selectedLocation.role}` : ""}` : "Location"}</p>
            <h2>{selectedLocation.name}</h2>
            <p>{selectedLocation.city}{selectedLocation.address ? `, ${selectedLocation.address}` : ""}</p>
            <p>{selectedLocation.systemKeys.length ? selectedLocation.systemKeys.join(", ") : "Systeme noch nicht gepflegt"}</p>
            <p>
              {selectedLocation.upcomingGameCount} kommende Sessions
              {selectedLocation.nextGameStartTimeUtc ? ` · nächste ${new Date(selectedLocation.nextGameStartTimeUtc).toLocaleString("de-DE")}` : ""}
            </p>
            <div className="preview-actions">
              <Link to="/locations">Details öffnen</Link>
              <button type="button" onClick={() => createAtLocation(selectedLocation.locationId)}>
                Session hier erstellen
              </button>
              {selectedLocation.isOwnLocation && <Link to="/locations">Mitglieder ansehen</Link>}
            </div>
          </article>
        )}
      </section>
    </div>
  );
}
