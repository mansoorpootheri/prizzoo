"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { getMyProducts, getMyStore } from "@/lib/api/retailer";
import { submitPrice } from "@/lib/api/priceSubmission";
import type { MyProduct } from "@/lib/api/types";
import { ApiError } from "@/lib/api/client";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import styles from "./RetailerForm.module.css";

export function NewPriceForm() {
  const router = useRouter();
  const [storeId, setStoreId] = useState<string | null>(null);
  const [products, setProducts] = useState<MyProduct[]>([]);
  const [productId, setProductId] = useState("");
  const [amount, setAmount] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    Promise.all([getMyStore(), getMyProducts()])
      .then(([store, myProducts]) => {
        setStoreId(store.id);
        setProducts(myProducts);
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : "Something went wrong."))
      .finally(() => setLoading(false));
  }, []);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!storeId) return;
    setError(null);
    setSubmitting(true);
    try {
      await submitPrice({ productId, storeId, amount: Number(amount) });
      router.replace("/retailer/dashboard");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not submit this price.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className={styles.root}>
      <button type="button" className={styles.back} onClick={() => router.back()}>
        ← Back
      </button>
      <h1 className={styles.heading}>Submit a price</h1>
      <p className={styles.subheading}>Prices are reviewed before they appear in search.</p>

      {loading && <LoadingSpinner />}
      {error && <ErrorBanner message={error} />}

      {!loading && !error && (
        <form className={styles.form} onSubmit={handleSubmit}>
          <label className={styles.label}>
            Product
            <select
              className={styles.field}
              value={productId}
              onChange={(e) => setProductId(e.target.value)}
              required
            >
              <option value="" disabled>
                Select a product
              </option>
              {products.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.name}
                  {p.isActive ? "" : " (pending review)"}
                </option>
              ))}
            </select>
          </label>
          <label className={styles.label}>
            Price (INR)
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

          <button className={styles.submit} type="submit" disabled={submitting || !productId}>
            {submitting ? <LoadingSpinner /> : "Submit price"}
          </button>
        </form>
      )}
    </div>
  );
}
