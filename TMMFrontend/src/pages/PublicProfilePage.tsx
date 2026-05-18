import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { getPublicUserProfile } from "../api/usersApi";
import DirectMessageButton from "../components/DirectMessageButton";
import Message from "../components/Message";
import { useUser } from "../context/UserContext";
import type { PublicUserProfileResponse } from "../types/game";

function ProfileValue({ label, value }: { label: string; value?: string | null }) {
  if (!value) return null;

  return (
    <div className="public-profile-row">
      <span>{label}</span>
      <b>{value}</b>
    </div>
  );
}

export default function PublicProfilePage() {
  const { userId } = useParams();
  const user = useUser();

  const [profile, setProfile] = useState<PublicUserProfileResponse | null>(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function load() {
      if (!userId) {
        setError("UserId fehlt.");
        setLoading(false);
        return;
      }

      try {
        setError("");
        setProfile(await getPublicUserProfile(userId, user));
      } catch (err) {
        setError(err instanceof Error ? err.message : "Profil konnte nicht geladen werden.");
      } finally {
        setLoading(false);
      }
    }

    void load();
  }, [user, userId]);

  return (
    <main className="container public-profile-page">
      <Link to="/friends" className="back-link">
        ← Zurück zu Freunde
      </Link>

      <Message text={loading ? "Lade Profil..." : ""} type="info" />
      <Message text={error} type="error" />

      {!loading && profile && (
        <section className="card public-profile-card">
          <div className="public-profile-header">
            <div className="public-profile-avatar">
              {profile.profileImageUrl ? (
                <img src={profile.profileImageUrl} alt={profile.displayName} />
              ) : (
                <span>{profile.displayName.slice(0, 1).toUpperCase()}</span>
              )}
            </div>

            <div>
              <h1>{profile.displayName}</h1>
              <p className="page-subtitle">
                {profile.isFriend ? "Freund" : "Öffentliches Profil"}
              </p>
            </div>

            {profile.canBeContacted && profile.userId !== user.userId && (
              <DirectMessageButton
                recipientUserId={profile.userId}
                recipientDisplayName={profile.displayName}
                contextLabel="aus dem öffentlichen Profil"
              />
            )}
          </div>

          <h2>Kontakt</h2>
          <div className="public-profile-grid">
            <ProfileValue label="E-Mail" value={profile.email} />
            <ProfileValue label="Telefon" value={profile.phoneNumber} />
            <ProfileValue label="Straße" value={profile.streetAddress} />
            <ProfileValue label="PLZ" value={profile.postalCode} />
            <ProfileValue label="Ort" value={profile.city} />
          </div>

          <h2>Tabletop-Profile</h2>
          <div className="public-profile-grid">
            <ProfileValue label="TabletopTO" value={profile.tabletopTo} />
            <ProfileValue label="Tabletop Herald" value={profile.tabletopHerald} />
            <ProfileValue label="T3" value={profile.t3} />
            <ProfileValue label="NewRecruit" value={profile.newRecruit} />
            <ProfileValue label="Best Coast Pairings / BCP" value={profile.bestSportsPairings} />
          </div>
        </section>
      )}
    </main>
  );
}