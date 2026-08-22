"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { deleteFlyer, getRecentFlyers } from "@/lib/api/flyer";
import { imageUrl } from "@/lib/api/image";
import type { FlyerDetail } from "@/lib/api/types";
import { ApiError } from "@/lib/api/client";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { EmptyState } from "@/components/common/EmptyState";
import styles from "./FlyerList.module.css";

// Admin-side read-only view of uploaded flyers, for checking a flyer's
// photo and mapped items after upload. Reuses GetRecentFlyersAsync (the
// same endpoint the shopper home screen's carousel calls) rather than a
// separate admin-only endpoint - the Admin role already carries every
// Shopper permission (see TenantRoleAndUserBuilder), and this is exactly
// "every flyer, newest first" already. Capped at the same 20 most recent
// flyers as the shopper carousel - fine for MVP scale.
export function FlyerList() {
  const router = useRouter();
  const { isAuthenticated, isAdmin, isReady } = useAuth();
  const [flyers, setFlyers] = useState<FlyerDetail[] | null>(null);
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

  function load() {
    setLoading(true);
    getRecentFlyers()
      .then(setFlyers)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Something went wrong."))
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    if (!isReady || !isAuthenticated || !isAdmin) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect
    load();
  }, [isReady, isAuthenticated, isAdmin]);

  async function handleDelete(flyer: FlyerDetail) {
    if (!window.confirm(`Remove this flyer for ${flyer.storeName ?? "this store"}? This cannot be undone.`)) return;
    setActingId(flyer.id);
    setError(null);
    try {
      await deleteFlyer(flyer.id);
      setFlyers((prev) => prev?.filter((f) => f.id !== flyer.id) ?? null);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not remove this flyer.");
    } finally {
      setActingId(null);
    }
  }

  if (!isReady || !isAuthenticated || !isAdmin) {
    return null;
  }

  return (
    <div className={styles.page}>
      <div className={styles.headerRow}>
        <button type="button" className={styles.back} onClick={() => router.back()}>
          ← Back
        </button>
        <button type="button" className={styles.back} onClick={() => router.push("/admin/dashboard")}>
          🏠 Dashboard
        </button>
      </div>

      <div className={styles.topBar}>
        <div>
          <h1 className={styles.heading}>Flyers</h1>
          <p className={styles.subheading}>
            Every flyer uploaded so far, most recent first - check the photo and mapped items here.
          </p>
        </div>
        <a className={styles.addButton} href="/admin/flyers/new">
          + Upload a flyer
        </a>
      </div>

      {error && <ErrorBanner message={error} />}
      {loading && <LoadingSpinner />}
      {!loading && (!flyers || flyers.length === 0) && <EmptyState message="No flyers uploaded yet." />}

      {!loading && flyers && flyers.length > 0 && (
        <div className={styles.tableWrap}>
          <table className={styles.table}>
            <thead>
              <tr>
                <th className={styles.thImage} />
                <th>Store</th>
                <th>Items</th>
                <th className={styles.thActions}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {flyers.map((flyer) => {
                const preview = flyer.items
                  .slice(0, 2)
                  .map((item) => item.productName)
                  .join(", ");
                const remaining = flyer.items.length - 2;

                return (
                  <tr key={flyer.id}>
                    <td className={styles.thImage}>
                      {/* eslint-disable-next-line @next/next/no-img-element */}
                      <img src={imageUrl(flyer.imageId)} alt="" className={styles.thumb} />
                    </td>
                    <td className={styles.nameCell}>{flyer.storeName ?? "Unknown store"}</td>
                    <td className={styles.mutedCell}>
                      {flyer.items.length} item{flyer.items.length === 1 ? "" : "s"}
                      {preview && (
                        <div className={styles.itemsPreview}>
                          {preview}
                          {remaining > 0 ? `, +${remaining} more` : ""}
                        </div>
                      )}
                    </td>
                    <td className={styles.actionsCell}>
                      <button
                        type="button"
                        className={styles.viewButton}
                        onClick={() =>
                          router.push(`/admin/flyers/add-item?flyerId=${flyer.id}&storeId=${flyer.storeId}`)
                        }
                      >
                        Add product
                      </button>
                      <button
                        type="button"
                        className={styles.viewButton}
                        onClick={() => router.push(`/store-flyer?storeId=${flyer.storeId}`)}
                      >
                        View
                      </button>
                      <button
                        type="button"
                        className={styles.deleteButton}
                        disabled={actingId === flyer.id}
                        onClick={() => handleDelete(flyer)}
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
