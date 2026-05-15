import { useCallback, useEffect, useMemo, useState } from "react";
import {
  acceptFriendRequest,
  getFriendRequests,
  getFriends,
  rejectFriendRequest,
  removeFriend,
  sendFriendRequest,
} from "../api/friendsApi";
import { searchUsers } from "../api/usersApi";
import DirectMessageButton from "../components/DirectMessageButton";
import { useToast } from "../context/ToastContext";
import { useUser } from "../context/UserContext";
import type { FriendDto, FriendRequestDto, UserSearchResponse } from "../types/game";

export default function FriendsPage() {
  const user = useUser();
  const { showToast } = useToast();
  const [friends, setFriends] = useState<FriendDto[]>([]);
  const [requests, setRequests] = useState<FriendRequestDto[]>([]);
  const [results, setResults] = useState<UserSearchResponse[]>([]);
  const [query, setQuery] = useState("");
  const [busyKey, setBusyKey] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const friendIds = useMemo(() => new Set(friends.map((friend) => friend.userId)), [friends]);

  const visibleResults = results.filter((result) =>
    result.userId !== user.userId &&
    !friendIds.has(result.userId)
  );

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [nextFriends, nextRequests] = await Promise.all([
        getFriends(user),
        getFriendRequests(user),
      ]);
      setFriends(nextFriends);
      setRequests(nextRequests);
    } catch (error) {
      showToast("error", error instanceof Error ? error.message : "Freunde konnten nicht geladen werden");
    } finally {
      setLoading(false);
    }
  }, [showToast, user]);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void load();
  }, [load]);

  useEffect(() => {
    const timeout = window.setTimeout(async () => {
      try {
        setResults(query.trim().length >= 2 ? await searchUsers(query) : []);
      } catch (error) {
        showToast("error", error instanceof Error ? error.message : "User-Suche fehlgeschlagen");
      }
    }, 250);

    return () => window.clearTimeout(timeout);
  }, [query, showToast]);

  async function requestFriend(result: UserSearchResponse) {
    setBusyKey(`request-${result.userId}`);
    try {
      await sendFriendRequest({
        receiverUserId: result.userId,
        receiverDisplayName: result.displayName,
      }, user);
      showToast("success", "Freundschaftsanfrage gesendet");
      setQuery("");
      setResults([]);
      await load();
    } catch (error) {
      showToast("error", error instanceof Error ? error.message : "Anfrage konnte nicht gesendet werden");
    } finally {
      setBusyKey(null);
    }
  }

  async function accept(id: string) {
    setBusyKey(`accept-${id}`);
    try {
      await acceptFriendRequest(id, user);
      showToast("success", "Freundschaft angenommen");
      await load();
    } catch (error) {
      showToast("error", error instanceof Error ? error.message : "Anfrage konnte nicht angenommen werden");
    } finally {
      setBusyKey(null);
    }
  }

  async function reject(id: string) {
    setBusyKey(`reject-${id}`);
    try {
      await rejectFriendRequest(id, user);
      showToast("success", "Anfrage abgelehnt");
      await load();
    } catch (error) {
      showToast("error", error instanceof Error ? error.message : "Anfrage konnte nicht abgelehnt werden");
    } finally {
      setBusyKey(null);
    }
  }

  async function remove(id: string) {
    setBusyKey(`remove-${id}`);
    try {
      await removeFriend(id, user);
      showToast("success", "Freund entfernt");
      await load();
    } catch (error) {
      showToast("error", error instanceof Error ? error.message : "Freund konnte nicht entfernt werden");
    } finally {
      setBusyKey(null);
    }
  }

  return (
    <main className="page friends-page">
      <div className="page-header">
        <div>
          <h1>Freunde</h1>
          <p className="page-subtitle">Finde Leute wieder, schreib sie direkt an und verwalte Anfragen.</p>
        </div>
      </div>

      <section className="card friend-search-card">
        <h3>Freund hinzufügen</h3>
        <input
          value={query}
          placeholder="User suchen..."
          onChange={(e) => setQuery(e.target.value)}
        />

        {query.trim().length > 0 && query.trim().length < 2 && (
          <div className="thread-empty">Gib mindestens zwei Zeichen ein.</div>
        )}

        <div className="friend-result-list">
          {visibleResults.map((result) => (
            <div key={result.userId} className="friend-row">
              <div>
                <b>{result.displayName}</b>
                {result.email && <small>{result.email}</small>}
              </div>
              <button
                type="button"
                disabled={busyKey === `request-${result.userId}`}
                onClick={() => requestFriend(result)}
              >
                Freund hinzufügen
              </button>
            </div>
          ))}
        </div>
      </section>

      <div className="friends-layout">
        <section className="card">
          <h3>Meine Freunde</h3>
          {loading && <div className="thread-empty">Freunde werden geladen...</div>}
          {!loading && friends.length === 0 && <div className="thread-empty">Noch keine Freunde.</div>}

          <div className="friend-list">
            {friends.map((friend) => (
              <div key={friend.id} className="friend-row">
                <div>
                  <b>{friend.displayName}</b>
                  <small>Seit {new Date(friend.updatedAtUtc).toLocaleDateString("de-DE")}</small>
                </div>
                <div className="friend-actions">
                  <DirectMessageButton
                    recipientUserId={friend.userId}
                    recipientDisplayName={friend.displayName}
                    contextLabel="aus deiner Freundesliste"
                    compact
                  />
                  <button
                    type="button"
                    disabled={busyKey === `remove-${friend.id}`}
                    onClick={() => remove(friend.id)}
                  >
                    Entfernen
                  </button>
                </div>
              </div>
            ))}
          </div>
        </section>

        <section className="card">
          <h3>Anfragen</h3>
          {requests.length === 0 && <div className="thread-empty">Keine offenen Anfragen.</div>}

          <div className="friend-list">
            {requests.map((request) => (
              <div key={request.id} className="friend-row">
                <div>
                  <b>{request.requesterDisplayName}</b>
                  <small>{new Date(request.createdAtUtc).toLocaleString("de-DE")}</small>
                </div>
                <div className="friend-actions">
                  <button
                    type="button"
                    disabled={busyKey === `accept-${request.id}`}
                    onClick={() => accept(request.id)}
                  >
                    Annehmen
                  </button>
                  <button
                    type="button"
                    disabled={busyKey === `reject-${request.id}`}
                    onClick={() => reject(request.id)}
                  >
                    Ablehnen
                  </button>
                </div>
              </div>
            ))}
          </div>
        </section>
      </div>
    </main>
  );
}
