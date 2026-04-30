import { MapContainer, Marker, TileLayer, useMap, useMapEvents } from "react-leaflet";
import type { LatLngExpression } from "leaflet";
import { useEffect } from "react";

type Props = {
  latitude: number | null;
  longitude: number | null;
  onChange: (lat: number, lng: number) => void;
};

export default function LocationPicker({ latitude, longitude, onChange }: Props) {
  const position: LatLngExpression = [
    latitude ?? 50.5558,
    longitude ?? 9.6808,
  ];

  function ClickHandler() {
    useMapEvents({
      click(e) {
        onChange(e.latlng.lat, e.latlng.lng);
      },
    });

    return latitude != null && longitude != null ? (
      <Marker position={[latitude, longitude]} />
    ) : null;
  }


  function FlyToLocation({ latitude, longitude }: { latitude: number | null; longitude: number | null }) {
    const map = useMap();

    useEffect(() => {
      if (latitude && longitude) {
        map.setView([latitude, longitude], 14); // oder flyTo
      }
    }, [latitude, longitude]);

    return null;
  }

  return (
    <div style={{ height: 300, marginTop: 12 }}>
      <MapContainer center={position} zoom={12} style={{ height: "100%" }}>
        <TileLayer
          attribution="&copy; OpenStreetMap"
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />
        <ClickHandler />
        <FlyToLocation latitude={latitude} longitude={longitude} />
      </MapContainer>
    </div>
  );
}