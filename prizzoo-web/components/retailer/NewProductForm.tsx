"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { createMyProduct, getCategoriesForDropdown, getUnitsForDropdown } from "@/lib/api/retailer";
import type { ComboboxItem } from "@/lib/api/types";
import { ApiError } from "@/lib/api/client";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ImageUploadField } from "@/components/common/ImageUploadField";
import styles from "./RetailerForm.module.css";

export function NewProductForm() {
  const router = useRouter();
  const [categories, setCategories] = useState<ComboboxItem[]>([]);
  const [units, setUnits] = useState<ComboboxItem[]>([]);
  const [name, setName] = useState("");
  const [barcode, setBarcode] = useState("");
  const [description, setDescription] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [unitId, setUnitId] = useState("");
  const [imageId, setImageId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    Promise.all([getCategoriesForDropdown(), getUnitsForDropdown()])
      .then(([cats, unitList]) => {
        setCategories(cats);
        setUnits(unitList);
      })
      .catch(() => {
        // Dropdowns staying empty just means free-text-only submission;
        // not worth blocking the whole form over.
      });
  }, []);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await createMyProduct({
        name,
        barcode: barcode || undefined,
        description: description || undefined,
        categoryId: categoryId || undefined,
        unitId: unitId || undefined,
        imageId: imageId ?? undefined,
      });
      router.replace("/retailer/products");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not add this product.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className={styles.root}>
      <button type="button" className={styles.back} onClick={() => router.back()}>
        ← Back
      </button>
      <h1 className={styles.heading}>Add a product</h1>
      <p className={styles.subheading}>
        New products are reviewed before they appear in search.
      </p>

      <form className={styles.form} onSubmit={handleSubmit}>
        <label className={styles.label}>
          Product name
          <input
            className={styles.field}
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
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
        <label className={styles.label}>
          Unit
          <select
            className={styles.field}
            value={unitId}
            onChange={(e) => setUnitId(e.target.value)}
          >
            <option value="">Select a unit</option>
            {units.map((u) => (
              <option key={u.value} value={u.value}>
                {u.displayText}
              </option>
            ))}
          </select>
        </label>
        <label className={styles.label}>
          Barcode
          <input
            className={styles.field}
            value={barcode}
            onChange={(e) => setBarcode(e.target.value)}
          />
        </label>
        <label className={styles.label}>
          Description
          <input
            className={styles.field}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
        </label>
        <ImageUploadField label="Product photo" value={imageId} onChange={setImageId} />

        {error && <ErrorBanner message={error} />}

        <button className={styles.submit} type="submit" disabled={submitting}>
          {submitting ? <LoadingSpinner /> : "Add product"}
        </button>
      </form>
    </div>
  );
}
