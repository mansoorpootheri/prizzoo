"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
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
  onSaved: () => void;
}

export function EditStoreForm({ storeId, onSaved }: EditStoreFormProps) {
  const router = useRouter();
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
  // Read-only display of the store's current (Location-derived) coordinates
  // - there is no way to edit these directly any more, only by picking a
  // different Location below.
  const [latitude, setLatitude] = useState(0);
  const [longitude, setLongitude] = useState(0);
  const [isVerified, setIsVerified] = useState(false);
  const [isActive, setIsActive] = useState(true);

  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
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
  }, [storeId]);

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

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!locationId) return;
    setError(null);
    setSubmitting(true);
    try {
      const districtName = districts.find((d) => d.value === districtId)?.displayText;
      const categoryName = categories.find((c) => c.value === categoryId)?.displayText;
      await updateStore({
        id: storeId,
        name,
        address: address || undefined,
        city: districtName,
        locationId,
        phone: phone || undefined,
        openingHours: openingHours || undefined,
        categoryTags: categoryName,
        imageId: imageId ?? undefined,
        isVerified,
        isActive,
      });
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not save this store.");
    } finally {
      setSubmitting(false);
    }
  }

  if (loading) {
    return <LoadingSpinner />;
  }

  if (loadError) {
    return <ErrorBanner message={loadError} />;
  }

  return (
    <form className={styles.form} onSubmit={handleSubmit}>
      <button
        type="button"
        className={styles.locateButton}
        onClick={() => router.push(`/store-flyer?storeId=${storeId}`)}
      >
        📄 View uploaded flyers for this store
      </button>
      <label className={styles.label}>
        Shop name
        <input className={styles.field} value={name} onChange={(e) => setName(e.target.value)} required />
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
          required
        >
          <option value="" disabled>
            Select a location
          </option>
          {locations.map((l) => (
            <option key={l.value} value={l.value}>
              {l.displayText}
            </option>
          ))}
        </select>
      </label>
      <label className={styles.label}>
        Phone
        <input className={styles.field} type="tel" value={phone} onChange={(e) => setPhone(e.target.value)} />
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
        <select className={styles.field} value={categoryId} onChange={(e) => setCategoryId(e.target.value)}>
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
        Currently stored coordinates: {latitude.toFixed(6)}, {longitude.toFixed(6)} (updates to match
        whichever location is selected above, once saved)
      </div>

      <label className={styles.checkboxLabel}>
        <input type="checkbox" checked={isVerified} onChange={(e) => setIsVerified(e.target.checked)} />
        Verified
      </label>
      <label className={styles.checkboxLabel}>
        <input type="checkbox" checked={isActive} onChange={(e) => setIsActive(e.target.checked)} />
        Active
      </label>

      {error && <ErrorBanner message={error} />}

      <button className={styles.submit} type="submit" disabled={submitting}>
        {submitting ? <LoadingSpinner /> : "Save changes"}
      </button>
    </form>
  );
}
