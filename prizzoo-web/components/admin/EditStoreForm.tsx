"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { getStore, updateStore } from "@/lib/api/adminStores";
import { getKeralaDistrictsForCombobox } from "@/lib/api/geography";
import { getCategoriesForCombobox, getLocationsForCombobox } from "@/lib/api/adminCatalog";
import { ApiError } from "@/lib/api/client";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ImageUploadField } from "@/components/common/ImageUploadField";
import type { ComboboxItem } from "@/lib/api/types";
import styles from "./AdminForm.module.css";

interface EditStoreFormProps {
  storeId: string;
}

export function EditStoreForm({ storeId }: EditStoreFormProps) {
  const router = useRouter();
  const { isAuthenticated, isReady } = useAuth();

  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);

  const [name, setName] = useState("");
  const [address, setAddress] = useState("");
  const [districts, setDistricts] = useState<ComboboxItem[]>([]);
  const [districtId, setDistrictId] = useState("");
  const [locations, setLocations] = useState<ComboboxItem[]>([]);
  const [locationId, setLocationId] = useState("");
  const [phone, setPhone] = useState("");
  const [openingHours, setOpeningHours] = useState("");
  const [categories, setCategories] = useState<ComboboxItem[]>([]);
  const [categoryId, setCategoryId] = useState("");
  const [imageId, setImageId] = useState<string | null>(null);
  const [latitude, setLatitude] = useState(0);
  const [longitude, setLongitude] = useState(0);
  const [locatingNow, setLocatingNow] = useState(false);
  const [isVerified, setIsVerified] = useState(false);
  const [isActive, setIsActive] = useState(true);

  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/login");
    }
  }, [isReady, isAuthenticated, router]);

  useEffect(() => {
    if (!isReady || !isAuthenticated) return;

    Promise.all([getStore(storeId), getKeralaDistrictsForCombobox(), getCategoriesForCombobox()])
      .then(([store, districtList, categoryList]) => {
        setName(store.name);
        setAddress(store.address ?? "");
        setPhone(store.phone ?? "");
        setOpeningHours(store.openingHours ?? "");
        setImageId(store.imageId);
        setLatitude(store.latitude);
        setLongitude(store.longitude);
        setIsVerified(store.isVerified);
        setIsActive(store.isActive);

        setDistricts(districtList);
        if (store.districtId != null) {
          setDistrictId(String(store.districtId));
        }

        setCategories(categoryList);
        if (store.categoryTags) {
          const match = categoryList.find((c) => c.displayText === store.categoryTags);
          if (match) setCategoryId(match.value);
        }

        if (store.districtId != null && store.locationId) {
          getLocationsForCombobox(store.districtId)
            .then((locationList) => {
              setLocations(locationList);
              setLocationId(store.locationId!);
            })
            .catch(() => {
              // Location dropdown staying empty just means it has to be re-picked.
            });
        }
      })
      .catch((err) => setLoadError(err instanceof ApiError ? err.message : "Could not load this store."))
      .finally(() => setLoading(false));
  }, [isReady, isAuthenticated, storeId]);

  // Only re-fetch locations on a *manual* district change, not the initial
  // load above (which already fetches the right list for the store's
  // existing district and must not have its selection clobbered by this).
  const [districtTouched, setDistrictTouched] = useState(false);
  useEffect(() => {
    if (!districtTouched) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setLocationId("");
    if (!districtId) {
      setLocations([]);
      return;
    }
    getLocationsForCombobox(Number(districtId))
      .then(setLocations)
      .catch(() => setLocations([]));
  }, [districtId, districtTouched]);

  function useCurrentLocation() {
    if (typeof navigator === "undefined" || !navigator.geolocation) return;
    setLocatingNow(true);
    navigator.geolocation.getCurrentPosition(
      (position) => {
        setLatitude(position.coords.latitude);
        setLongitude(position.coords.longitude);
        setLocatingNow(false);
      },
      () => setLocatingNow(false),
      { timeout: 8000 }
    );
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setSaved(false);
    setSubmitting(true);
    try {
      const districtName = districts.find((d) => d.value === districtId)?.displayText;
      const categoryName = categories.find((c) => c.value === categoryId)?.displayText;
      await updateStore({
        id: storeId,
        name,
        address: address || undefined,
        city: districtName,
        locationId: locationId || undefined,
        phone: phone || undefined,
        latitude,
        longitude,
        openingHours: openingHours || undefined,
        categoryTags: categoryName,
        imageId: imageId ?? undefined,
        isVerified,
        isActive,
      });
      setSaved(true);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not save this store.");
    } finally {
      setSubmitting(false);
    }
  }

  if (!isReady || !isAuthenticated) {
    return null;
  }

  if (loading) {
    return (
      <div className={styles.root}>
        <LoadingSpinner />
      </div>
    );
  }

  if (loadError) {
    return (
      <div className={styles.root}>
        <button type="button" className={styles.back} onClick={() => router.back()}>
          ← Back
        </button>
        <ErrorBanner message={loadError} />
      </div>
    );
  }

  return (
    <div className={styles.root}>
      <button type="button" className={styles.back} onClick={() => router.back()}>
        ← Back
      </button>
      <h1 className={styles.heading}>Edit store</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <h2 className={styles.sectionHeading}>Shop details</h2>
        <label className={styles.label}>
          Shop name
          <input
            className={styles.field}
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
          />
        </label>
        <label className={styles.label}>
          Address
          <input className={styles.field} value={address} onChange={(e) => setAddress(e.target.value)} />
        </label>
        <label className={styles.label}>
          City (district)
          <select
            className={styles.field}
            value={districtId}
            onChange={(e) => {
              setDistrictTouched(true);
              setDistrictId(e.target.value);
            }}
            required
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
            disabled={!districtId}
          >
            <option value="">No specific location</option>
            {locations.map((l) => (
              <option key={l.value} value={l.value}>
                {l.displayText}
              </option>
            ))}
          </select>
        </label>
        <label className={styles.label}>
          Phone
          <input
            className={styles.field}
            type="tel"
            value={phone}
            onChange={(e) => setPhone(e.target.value)}
          />
        </label>
        <label className={styles.label}>
          Opening hours
          <input
            className={styles.field}
            placeholder="e.g. 9am - 9pm"
            value={openingHours}
            onChange={(e) => setOpeningHours(e.target.value)}
          />
        </label>
        <label className={styles.label}>
          Category
          <select
            className={styles.field}
            value={categoryId}
            onChange={(e) => setCategoryId(e.target.value)}
          >
            <option value="">Select a category</option>
            {categories.map((c) => (
              <option key={c.value} value={c.value}>
                {c.displayText}
              </option>
            ))}
          </select>
        </label>
        <ImageUploadField label="Shop photo" value={imageId} onChange={setImageId} />
        <div className={styles.locationNote}>
          Location: {latitude.toFixed(6)}, {longitude.toFixed(6)}
          {latitude === 0 && longitude === 0 && (
            <div className={styles.body}>⚠ No real coordinates set - this store won&apos;t show up in nearby searches.</div>
          )}
          <button
            type="button"
            className={styles.locationButton}
            onClick={useCurrentLocation}
            disabled={locatingNow}
          >
            {locatingNow ? "Detecting…" : "📍 Use my current location"}
          </button>
        </div>

        <label className={styles.checkboxLabel}>
          <input
            type="checkbox"
            checked={isVerified}
            onChange={(e) => setIsVerified(e.target.checked)}
          />
          Verified
        </label>
        <label className={styles.checkboxLabel}>
          <input
            type="checkbox"
            checked={isActive}
            onChange={(e) => setIsActive(e.target.checked)}
          />
          Active
        </label>

        {error && <ErrorBanner message={error} />}
        {saved && !error && <p className={styles.body}>Saved.</p>}

        <button className={styles.submit} type="submit" disabled={submitting}>
          {submitting ? <LoadingSpinner /> : "Save changes"}
        </button>
      </form>
    </div>
  );
}
