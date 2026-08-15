"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import { applyAsRetailer } from "@/lib/api/retailer";
import { useGeolocation } from "@/lib/geolocation/useGeolocation";
import { ApiError } from "@/lib/api/client";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ImageUploadField } from "@/components/common/ImageUploadField";
import styles from "./RetailerForm.module.css";

export function ApplyForm() {
  const router = useRouter();
  const { coordinates, isLocating, isFallback } = useGeolocation();
  const [name, setName] = useState("");
  const [address, setAddress] = useState("");
  const [city, setCity] = useState("");
  const [phone, setPhone] = useState("");
  const [openingHours, setOpeningHours] = useState("");
  const [categoryTags, setCategoryTags] = useState("");
  const [imageId, setImageId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await applyAsRetailer({
        name,
        address,
        city,
        phone,
        openingHours,
        categoryTags,
        latitude: coordinates.latitude,
        longitude: coordinates.longitude,
        imageId: imageId ?? undefined,
      });
      router.replace("/retailer/dashboard");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not submit your application.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className={styles.root}>
      <button type="button" className={styles.back} onClick={() => router.back()}>
        ← Back
      </button>
      <h1 className={styles.heading}>Tell us about your shop</h1>
      <p className={styles.subheading}>
        We&apos;ll review your details before your shop goes live on Prizzoo.
      </p>

      <form className={styles.form} onSubmit={handleSubmit}>
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
          <input
            className={styles.field}
            value={address}
            onChange={(e) => setAddress(e.target.value)}
          />
        </label>
        <label className={styles.label}>
          City
          <input
            className={styles.field}
            value={city}
            onChange={(e) => setCity(e.target.value)}
          />
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
          Categories
          <input
            className={styles.field}
            placeholder="e.g. mobiles, electronics"
            value={categoryTags}
            onChange={(e) => setCategoryTags(e.target.value)}
          />
        </label>
        <ImageUploadField label="Shop photo" value={imageId} onChange={setImageId} />

        <p className={styles.locationNote}>
          {isLocating
            ? "Detecting your shop's location…"
            : isFallback
              ? "Couldn't detect your location - using a default. You can update this later."
              : `Location detected: ${coordinates.latitude.toFixed(4)}, ${coordinates.longitude.toFixed(4)}`}
        </p>

        {error && <ErrorBanner message={error} />}

        <button className={styles.submit} type="submit" disabled={submitting || isLocating}>
          {submitting ? <LoadingSpinner /> : "Submit application"}
        </button>
      </form>
    </div>
  );
}
