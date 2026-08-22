"use client";

import { useEffect, useState } from "react";
import { getKeralaDistrictsForCombobox } from "@/lib/api/geography";
import { getLocations } from "@/lib/api/adminCatalog";
import { ApiError } from "@/lib/api/client";
import type { AdminLocation, ComboboxItem } from "@/lib/api/types";
import type { ShopperLocation } from "@/lib/location/shopperLocation";
import { Modal } from "@/components/common/Modal";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import styles from "./LocationPickerModal.module.css";

interface LocationPickerModalProps {
  // True for the shopper's first-run pick - non-dismissible until they
  // choose something, since every search on the app depends on it.
  mandatory: boolean;
  onPicked: (location: ShopperLocation) => void;
  onClose: () => void;
}

// District -> Location picker, reused as both the mandatory first-login
// popup and the "change location" flow from LocationBar. Deliberately a
// plain select-from-list, not device geolocation - coordinates only ever
// come from an admin-captured Location (see LocationMaster), never a raw
// GPS reading, so a shopper is never asked for a location permission.
export function LocationPickerModal({ mandatory, onPicked, onClose }: LocationPickerModalProps) {
  const [districts, setDistricts] = useState<ComboboxItem[]>([]);
  const [districtId, setDistrictId] = useState("");
  const [locations, setLocations] = useState<AdminLocation[]>([]);
  const [locationId, setLocationId] = useState("");
  const [loadingDistricts, setLoadingDistricts] = useState(true);
  const [loadingLocations, setLoadingLocations] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getKeralaDistrictsForCombobox()
      .then(setDistricts)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Could not load districts."))
      .finally(() => setLoadingDistricts(false));
  }, []);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setLocationId("");
    if (!districtId) {
      setLocations([]);
      return;
    }
    setLoadingLocations(true);
    getLocations(Number(districtId))
      .then((list) => setLocations(list.filter((l) => l.isActive && l.latitude != null && l.longitude != null)))
      .catch(() => setLocations([]))
      .finally(() => setLoadingLocations(false));
  }, [districtId]);

  function handleConfirm() {
    const district = districts.find((d) => d.value === districtId);
    const location = locations.find((l) => l.id === locationId);
    if (!district || !location || location.latitude == null || location.longitude == null) return;

    onPicked({
      locationId: location.id,
      locationName: location.name,
      districtId: Number(districtId),
      districtName: district.displayText,
      latitude: location.latitude,
      longitude: location.longitude,
    });
  }

  return (
    <Modal title="Choose your location" onClose={onClose} dismissible={!mandatory}>
      <div className={styles.form}>
        <p className={styles.intro}>
          Products and prices are shown for stores near this location. You can change it any time from the
          home screen.
        </p>

        <label className={styles.label}>
          City (district)
          <select
            className={styles.field}
            value={districtId}
            onChange={(e) => setDistrictId(e.target.value)}
            disabled={loadingDistricts}
          >
            <option value="" disabled>
              Select a city
            </option>
            {districts.map((d) => (
              <option key={d.value} value={d.value}>
                {d.displayText}
              </option>
            ))}
          </select>
        </label>

        <label className={styles.label}>
          Location (locality within the city)
          <select
            className={styles.field}
            value={locationId}
            onChange={(e) => setLocationId(e.target.value)}
            disabled={!districtId || loadingLocations}
          >
            <option value="" disabled>
              Select a location
            </option>
            {locations.map((l) => (
              <option key={l.id} value={l.id}>
                {l.name}
              </option>
            ))}
          </select>
        </label>
        {districtId && !loadingLocations && locations.length === 0 && (
          <p className={styles.empty}>No locations available yet in this city.</p>
        )}

        {error && <ErrorBanner message={error} />}

        <button className={styles.submit} type="button" onClick={handleConfirm} disabled={!locationId}>
          {loadingDistricts ? <LoadingSpinner /> : "Confirm location"}
        </button>
      </div>
    </Modal>
  );
}
