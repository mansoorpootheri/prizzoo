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
import { Modal } from "@/components/common/Modal";
import { CreateStoreForm } from "./CreateStoreForm";
import { EditStoreForm } from "./EditStoreForm";
import styles from "./CategoryMaster.module.css";

type ModalState = { mode: "add" } | { mode: "edit"; id: string } | null;

export function StoreList() {
  const router = useRouter();
  const { isAuthenticated, isAdmin, isReady } = useAuth();
  const [stores, setStores] = useState<AdminStoreDetail[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [modal, setModal] = useState<ModalState>(null);

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/phone-entry");
    } else if (isReady && isAuthenticated && !isAdmin) {
      router.replace("/home");
    }
  }, [isReady, isAuthenticated, isAdmin, router]);

  function load() {
    setLoading(true);
    getStores()
      .then((result) => setStores(result.items))
      .catch((err) => setError(err instanceof ApiError ? err.message : "Something went wrong."))
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    if (!isReady || !isAuthenticated) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect
    load();
  }, [isReady, isAuthenticated]);

  function handleSaved() {
    setModal(null);
    load();
  }

  if (!isReady || !isAuthenticated || !isAdmin) {
    return null;
  }

  return (
    <div className={styles.root}>
      <button type="button" className={styles.back} onClick={() => router.back()}>
        ← Back
      </button>

      <div className={styles.topBar}>
        <div className={styles.topBarHeadings}>
          <h1 className={styles.heading}>Stores</h1>
          <p className={styles.subheading}>
            Fix a store&apos;s location, category, or verification status here.
          </p>
        </div>
        <button type="button" className={styles.addButton} onClick={() => setModal({ mode: "add" })}>
          + Add store
        </button>
      </div>

      {loading && <LoadingSpinner />}
      {error && <ErrorBanner message={error} />}
      {!loading && !error && (!stores || stores.length === 0) && (
        <EmptyState message="No stores yet." />
      )}
      {!loading && !error && stores && stores.length > 0 && (
        <div className={styles.list}>
          {stores.map((store) => (
            <button
              key={store.id}
              type="button"
              className={styles.card}
              onClick={() => setModal({ mode: "edit", id: store.id })}
            >
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
            </button>
          ))}
        </div>
      )}

      {modal?.mode === "add" && (
        <Modal title="Create a store" onClose={() => setModal(null)}>
          <CreateStoreForm onSaved={handleSaved} />
        </Modal>
      )}
      {modal?.mode === "edit" && (
        <Modal title="Edit store" onClose={() => setModal(null)}>
          <EditStoreForm storeId={modal.id} onSaved={handleSaved} />
        </Modal>
      )}
    </div>
  );
}
