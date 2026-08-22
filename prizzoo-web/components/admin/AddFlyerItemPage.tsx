"use client";

import { Suspense, useCallback, useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { getFlyersForStore } from "@/lib/api/flyer";
import { imageUrl } from "@/lib/api/image";
import type { FlyerDetail } from "@/lib/api/types";
import { ApiError } from "@/lib/api/client";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { AddFlyerItemsForm } from "./AddFlyerItemsForm";
import layoutStyles from "./UploadFlyerForStoreForm.module.css";
import styles from "./AddFlyerItemPage.module.css";

// flyerId/storeId as query params, not a dynamic route segment - same
// static-export reasoning as /product?keyword= and /store-flyer?storeId=
// (see CLAUDE.md). A full page (not a Modal) so the flyer's photo can sit
// alongside the form, the same "photo left, items right" split the
// original upload form (UploadFlyerForStoreForm) already uses.
function AddFlyerItemPageContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const flyerId = searchParams.get("flyerId") ?? "";
  const storeId = searchParams.get("storeId") ?? "";
  const { isAuthenticated, isAdmin, isReady } = useAuth();

  const [flyer, setFlyer] = useState<FlyerDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/phone-entry");
    } else if (isReady && isAuthenticated && !isAdmin) {
      router.replace("/home");
    }
  }, [isReady, isAuthenticated, isAdmin, router]);

  const load = useCallback(() => {
    if (!storeId || !flyerId) {
      setError("Missing flyer.");
      setLoading(false);
      return;
    }
    setLoading(true);
    getFlyersForStore(storeId)
      .then((list) => {
        const match = list.find((f) => f.id === flyerId);
        setFlyer(match ?? null);
        setError(match ? null : "Flyer not found.");
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : "Something went wrong."))
      .finally(() => setLoading(false));
  }, [storeId, flyerId]);

  useEffect(() => {
    if (!isReady || !isAuthenticated || !isAdmin) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect
    load();
  }, [isReady, isAuthenticated, isAdmin, load]);

  if (!isReady || !isAuthenticated || !isAdmin) {
    return null;
  }

  return (
    <div className={layoutStyles.page}>
      <div className={layoutStyles.headerRow}>
        <button type="button" className={layoutStyles.back} onClick={() => router.push("/admin/flyers")}>
          ← Back to flyers
        </button>
        <button type="button" className={layoutStyles.back} onClick={() => router.push("/admin/dashboard")}>
          🏠 Dashboard
        </button>
      </div>
      <h1 className={layoutStyles.heading}>Add product to flyer</h1>
      <p className={layoutStyles.subheading}>
        {flyer?.storeName ? `For ${flyer.storeName}'s uploaded flyer.` : "Pick a product and set its price."}
      </p>

      {loading && <LoadingSpinner />}
      {error && <ErrorBanner message={error} />}

      {!loading && flyer && (
        <div className={layoutStyles.layout}>
          <div className={layoutStyles.left}>
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={imageUrl(flyer.imageId)} alt="" className={layoutStyles.preview} />

            {flyer.items.length > 0 && (
              <>
                <span className={styles.existingHeading}>Already on this flyer</span>
                <div className={styles.existingList}>
                  {flyer.items.map((item, i) => (
                    <div key={i} className={styles.existingItem}>
                      <span className={styles.existingName}>{item.productName}</span>
                      <span className={styles.existingPrice}>₹{item.amount.toFixed(2)}</span>
                    </div>
                  ))}
                </div>
              </>
            )}
          </div>

          <div className={layoutStyles.right}>
            <AddFlyerItemsForm flyerId={flyerId} storeId={storeId} onSaved={load} />
          </div>
        </div>
      )}
    </div>
  );
}

export function AddFlyerItemPage() {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <AddFlyerItemPageContent />
    </Suspense>
  );
}
