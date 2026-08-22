"use client";

import { FormEvent, useEffect, useState } from "react";
import { getProduct, updateProduct } from "@/lib/api/product";
import { getCategoriesForCombobox, getUnitsForCombobox } from "@/lib/api/adminCatalog";
import { ApiError } from "@/lib/api/client";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ImageUploadField } from "@/components/common/ImageUploadField";
import type { ComboboxItem } from "@/lib/api/types";
import styles from "./AdminForm.module.css";

interface EditProductFormProps {
  productId: string;
  onSaved: () => void;
}

export function EditProductForm({ productId, onSaved }: EditProductFormProps) {
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);

  const [name, setName] = useState("");
  const [barcode, setBarcode] = useState("");
  const [description, setDescription] = useState("");
  const [categories, setCategories] = useState<ComboboxItem[]>([]);
  const [categoryId, setCategoryId] = useState("");
  const [units, setUnits] = useState<ComboboxItem[]>([]);
  const [unitId, setUnitId] = useState("");
  const [imageId, setImageId] = useState<string | null>(null);
  const [isActive, setIsActive] = useState(true);

  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    Promise.all([getProduct(productId), getCategoriesForCombobox(), getUnitsForCombobox()])
      .then(([product, categoryList, unitList]) => {
        setName(product.name);
        setBarcode(product.barcode ?? "");
        setDescription(product.description ?? "");
        setImageId(product.imageId);
        setIsActive(product.isActive);
        setCategories(categoryList);
        setCategoryId(product.categoryId ?? "");
        setUnits(unitList);
        setUnitId(product.unitId ?? "");
      })
      .catch((err) => setLoadError(err instanceof ApiError ? err.message : "Could not load this product."))
      .finally(() => setLoading(false));
  }, [productId]);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await updateProduct({
        id: productId,
        name,
        barcode: barcode || undefined,
        description: description || undefined,
        categoryId: categoryId || undefined,
        unitId: unitId || undefined,
        imageId: imageId ?? undefined,
        isActive,
      });
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not save this product.");
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
      <label className={styles.label}>
        Name
        <input className={styles.field} value={name} onChange={(e) => setName(e.target.value)} required />
      </label>
      <label className={styles.label}>
        Barcode
        <input className={styles.field} value={barcode} onChange={(e) => setBarcode(e.target.value)} />
      </label>
      <label className={styles.label}>
        Description
        <input className={styles.field} value={description} onChange={(e) => setDescription(e.target.value)} />
      </label>
      <label className={styles.label}>
        Category
        <select className={styles.field} value={categoryId} onChange={(e) => setCategoryId(e.target.value)}>
          <option value="">No category</option>
          {categories.map((c) => (
            <option key={c.value} value={c.value}>
              {c.displayText}
            </option>
          ))}
        </select>
      </label>
      <label className={styles.label}>
        Unit
        <select className={styles.field} value={unitId} onChange={(e) => setUnitId(e.target.value)}>
          <option value="">No unit</option>
          {units.map((u) => (
            <option key={u.value} value={u.value}>
              {u.displayText}
            </option>
          ))}
        </select>
      </label>
      <ImageUploadField label="Product image" value={imageId} onChange={setImageId} />
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
