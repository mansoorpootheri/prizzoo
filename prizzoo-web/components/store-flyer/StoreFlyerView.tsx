"use client";

import { Suspense, useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { getFlyersForStore } from "@/lib/api/flyer";
import { imageUrl } from "@/lib/api/image";
import type { FlyerDetail } from "@/lib/api/types";
import { ApiError } from "@/lib/api/client";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { EmptyState } from "@/components/common/EmptyState";
import styles from "./StoreFlyerView.module.css";

function StoreFlyerViewContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const storeId = searchParams.get("storeId") ?? "";
  const { isAuthenticated, isReady } = useAuth();

  const [flyers, setFlyers] = useState<FlyerDetail[] | null>(null);
  const [index, setIndex] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/phone-entry");
    }
  }, [isReady, isAuthenticated, router]);

  useEffect(() => {
    if (!isReady || !isAuthenticated || !storeId) return;
    getFlyersForStore(storeId)
      .then((result) => {
        setFlyers(result);
        setIndex(0); // newest flyer shown first
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : "Something went wrong."))
      .finally(() => setLoading(false));
  }, [isReady, isAuthenticated, storeId]);

  function goToProduct(productName: string) {
    router.push(`/product?keyword=${encodeURIComponent(productName)}`);
  }

  if (!isReady || !isAuthenticated) {
    return null;
  }

  const flyer = flyers && flyers.length > 0 ? flyers[index] : null;

  return (
    <div className={styles.root}>
      <button type="button" className={styles.back} onClick={() => router.back()}>
        ← Back
      </button>

      {loading && <LoadingSpinner />}
      {error && <ErrorBanner message={error} />}

      {!loading && !error && (!flyers || flyers.length === 0) && (
        <EmptyState message="This store hasn't shared a flyer yet." />
      )}

      {!loading && flyer && flyers && (
        <>
          <h1 className={styles.heading}>{flyer.storeName}</h1>

          <div className={styles.carousel}>
            {flyers.length > 1 && (
              <button
                type="button"
                className={`${styles.navButton} ${styles.navPrev}`}
                onClick={() => setIndex((i) => (i - 1 + flyers.length) % flyers.length)}
                aria-label="Previous flyer"
              >
                ‹
              </button>
            )}
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={imageUrl(flyer.imageId)} alt="" className={styles.flyerImage} />
            {flyers.length > 1 && (
              <button
                type="button"
                className={`${styles.navButton} ${styles.navNext}`}
                onClick={() => setIndex((i) => (i + 1) % flyers.length)}
                aria-label="Next flyer"
              >
                ›
              </button>
            )}
          </div>

          {flyers.length > 1 && (
            <div className={styles.dots}>
              {flyers.map((f, i) => (
                <button
                  key={f.id}
                  type="button"
                  className={`${styles.dot} ${i === index ? styles.dotActive : ""}`}
                  onClick={() => setIndex(i)}
                  aria-label={`Flyer ${i + 1} of ${flyers.length}`}
                />
              ))}
            </div>
          )}

          {flyer.items.length === 0 ? (
            <p className={styles.note}>No items listed for this flyer yet.</p>
          ) : (
            <div className={styles.list}>
              {flyer.items.map((item, i) => (
                <button
                  key={i}
                  type="button"
                  className={styles.itemCard}
                  onClick={() => goToProduct(item.productName)}
                >
                  <span className={styles.itemName}>{item.productName}</span>
                  <span className={styles.priceGroup}>
                    {item.originalAmount != null && item.originalAmount > item.amount && (
                      <span className={styles.originalAmount}>₹{item.originalAmount.toFixed(2)}</span>
                    )}
                    <span className={styles.amount}>₹{item.amount.toFixed(2)}</span>
                  </span>
                </button>
              ))}
            </div>
          )}
        </>
      )}
    </div>
  );
}

export function StoreFlyerView() {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <StoreFlyerViewContent />
    </Suspense>
  );
}
