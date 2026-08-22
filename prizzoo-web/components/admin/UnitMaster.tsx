"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { createOrEditUnit, deleteUnit, getUnits } from "@/lib/api/adminCatalog";
import type { AdminUnit } from "@/lib/api/types";
import { ApiError } from "@/lib/api/client";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { EmptyState } from "@/components/common/EmptyState";
import { Modal } from "@/components/common/Modal";
import styles from "./CategoryMaster.module.css";

interface UnitFormState {
  id?: string;
  name: string;
  code: string;
  decimalPlaces: number;
  isActive: boolean;
}

const EMPTY_FORM: UnitFormState = {
  id: undefined,
  name: "",
  code: "",
  decimalPlaces: 0,
  isActive: true,
};

export function UnitMaster() {
  const router = useRouter();
  const { isAuthenticated, isAdmin, isReady } = useAuth();
  const [units, setUnits] = useState<AdminUnit[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState<UnitFormState>(EMPTY_FORM);
  const [modalOpen, setModalOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [actingId, setActingId] = useState<string | null>(null);

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/phone-entry");
    } else if (isReady && isAuthenticated && !isAdmin) {
      router.replace("/home");
    }
  }, [isReady, isAuthenticated, isAdmin, router]);

  function load() {
    setLoading(true);
    getUnits()
      .then(setUnits)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Something went wrong."))
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    if (!isReady || !isAuthenticated) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect
    load();
  }, [isReady, isAuthenticated]);

  function openAdd() {
    setForm(EMPTY_FORM);
    setModalOpen(true);
  }

  function openEdit(unit: AdminUnit) {
    setForm({
      id: unit.id,
      name: unit.name,
      code: unit.code,
      decimalPlaces: unit.decimalPlaces,
      isActive: unit.isActive,
    });
    setModalOpen(true);
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await createOrEditUnit({
        id: form.id,
        name: form.name,
        code: form.code,
        decimalPlaces: form.decimalPlaces,
        isActive: form.isActive,
      });
      setModalOpen(false);
      load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not save this unit.");
    } finally {
      setSubmitting(false);
    }
  }

  async function handleDelete(id: string) {
    setActingId(id);
    setError(null);
    try {
      await deleteUnit(id);
      setUnits((prev) => prev?.filter((u) => u.id !== id) ?? null);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not delete this unit.");
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
          <h1 className={styles.heading}>Unit master</h1>
          <p className={styles.subheading}>
            Units of measure a product can be sold in - e.g. kg, litre, piece.
          </p>
        </div>
        <button type="button" className={styles.addButton} onClick={openAdd}>
          + Add unit
        </button>
      </div>

      {loading && <LoadingSpinner />}
      {!loading && (!units || units.length === 0) && <EmptyState message="No units yet." />}
      {!loading && units && units.length > 0 && (
        <div className={styles.list}>
          {units.map((unit) => (
            <div key={unit.id} className={styles.card}>
              <div className={styles.info}>
                <div className={styles.name}>{unit.name}</div>
                <div className={styles.meta}>
                  {unit.code} · {unit.decimalPlaces} decimal place{unit.decimalPlaces === 1 ? "" : "s"}
                </div>
              </div>
              <span className={unit.isActive ? styles.badgeActive : styles.badgeInactive}>
                {unit.isActive ? "Active" : "Inactive"}
              </span>
              <div className={styles.rowActions}>
                <button type="button" className={styles.editButton} onClick={() => openEdit(unit)}>
                  Edit
                </button>
                <button
                  type="button"
                  className={styles.deleteButton}
                  disabled={actingId === unit.id}
                  onClick={() => handleDelete(unit.id)}
                >
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {modalOpen && (
        <Modal title={form.id ? "Edit unit" : "Add a unit"} onClose={() => setModalOpen(false)}>
          <form className={styles.form} onSubmit={handleSubmit}>
            <label className={styles.label}>
              Name
              <input
                className={styles.field}
                placeholder="e.g. Kilogram"
                value={form.name}
                onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                required
              />
            </label>
            <label className={styles.label}>
              Code
              <input
                className={styles.field}
                placeholder="e.g. kg"
                value={form.code}
                onChange={(e) => setForm((f) => ({ ...f, code: e.target.value }))}
                required
              />
            </label>
            <label className={styles.label}>
              Decimal places
              <input
                className={styles.field}
                type="number"
                min={0}
                max={6}
                value={form.decimalPlaces}
                onChange={(e) => setForm((f) => ({ ...f, decimalPlaces: Number(e.target.value) }))}
              />
            </label>
            <label className={styles.checkboxLabel}>
              <input
                type="checkbox"
                checked={form.isActive}
                onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))}
              />
              Active
            </label>

            {error && <ErrorBanner message={error} />}

            <button className={styles.submit} type="submit" disabled={submitting}>
              {submitting ? <LoadingSpinner /> : form.id ? "Save changes" : "Add unit"}
            </button>
          </form>
        </Modal>
      )}
    </div>
  );
}
