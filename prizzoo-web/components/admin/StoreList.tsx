"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { getStores } from "@/lib/api/adminStores";
import type { AdminStoreDetail } from "@/lib/api/types";
import { ApiError } from "@/lib/api/client";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { EmptyState } from "@/components/common/EmptyState";
import styles from "./CategoryMaster.module.css";

export function StoreList() {
  const router = useRouter();
  const { isAuthenticated, isReady } = useAuth();
  const [stores, setStores] = useState<AdminStoreDetail[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/login");
    }
  }, [isReady, isAuthenticated, router]);

  useEffect(() => {
    if (!isReady || !isAuthenticated) return;
    getStores()
      .then((result) => setStores(result.items))
      .catch((err) => setError(err instanceof ApiError ? err.message : "Something went wrong."))
      .finally(() => setLoading(false));
  }, [isReady, isAuthenticated]);

  if (!isReady || !isAuthenticated) {
    return null;
  }

  return (
    <div className={styles.root}>
      <button type="button" className={styles.back} onClick={() => router.back()}>
        ← Back
      </button>
      <h1 className={styles.heading}>Stores</h1>
      <p className={styles.subheading}>
        Fix a store&apos;s location, category, or verification status here.
      </p>

      {loading && <LoadingSpinner />}
      {error && <ErrorBanner message={error} />}
      {!loading && !error && (!stores || stores.length === 0) && (
        <EmptyState message="No stores yet." />
      )}
      {!loading && !error && stores && stores.length > 0 && (
        <div className={styles.list}>
          {stores.map((store) => (
            <a key={store.id} className={styles.card} href={`/admin/stores/${store.id}/edit`}>
              <div className={styles.info}>
                <div className={styles.name}>{store.name}</div>
                <div className={styles.meta}>
                  {store.locationName ? `${store.locationName}, ${store.city}` : store.city ?? "No city set"}
                  {store.latitude === 0 && store.longitude === 0 ? " · no coordinates" : ""}
                </div>
              </div>
              <span className={store.isVerified ? styles.badgeActive : styles.badgeInactive}>
                {store.isVerified ? "Verified" : "Unverified"}
              </span>
            </a>
          ))}
        </div>
      )}
    </div>
  );
}
