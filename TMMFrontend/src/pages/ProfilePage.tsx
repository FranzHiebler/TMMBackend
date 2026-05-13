import { useCallback, useEffect, useState } from "react";
import { getMyLocations } from "../api/locationsApi";
import { getCurrentUserProfile, updateCurrentUserProfile } from "../api/usersApi";
import Message from "../components/Message";
import { useUser } from "../context/UserContext";
import type { LocationResponse, UserProfileResponse } from "../types/game";

export default function ProfilePage() {
  const user = useUser();

  const [profile, setProfile] = useState<UserProfileResponse | null>(null);
  const [locations, setLocations] = useState<LocationResponse[]>([]);
  const [displayName, setDisplayName] = useState("");
  const [defaultLocationId, setDefaultLocationId] = useState("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const loadProfile = useCallback(async () => {
    try {
      setError("");

      const [profileData, locationData] = await Promise.all([
        getCurrentUserProfile(user),
        getMyLocations(user),
      ]);

      setProfile(profileData);
      setLocations(locationData);
      setDisplayName(profileData.displayName);
      setDefaultLocationId(profileData.defaultLocationId ?? "");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Profil konnte nicht geladen werden.");
    } finally {
      setLoading(false);
    }
  }, [user]);

  useEffect(() => {
    void loadProfile();
  }, [loadProfile]);

  useEffect(() => {
    if (!success) return;

    const timeout = window.setTimeout(() => setSuccess(""), 3500);
    return () => window.clearTimeout(timeout);
  }, [success]);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    try {
      setSaving(true);
      setError("");
      setSuccess("");

      const updated = await updateCurrentUserProfile(
        {
          displayName,
          defaultLocationId: defaultLocationId || null,
        },
        user
      );

      setProfile(updated);
      user.setUser({
        userId: updated.userId,
        displayName: updated.displayName,
      });

      setSuccess("Profil gespeichert.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Profil konnte nicht gespeichert werden.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <main className="container">
      <h1>Mein Profil</h1>

      <Message text={loading ? "Lade Profil..." : ""} type="info" />
      <Message text={error} type="error" />
      <Message text={success} type="success" />

      {!loading && profile && (
        <form className="card form" onSubmit={handleSubmit}>
          <div className="field">
            <label>User-ID</label>
            <input value={profile.userId} disabled />
          </div>

          <div className="field">
            <label>Anzeigename</label>
            <input
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              placeholder="Dein Anzeigename"
            />
          </div>

          <div className="field">
            <label>E-Mail</label>
            <input value={profile.email ?? ""} disabled placeholder="Noch nicht hinterlegt" />
          </div>

          <div className="field">
            <label>Standard-Location</label>
            <select
              value={defaultLocationId}
              onChange={(e) => setDefaultLocationId(e.target.value)}
            >
              <option value="">Keine Standard-Location</option>

              {locations.map((location) => (
                <option key={location.id} value={location.id}>
                  {location.name} ({location.city})
                </option>
              ))}
            </select>
          </div>

          <button type="submit" disabled={saving}>
            {saving ? "Speichert..." : "Profil speichern"}
          </button>
        </form>
      )}
    </main>
  );
}