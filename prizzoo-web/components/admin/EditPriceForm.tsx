"use client";

import { FormEvent, useEffect, useState } from "react";
import { getPrices, updatePrice } from "@/lib/api/priceSubmission";
import { ApiError } from "@/lib/api/client";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { PriceStatus, type AdminPrice } from "@/lib/api/types";
import styles from "./AdminForm.module.css";

interface EditPriceFormProps {
  priceId: string;
  onSaved: () => void;
}

export function EditPriceForm({ priceId, onSaved }: EditPriceFormProps) {
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [price, setPrice] = useState<AdminPrice | null>(null);

  const [amount, setAmount] = useState("");
  const [originalAmount, setOriginalAmount] = useState("");
  const [status, setStatus] = useState<PriceStatus>(PriceStatus.Approved);

  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    // No single-price Get endpoint - the list endpoint already carries every
    // field this form needs, filtered down by id client-side.
    getPrices()
      .then((result) => {
        const found = result.items.find((p) => p.id === priceId);
        if (!found) {
          setLoadError("Price not found.");
          return;
        }
        setPrice(found);
        setAmount(String(found.amount));
        setOriginalAmount(found.originalAmount != null ? String(found.originalAmount) : "");
        setStatus(found.status);
      })
      .catch((err) => setLoadError(err instanceof ApiError ? err.message : "Could not load this price."))
      .finally(() => setLoading(false));
  }, [priceId]);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await updatePrice({
        id: priceId,
        amount: Number(amount),
        originalAmount: originalAmount ? Number(originalAmount) : undefined,
        status,
      });
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not save this price.");
    } finally {
      setSubmitting(false);
    }
  }

  if (loading) {
    return <LoadingSpinner />;
  }

  if (loadError || !price) {
    return <ErrorBanner message={loadError ?? "Price not found."} />;
  }

  return (
    <form className={styles.form} onSubmit={handleSubmit}>
      <p className={styles.body}>
        {price.productName} at {price.storeName}
      </p>
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
      <label className={styles.label}>
        Status
        <select
          className={styles.field}
          value={status}
          onChange={(e) => setStatus(Number(e.target.value) as PriceStatus)}
        >
          <option value={PriceStatus.Pending}>Pending</option>
          <option value={PriceStatus.Approved}>Approved</option>
          <option value={PriceStatus.Flagged}>Flagged</option>
          <option value={PriceStatus.Rejected}>Rejected</option>
        </select>
      </label>

      {error && <ErrorBanner message={error} />}

      <button className={styles.submit} type="submit" disabled={submitting}>
        {submitting ? <LoadingSpinner /> : "Save changes"}
      </button>
    </form>
  );
}
