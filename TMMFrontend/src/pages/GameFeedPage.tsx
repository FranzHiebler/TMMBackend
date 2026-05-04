import { useEffect, useMemo, useState } from "react";
import { getAllGames } from "../api/gamesService";
import { useJoinGame } from "../api/useJoinGame";
import GameList from "../components/GameList";
import { useUser } from "../context/UserContext";
import type { GameResponse } from "../types/game";

type ViewFilter = "all" | "own" | "joined";

function Message({ text, type }: { text: string; type: "success" | "error" | "info" }) {
  if (!text) return null;
  return <div className={`message message-${type}`}>{text}</div>;
}

function isPast(game: GameResponse) {
  return new Date(game.startTimeUtc).getTime() < Date.now();
}

function isOwn(game: GameResponse, userId: string) {
  return game.host?.userId === userId;
}

function isJoinedOrApplied(game: GameResponse, userId: string) {
  return game.tables.some((table) =>
    table.assignedPlayers.some((p) => p.userId === userId) ||
    table.applications.some((a) => a.player.userId === userId)
  );
}

function matchesSystem(game: GameResponse, systemFilter: string) {
  if (!systemFilter) return true;

  return game.tables.some((table) =>
    table.systems.some((s) =>
      s.toLowerCase().includes(systemFilter.toLowerCase())
    )
  );
}

function hasOpenSlots(game: GameResponse) {
  return game.openSlots > 0;
}

function priority(game: GameResponse, userId: string) {
  if (isOwn(game, userId)) return 0;
  if (isJoinedOrApplied(game, userId)) return 1;
  return 2;
}

export default function GamesPage() {
  const user = useUser();

  const [games, setGames] = useState<GameResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [showPast, setShowPast] = useState(false);
  const [onlyOpenSlots, setOnlyOpenSlots] = useState(false);
  const [cityFilter, setCityFilter] = useState("");
  const [systemFilter, setSystemFilter] = useState("");
  const [viewFilter, setViewFilter] = useState<ViewFilter>("all");

  async function loadGames() {
    try {
      setLoading(true);
      setError("");
      setGames(await getAllGames());
    } catch (err) {
      setError(err instanceof Error ? err.message : "Games konnten nicht geladen werden.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadGames();
  }, []);

  function handleJoined(gameId: string, tableId: string) {
    setGames((prev) =>
      prev.map((game) =>
        game.id !== gameId
          ? game
          : {
              ...game,
              assignedPlayers: game.assignedPlayers + 1,
              openSlots: Math.max(0, game.openSlots - 1),
              tables: game.tables.map((table) =>
                table.id !== tableId
                  ? table
                  : {
                      ...table,
                      openSlots: Math.max(0, table.openSlots - 1),
                      assignedPlayers: [
                        ...table.assignedPlayers,
                        {
                          userId: user.userId,
                          displayName: user.displayName,
                        },
                      ],
                    }
              ),
            }
      )
    );
  }

  function handleApplied(gameId: string, tableId: string, systemKey?: string) {
    setGames((prev) =>
      prev.map((game) =>
        game.id !== gameId
          ? game
          : {
              ...game,
              tables: game.tables.map((table) =>
                table.id !== tableId
                  ? table
                  : {
                      ...table,
                      applications: [
                        ...table.applications,
                        {
                          id: `local-${Date.now()}`,
                          tableId,
                          player: {
                            userId: user.userId,
                            displayName: user.displayName,
                          },
                          systemKey: systemKey ?? null,
                          message: null,
                          status: 0,
                          createdAt: new Date().toISOString(),
                        },
                      ],
                    }
              ),
            }
      )
    );
  }

  const { join, joiningKey, errorMessage, successMessage, messageByKey } = useJoinGame({
    onJoined: handleJoined,
    onApplied: handleApplied,
  });

  const filteredGames = useMemo(() => {
    return games
      .filter((game) => showPast || !isPast(game))
      .filter((game) => !onlyOpenSlots || hasOpenSlots(game))
      .filter((game) =>
        !cityFilter ||
        game.location?.city?.toLowerCase().includes(cityFilter.toLowerCase())
      )
      .filter((game) => matchesSystem(game, systemFilter))
      .filter((game) => {
        if (viewFilter === "own") return isOwn(game, user.userId);
        if (viewFilter === "joined") return isJoinedOrApplied(game, user.userId);
        return true;
      })
      .sort((a, b) => {
        const p = priority(a, user.userId) - priority(b, user.userId);
        if (p !== 0) return p;

        return new Date(a.startTimeUtc).getTime() - new Date(b.startTimeUtc).getTime();
      });
  }, [games, showPast, onlyOpenSlots, cityFilter, systemFilter, viewFilter, user.userId]);

  return (
    <div className="container">
      <Message text={successMessage} type="success" />
      <Message text={errorMessage} type="error" />
      <Message text={error} type="error" />
      {loading && <Message text="Lade Games..." type="info" />}

      <h1>Alle GameSessions ({filteredGames.length})</h1>

      <div className="card form">
        <select value={viewFilter} onChange={(e) => setViewFilter(e.target.value as ViewFilter)}>
          <option value="all">Alle anzeigen</option>
          <option value="own">Nur meine Sessions</option>
          <option value="joined">Nur angemeldet / beworben</option>
        </select>

        <input
          value={cityFilter}
          onChange={(e) => setCityFilter(e.target.value)}
          placeholder="Stadt filtern"
        />

        <input
          value={systemFilter}
          onChange={(e) => setSystemFilter(e.target.value)}
          placeholder="System filtern, z.B. Warhammer"
        />

        <label>
          <input
            type="checkbox"
            checked={onlyOpenSlots}
            onChange={(e) => setOnlyOpenSlots(e.target.checked)}
          />
          Nur mit freien Plätzen
        </label>

        <label>
          <input
            type="checkbox"
            checked={showPast}
            onChange={(e) => setShowPast(e.target.checked)}
          />
          Vergangene Sessions anzeigen
        </label>
      </div>

      {!loading && !error && (
        <GameList
          games={filteredGames}
          joiningKey={joiningKey}
          currentUserId={user.userId}
          messageByKey={messageByKey}
          onJoin={join}
        />
      )}
    </div>
  );
}