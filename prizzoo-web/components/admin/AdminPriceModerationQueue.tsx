"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { getPendingPrices, moderatePrice } from "@/lib/api/priceSubmission";
import { PriceStatus, type PendingPrice } from "@/lib/api/types";
import { ApiError } from "@/lib/api/client";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { EmptyState } from "@/components/common/EmptyState";
import styles from "./AdminModerationQueue.module.css";

export function AdminPriceModerationQueue() {
  const router = useRouter();
  const { isAuthenticated, isAdmin, isReady } = useAuth();
  const [prices, setPrices] = useState<PendingPrice[] | null>(null);
  const [notes, setNotes] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [actingId, setActingId] = useState<string | null>(null);

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/phone-entry");
    } else if (isReady && isAuthenticated && !isAdmin) {
      router.replace("/home");
    }
  }, [isReady, isAuthenticated, isAdmin, router]);

  useEffect(() => {
    if (!isReady || !isAuthenticated) return;
    getPendingPrices()
      .then(setPrices)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Something went wrong."))
      .finally(() => setLoading(false));
  }, [isReady, isAuthenticated]);

  async function handleModerate(id: string, status: PriceStatus) {
    setActingId(id);
    try {
      await moderatePrice(id, status, notes[id] || undefined);
      setPrices((prev) => prev?.filter((p) => p.id !== id) ?? null);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not update this price.");
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
      <h1 className={styles.heading}>Price submissions</h1>
      <p className={styles.subheading}>
        Approve, flag, or reject prices submitted for review.
      </p>

      {loading && <LoadingSpinner />}
      {error && <ErrorBanner message={error} />}

      {!loading && !error && (
        <>
          {!prices || prices.length === 0 ? (
            <EmptyState message="No prices waiting for review." />
          ) : (
            <div className={styles.list}>
              {prices.map((price) => (
                <div key={price.id} className={styles.card}>
                  <div className={styles.title}>{price.productName}</div>
                  <div className={styles.meta}>
                    {price.storeName} · {price.amount.toFixed(2)}
                  </div>
                  <input
                    className={styles.noteField}
                    placeholder="Moderation note (shown to shop owner on reject)"
                    value={notes[price.id] ?? ""}
                    onChange={(e) =>
                      setNotes((prev) => ({ ...prev, [price.id]: e.target.value }))
                    }
                  />
                  <div className={styles.actions}>
                    <button
                      type="button"
                      className={styles.approve}
                      disabled={actingId === price.id}
                      onClick={() => handleModerate(price.id, PriceStatus.Approved)}
                    >
                      Approve
                    </button>
                    <button
                      type="button"
                      className={styles.flag}
                      disabled={actingId === price.id}
                      onClick={() => handleModerate(price.id, PriceStatus.Flagged)}
                    >
                      Flag
                    </button>
                    <button
                      type="button"
                      className={styles.reject}
                      disabled={actingId === price.id}
                      onClick={() => handleModerate(price.id, PriceStatus.Rejected)}
                    >
                      Reject
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </>
      )}
    </div>
  );
}
