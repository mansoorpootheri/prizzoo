"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { deletePrice, getPrices } from "@/lib/api/priceSubmission";
import { getStores } from "@/lib/api/adminStores";
import type { AdminPrice, AdminStoreDetail } from "@/lib/api/types";
import { PriceStatus } from "@/lib/api/types";
import { ApiError } from "@/lib/api/client";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { EmptyState } from "@/components/common/EmptyState";
import { Modal } from "@/components/common/Modal";
import { AddStorePriceForm } from "./AddStorePriceForm";
import { EditPriceForm } from "./EditPriceForm";
import styles from "./PriceList.module.css";

type ModalState = { mode: "add"; productId?: string } | { mode: "edit"; id: string } | null;

const STATUS_LABEL: Record<PriceStatus, string> = {
  [PriceStatus.Pending]: "Pending",
  [PriceStatus.Approved]: "Approved",
  [PriceStatus.Flagged]: "Flagged",
  [PriceStatus.Rejected]: "Rejected",
};

const STATUS_BADGE: Record<PriceStatus, string> = {
  [PriceStatus.Pending]: "badgePending",
  [PriceStatus.Approved]: "badgeApproved",
  [PriceStatus.Flagged]: "badgeFlagged",
  [PriceStatus.Rejected]: "badgeRejected",
};

// ERP-style "which store sells this at what price" view - every Price row
// ever recorded, any status, not just the pending-moderation queue.
export function PriceList() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { isAuthenticated, isAdmin, isReady } = useAuth();
  const [prices, setPrices] = useState<AdminPrice[] | null>(null);
  const [stores, setStores] = useState<AdminStoreDetail[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [keyword, setKeyword] = useState("");
  const [storeId, setStoreId] = useState("");
  const [status, setStatus] = useState<string>("");
  const [actingId, setActingId] = useState<string | null>(null);
  // Arriving from the product list's "Add price" action (see
  // ProductList.tsx) puts ?productId= in the URL - initialize the modal
  // already open and pre-filled with that product, computed once from the
  // param present on first render rather than via an effect.
  const [modal, setModal] = useState<ModalState>(() => {
    const preselectedProductId = searchParams.get("productId");
    return preselectedProductId ? { mode: "add", productId: preselectedProductId } : null;
  });

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/phone-entry");
    } else if (isReady && isAuthenticated && !isAdmin) {
      router.replace("/home");
    }
  }, [isReady, isAuthenticated, isAdmin, router]);

  function load(filters?: { keyword?: string; storeId?: string; status?: string }) {
    setLoading(true);
    setError(null);
    getPrices({
      keyword: filters?.keyword || undefined,
      storeId: filters?.storeId || undefined,
      status: filters?.status ? (Number(filters.status) as PriceStatus) : undefined,
    })
      .then((result) => setPrices(result.items))
      .catch((err) => setError(err instanceof ApiError ? err.message : "Something went wrong."))
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    if (!isReady || !isAuthenticated) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect
    load();
    getStores()
      .then((result) => setStores(result.items))
      .catch(() => undefined);
  }, [isReady, isAuthenticated]);

  function handleFilter(event: FormEvent) {
    event.preventDefault();
    load({ keyword, storeId, status });
  }

  function handleAddSaved() {
    // Doesn't close the modal - see AddStorePriceForm's doc comment.
    load({ keyword, storeId, status });
  }

  function handleEditSaved() {
    setModal(null);
    load({ keyword, storeId, status });
  }

  async function handleDelete(price: AdminPrice) {
    if (!window.confirm(`Delete the ${price.amount} price for "${price.productName}" at ${price.storeName}?`)) {
      return;
    }
    setActingId(price.id);
    setError(null);
    try {
      await deletePrice(price.id);
      setPrices((prev) => prev?.filter((p) => p.id !== price.id) ?? null);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not delete this price.");
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
          <h1 className={styles.heading}>Store prices</h1>
          <p className={styles.subheading}>Every price on record - which store sells what, at what price.</p>
        </div>
        <button type="button" className={styles.addButton} onClick={() => setModal({ mode: "add" })}>
          + Add price
        </button>
      </div>

      <form className={styles.filterRow} onSubmit={handleFilter}>
        <input
          className={styles.searchField}
          placeholder="Search by product or store"
          value={keyword}
          onChange={(e) => setKeyword(e.target.value)}
        />
        <select className={styles.filterField} value={storeId} onChange={(e) => setStoreId(e.target.value)}>
          <option value="">All stores</option>
          {stores.map((s) => (
            <option key={s.id} value={s.id}>
              {s.name}
            </option>
          ))}
        </select>
        <select className={styles.filterField} value={status} onChange={(e) => setStatus(e.target.value)}>
          <option value="">All statuses</option>
          <option value={PriceStatus.Pending}>Pending</option>
          <option value={PriceStatus.Approved}>Approved</option>
          <option value={PriceStatus.Flagged}>Flagged</option>
          <option value={PriceStatus.Rejected}>Rejected</option>
        </select>
        <button type="submit" className={styles.secondary}>
          Filter
        </button>
      </form>

      {error && <ErrorBanner message={error} />}
      {loading && <LoadingSpinner />}
      {!loading && (!prices || prices.length === 0) && <EmptyState message="No prices yet." />}

      {!loading && prices && prices.length > 0 && (
        <div className={styles.tableWrap}>
          <table className={styles.table}>
            <thead>
              <tr>
                <th>Product</th>
                <th>Store</th>
                <th>Price</th>
                <th>MRP</th>
                <th>Status</th>
                <th>Updated</th>
                <th className={styles.thActions}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {prices.map((price) => (
                <tr key={price.id}>
                  <td className={styles.nameCell}>{price.productName}</td>
                  <td className={styles.mutedCell}>{price.storeName}</td>
                  <td className={styles.nameCell}>₹{price.amount.toFixed(2)}</td>
                  <td className={styles.mutedCell}>
                    {price.originalAmount ? `₹${price.originalAmount.toFixed(2)}` : "—"}
                  </td>
                  <td>
                    <span className={styles[STATUS_BADGE[price.status]]}>{STATUS_LABEL[price.status]}</span>
                  </td>
                  <td className={styles.mutedCell}>{new Date(price.observedAt).toLocaleDateString()}</td>
                  <td className={styles.actionsCell}>
                    <button
                      type="button"
                      className={styles.editButton}
                      onClick={() => setModal({ mode: "edit", id: price.id })}
                    >
                      Edit
                    </button>
                    <button
                      type="button"
                      className={styles.deleteButton}
                      disabled={actingId === price.id}
                      onClick={() => handleDelete(price)}
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {modal?.mode === "add" && (
        <Modal title="Add a store price" onClose={() => setModal(null)}>
          <AddStorePriceForm productId={modal.productId} onSaved={handleAddSaved} />
        </Modal>
      )}
      {modal?.mode === "edit" && (
        <Modal title="Edit price" onClose={() => setModal(null)}>
          <EditPriceForm priceId={modal.id} onSaved={handleEditSaved} />
        </Modal>
      )}
    </div>
  );
}
