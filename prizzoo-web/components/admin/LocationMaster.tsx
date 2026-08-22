"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { createOrEditLocation, deleteLocation, getLocations } from "@/lib/api/adminCatalog";
import { getKeralaDistrictsForCombobox } from "@/lib/api/geography";
import type { AdminLocation, ComboboxItem } from "@/lib/api/types";
import { ApiError } from "@/lib/api/client";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { EmptyState } from "@/components/common/EmptyState";
import { Modal } from "@/components/common/Modal";
import styles from "./CategoryMaster.module.css";

interface LocationFormState {
  id?: string;
  name: string;
  districtId: string;
  latitude: string;
  longitude: string;
  isActive: boolean;
}

const EMPTY_FORM: LocationFormState = {
  id: undefined,
  name: "",
  districtId: "",
  latitude: "",
  longitude: "",
  isActive: true,
};

export function LocationMaster() {
  const router = useRouter();
  const { isAuthenticated, isAdmin, isReady } = useAuth();
  const [districts, setDistricts] = useState<ComboboxItem[]>([]);
  const [filterDistrictId, setFilterDistrictId] = useState("");
  const [locations, setLocations] = useState<AdminLocation[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState<LocationFormState>(EMPTY_FORM);
  const [modalOpen, setModalOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [actingId, setActingId] = useState<string | null>(null);
  const [locatingNow, setLocatingNow] = useState(false);

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/phone-entry");
    } else if (isReady && isAuthenticated && !isAdmin) {
      router.replace("/home");
    }
  }, [isReady, isAuthenticated, isAdmin, router]);

  function loadDistrictsAndLocations(districtId: string) {
    setLoading(true);
    Promise.all([
      getKeralaDistrictsForCombobox(),
      getLocations(districtId ? Number(districtId) : undefined),
    ])
      .then(([districtList, locationList]) => {
        setDistricts(districtList);
        setLocations(locationList);
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : "Something went wrong."))
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    if (!isReady || !isAuthenticated) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadDistrictsAndLocations(filterDistrictId);
  }, [isReady, isAuthenticated, filterDistrictId]);

  function useCurrentLocationForForm() {
    if (typeof navigator === "undefined" || !navigator.geolocation) return;
    setLocatingNow(true);
    navigator.geolocation.getCurrentPosition(
      (position) => {
        setForm((f) => ({
          ...f,
          latitude: position.coords.latitude.toFixed(6),
          longitude: position.coords.longitude.toFixed(6),
        }));
        setLocatingNow(false);
      },
      () => setLocatingNow(false),
      { timeout: 8000 }
    );
  }

  function openAdd() {
    setForm(EMPTY_FORM);
    setModalOpen(true);
  }

  function openEdit(location: AdminLocation) {
    setForm({
      id: location.id,
      name: location.name,
      districtId: String(location.districtId),
      latitude: location.latitude != null ? String(location.latitude) : "",
      longitude: location.longitude != null ? String(location.longitude) : "",
      isActive: location.isActive,
    });
    setModalOpen(true);
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!form.districtId) return;
    setError(null);
    setSubmitting(true);
    try {
      await createOrEditLocation({
        id: form.id,
        name: form.name,
        districtId: Number(form.districtId),
        latitude: form.latitude ? Number(form.latitude) : undefined,
        longitude: form.longitude ? Number(form.longitude) : undefined,
        isActive: form.isActive,
      });
      setModalOpen(false);
      loadDistrictsAndLocations(filterDistrictId);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not save this location.");
    } finally {
      setSubmitting(false);
    }
  }

  async function handleDelete(id: string) {
    setActingId(id);
    setError(null);
    try {
      await deleteLocation(id);
      setLocations((prev) => prev?.filter((l) => l.id !== id) ?? null);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not delete this location.");
    } finally {
      setActingId(null);
    }
  }

  if (!isReady || !isAuthenticated || !isAdmin) {
    return null;
  }

  return (
    <div className={styles.root}>
      <button type="button" className={styles.back} onClick={() => router.back()}>
        ← Back
      </button>

      <div className={styles.topBar}>
        <div className={styles.topBarHeadings}>
          <h1 className={styles.heading}>Location master</h1>
          <p className={styles.subheading}>
            A locality within a district, e.g. &quot;Feroke&quot; within &quot;Kozhikode&quot;. Coordinates
            let a shopper&apos;s live GPS position be matched to the nearest location later.
          </p>
        </div>
        <button type="button" className={styles.addButton} onClick={openAdd}>
          + Add location
        </button>
      </div>

      <label className={styles.label}>
        Filter by district
        <select
          className={styles.field}
          value={filterDistrictId}
          onChange={(e) => setFilterDistrictId(e.target.value)}
        >
          <option value="">All districts</option>
          {districts.map((d) => (
            <option key={d.value} value={d.value}>
              {d.displayText}
            </option>
          ))}
        </select>
      </label>

      {loading && <LoadingSpinner />}
      {!loading && (!locations || locations.length === 0) && <EmptyState message="No locations yet." />}
      {!loading && locations && locations.length > 0 && (
        <div className={styles.list}>
          {locations.map((location) => (
            <div key={location.id} className={styles.card}>
              <div className={styles.info}>
                <div className={styles.name}>{location.name}</div>
                <div className={styles.meta}>
                  {location.districtName ?? "Unknown district"}
                  {location.latitude != null && location.longitude != null
                    ? ` · ${location.latitude.toFixed(4)}, ${location.longitude.toFixed(4)}`
                    : " · no coordinates"}
                </div>
              </div>
              <span className={location.isActive ? styles.badgeActive : styles.badgeInactive}>
                {location.isActive ? "Active" : "Inactive"}
              </span>
              <div className={styles.rowActions}>
                <button type="button" className={styles.editButton} onClick={() => openEdit(location)}>
                  Edit
                </button>
                <button
                  type="button"
                  className={styles.deleteButton}
                  disabled={actingId === location.id}
                  onClick={() => handleDelete(location.id)}
                >
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {modalOpen && (
        <Modal title={form.id ? "Edit location" : "Add a location under a district"} onClose={() => setModalOpen(false)}>
          <form className={styles.form} onSubmit={handleSubmit}>
            <label className={styles.label}>
              District (city)
              <select
                className={styles.field}
                value={form.districtId}
                onChange={(e) => setForm((f) => ({ ...f, districtId: e.target.value }))}
                required
              >
                <option value="" disabled>
                  Select a district
                </option>
                {districts.map((d) => (
                  <option key={d.value} value={d.value}>
                    {d.displayText}
                  </option>
                ))}
              </select>
            </label>
            <label className={styles.label}>
              Name
              <input
                className={styles.field}
                value={form.name}
                onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                required
              />
            </label>
            <label className={styles.label}>
              Latitude
              <input
                className={styles.field}
                type="number"
                step="0.000001"
                value={form.latitude}
                onChange={(e) => setForm((f) => ({ ...f, latitude: e.target.value }))}
              />
            </label>
            <label className={styles.label}>
              Longitude
              <input
                className={styles.field}
                type="number"
                step="0.000001"
                value={form.longitude}
                onChange={(e) => setForm((f) => ({ ...f, longitude: e.target.value }))}
              />
            </label>
            <button
              type="button"
              className={styles.locateButton}
              onClick={useCurrentLocationForForm}
              disabled={locatingNow}
            >
              {locatingNow ? "Detecting…" : "📍 Use my current location"}
            </button>
            <label className={styles.checkboxLabel}>
              <input
                type="checkbox"
                checked={form.isActive}
                onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))}
              />
              Active
            </label>

            {error && <ErrorBanner message={error} />}

            <button className={styles.submit} type="submit" disabled={submitting || !form.districtId}>
              {submitting ? <LoadingSpinner /> : form.id ? "Save changes" : "Add location"}
            </button>
          </form>
        </Modal>
      )}
    </div>
  );
}
