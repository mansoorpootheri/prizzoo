"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { createOrEditCategory, deleteCategory, getCategories } from "@/lib/api/adminCatalog";
import type { AdminCategory } from "@/lib/api/types";
import { ApiError } from "@/lib/api/client";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { EmptyState } from "@/components/common/EmptyState";
import { Modal } from "@/components/common/Modal";
import styles from "./CategoryMaster.module.css";

interface CategoryFormState {
  id?: string;
  name: string;
  code: string;
  description: string;
  isActive: boolean;
}

const EMPTY_FORM: CategoryFormState = {
  id: undefined,
  name: "",
  code: "",
  description: "",
  isActive: true,
};

export function CategoryMaster() {
  const router = useRouter();
  const { isAuthenticated, isAdmin, isReady } = useAuth();
  const [categories, setCategories] = useState<AdminCategory[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState<CategoryFormState>(EMPTY_FORM);
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
    getCategories()
      .then(setCategories)
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

  function openEdit(category: AdminCategory) {
    setForm({
      id: category.id,
      name: category.name,
      code: category.code ?? "",
      description: category.description ?? "",
      isActive: category.isActive,
    });
    setModalOpen(true);
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await createOrEditCategory({
        id: form.id,
        name: form.name,
        code: form.code || undefined,
        description: form.description || undefined,
        isActive: form.isActive,
      });
      setModalOpen(false);
      load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not save this category.");
    } finally {
      setSubmitting(false);
    }
  }

  async function handleDelete(id: string) {
    setActingId(id);
    setError(null);
    try {
      await deleteCategory(id);
      setCategories((prev) => prev?.filter((c) => c.id !== id) ?? null);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not delete this category.");
    } finally {
      setActingId(null);
    }
  }

  if (!isReady || !isAuthenticated || !isAdmin) {
    return null;
  }

  return (
    <div className={styles.root}>
      <div className={styles.headerRow}>
        <button type="button" className={styles.back} onClick={() => router.back()}>
          ← Back
        </button>
        <button type="button" className={styles.back} onClick={() => router.push("/admin/dashboard")}>
          🏠 Dashboard
        </button>
      </div>

      <div className={styles.topBar}>
        <div className={styles.topBarHeadings}>
          <h1 className={styles.heading}>Category master</h1>
          <p className={styles.subheading}>
            Shared public catalog data, used by every product - not tenant-scoped.
          </p>
        </div>
        <button type="button" className={styles.addButton} onClick={openAdd}>
          + Add category
        </button>
      </div>

      {loading && <LoadingSpinner />}
      {!loading && (!categories || categories.length === 0) && (
        <EmptyState message="No categories yet." />
      )}
      {!loading && categories && categories.length > 0 && (
        <div className={styles.list}>
          {categories.map((category) => (
            <div key={category.id} className={styles.card}>
              <div className={styles.info}>
                <div className={styles.name}>{category.name}</div>
                <div className={styles.meta}>
                  {category.code ?? "No code"}
                  {category.description ? ` · ${category.description}` : ""}
                </div>
              </div>
              <span className={category.isActive ? styles.badgeActive : styles.badgeInactive}>
                {category.isActive ? "Active" : "Inactive"}
              </span>
              <div className={styles.rowActions}>
                <button type="button" className={styles.editButton} onClick={() => openEdit(category)}>
                  Edit
                </button>
                <button
                  type="button"
                  className={styles.deleteButton}
                  disabled={actingId === category.id}
                  onClick={() => handleDelete(category.id)}
                >
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {modalOpen && (
        <Modal title={form.id ? "Edit category" : "Add a category"} onClose={() => setModalOpen(false)}>
          <form className={styles.form} onSubmit={handleSubmit}>
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
              Code
              <input
                className={styles.field}
                value={form.code}
                onChange={(e) => setForm((f) => ({ ...f, code: e.target.value }))}
              />
            </label>
            <label className={styles.label}>
              Description
              <input
                className={styles.field}
                value={form.description}
                onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
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
              {submitting ? <LoadingSpinner /> : form.id ? "Save changes" : "Add category"}
            </button>
          </form>
        </Modal>
      )}
    </div>
  );
}
