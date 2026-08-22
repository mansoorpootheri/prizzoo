"use client";

import { FormEvent, useEffect, useState } from "react";
import { createApprovedPrice } from "@/lib/api/priceSubmission";
import { getProductsForCombobox } from "@/lib/api/adminCatalog";
import { getStores } from "@/lib/api/adminStores";
import { ApiError } from "@/lib/api/client";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import type { AdminStoreDetail, ComboboxItem } from "@/lib/api/types";
import styles from "./AdminForm.module.css";

interface AddStorePriceFormProps {
  // Preselects the product when arriving from the product list's
  // "Add price" action - see ProductList.tsx.
  productId?: string;
  // Called after every successful save (does NOT close the modal - an
  // admin adding prices for a product is usually about to add several
  // store prices in a row; only the amount fields reset). The parent
  // should re-run its list load() here.
  onSaved: () => void;
}

export function AddStorePriceForm({ productId, onSaved }: AddStorePriceFormProps) {
  const [products, setProducts] = useState<ComboboxItem[]>([]);
  const [stores, setStores] = useState<AdminStoreDetail[]>([]);
  const [loadingOptions, setLoadingOptions] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);

  const [selectedProductId, setSelectedProductId] = useState(productId ?? "");
  const [storeId, setStoreId] = useState("");
  const [amount, setAmount] = useState("");
  const [originalAmount, setOriginalAmount] = useState("");

  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    Promise.all([getProductsForCombobox(), getStores()])
      .then(([productList, storeResult]) => {
        setProducts(productList);
        setStores(storeResult.items);
      })
      .catch((err) => setLoadError(err instanceof ApiError ? err.message : "Could not load products/stores."))
      .finally(() => setLoadingOptions(false));
  }, []);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setSaved(false);
    setSubmitting(true);
    try {
      await createApprovedPrice({
        productId: selectedProductId,
        storeId,
        amount: Number(amount),
        originalAmount: originalAmount ? Number(originalAmount) : undefined,
      });
      // Stay open - only the amount fields reset; product/store selection
      // carries over so several store prices can be added in a row.
      setAmount("");
      setOriginalAmount("");
      setSaved(true);
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not save this price.");
    } finally {
      setSubmitting(false);
    }
  }

  if (loadingOptions) {
    return <LoadingSpinner />;
  }

  if (loadError) {
    return <ErrorBanner message={loadError} />;
  }

  return (
    <form className={styles.form} onSubmit={handleSubmit}>
      <p className={styles.body}>
        Goes live immediately - no moderation step, since you&apos;re the admin entering it directly.
      </p>
      <label className={styles.label}>
        Product
        <select
          className={styles.field}
          value={selectedProductId}
          onChange={(e) => setSelectedProductId(e.target.value)}
          required
        >
          <option value="" disabled>
            Select a product
          </option>
          {products.map((p) => (
            <option key={p.value} value={p.value}>
              {p.displayText}
            </option>
          ))}
        </select>
      </label>
      <label className={styles.label}>
        Store
        <select className={styles.field} value={storeId} onChange={(e) => setStoreId(e.target.value)} required>
          <option value="" disabled>
            Select a store
          </option>
          {stores.map((s) => (
            <option key={s.id} value={s.id}>
              {s.name}
            </option>
          ))}
        </select>
      </label>
      <label className={styles.label}>
        Selling price (₹)
        <input
          className={styles.field}
          type="number"
          min="0.01"
          step="0.01"
          value={amount}
          onChange={(e) => setAmount(e.target.value)}
          required
        />
      </label>
      <label className={styles.label}>
        MRP / original price (₹) - optional
        <input
          className={styles.field}
          type="number"
          min="0.01"
          step="0.01"
          value={originalAmount}
          onChange={(e) => setOriginalAmount(e.target.value)}
        />
      </label>

      {error && <ErrorBanner message={error} />}
      {saved && !error && <p className={styles.body}>Saved - price is live now. Add another, or close this dialog.</p>}

      <button className={styles.submit} type="submit" disabled={submitting}>
        {submitting ? <LoadingSpinner /> : "Add price"}
      </button>
    </form>
  );
}
