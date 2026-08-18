"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { createStoreWithOwner } from "@/lib/api/adminStores";
import { getKeralaDistrictsForCombobox } from "@/lib/api/geography";
import { getCategoriesForCombobox, getLocationsForCombobox } from "@/lib/api/adminCatalog";
import { ApiError } from "@/lib/api/client";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ImageUploadField } from "@/components/common/ImageUploadField";
import type { ComboboxItem, CreatedStore } from "@/lib/api/types";
import styles from "./AdminForm.module.css";

export function CreateStoreForm() {
  const router = useRouter();

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

  const [ownerName, setOwnerName] = useState("");
  const [ownerSurname, setOwnerSurname] = useState("");
  const [ownerEmail, setOwnerEmail] = useState("");
  const [ownerPhoneNumber, setOwnerPhoneNumber] = useState("");

  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [created, setCreated] = useState<CreatedStore | null>(null);

  useEffect(() => {
    getKeralaDistrictsForCombobox()
      .then(setDistricts)
      .catch(() => {
        // District dropdown staying empty just means the admin has to type
        // the city as free text isn't possible anymore - not fatal, but
        // flagged via the empty select below.
      });
    getCategoriesForCombobox()
      .then(setCategories)
      .catch(() => {
        // Empty dropdown just means no categories to pick from yet.
      });
  }, []);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setLocationId("");
    if (!districtId) {
      setLocations([]);
      return;
    }
    getLocationsForCombobox(Number(districtId))
      .then(setLocations)
      .catch(() => setLocations([]));
  }, [districtId]);

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
    setSubmitting(true);
    try {
      const districtName = districts.find((d) => d.value === districtId)?.displayText;
      const categoryName = categories.find((c) => c.value === categoryId)?.displayText;
      const result = await createStoreWithOwner({
        name,
        address: address || undefined,
        // City is the district name as a fallback display value; LocationId
        // (if a specific locality was picked) is what the backend actually
        // treats as the source of truth and re-derives City from.
        city: districtName,
        locationId: locationId || undefined,
        phone: phone || undefined,
        latitude,
        longitude,
        openingHours: openingHours || undefined,
        categoryTags: categoryName,
        imageId: imageId ?? undefined,
        ownerName,
        ownerSurname: ownerSurname || undefined,
        ownerEmail,
        ownerPhoneNumber: ownerPhoneNumber || undefined,
      });
      setCreated(result);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not create this store.");
    } finally {
      setSubmitting(false);
    }
  }

  if (created) {
    return (
      <div className={styles.root}>
        <h1 className={styles.heading}>Store created</h1>
        <div className={styles.successCard}>
          <div className={styles.successName}>{created.name}</div>
          <p className={styles.body}>
            Share these login details with the shop owner directly - this password is shown
            only once and can&apos;t be retrieved again.
          </p>
          <div className={styles.credentialRow}>
            <span className={styles.credentialLabel}>Email</span>
            <span className={styles.credentialValue}>{ownerEmail}</span>
          </div>
          <div className={styles.credentialRow}>
            <span className={styles.credentialLabel}>Temporary password</span>
            <span className={styles.credentialValue}>{created.ownerTemporaryPassword}</span>
          </div>
        </div>
        <div className={styles.actions}>
          <button
            type="button"
            className={styles.submit}
            onClick={() => router.push("/admin/dashboard")}
          >
            Back to dashboard
          </button>
          <button type="button" className={styles.secondary} onClick={() => setCreated(null)}>
            Create another store
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.root}>
      <button type="button" className={styles.back} onClick={() => router.back()}>
        ← Back
      </button>
      <h1 className={styles.heading}>Create a store + owner</h1>
      <p className={styles.subheading}>
        There is no self-service signup - this creates the shop and its owner&apos;s login
        together, in one step.
      </p>

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
            onChange={(e) => setDistrictId(e.target.value)}
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
          Location: {latitude.toFixed(4)}, {longitude.toFixed(4)}
          <button
            type="button"
            className={styles.locationButton}
            onClick={useCurrentLocation}
            disabled={locatingNow}
          >
            {locatingNow ? "Detecting…" : "📍 Use my current location"}
          </button>
        </div>

        <h2 className={styles.sectionHeading}>Owner login</h2>
        <label className={styles.label}>
          Owner first name
          <input
            className={styles.field}
            value={ownerName}
            onChange={(e) => setOwnerName(e.target.value)}
            required
          />
        </label>
        <label className={styles.label}>
          Owner last name
          <input
            className={styles.field}
            value={ownerSurname}
            onChange={(e) => setOwnerSurname(e.target.value)}
          />
        </label>
        <label className={styles.label}>
          Owner email (their login username)
          <input
            className={styles.field}
            type="email"
            value={ownerEmail}
            onChange={(e) => setOwnerEmail(e.target.value)}
            required
          />
        </label>
        <label className={styles.label}>
          Owner phone
          <input
            className={styles.field}
            type="tel"
            value={ownerPhoneNumber}
            onChange={(e) => setOwnerPhoneNumber(e.target.value)}
          />
        </label>

        {error && <ErrorBanner message={error} />}

        <button className={styles.submit} type="submit" disabled={submitting}>
          {submitting ? <LoadingSpinner /> : "Create store + owner"}
        </button>
      </form>
    </div>
  );
}
