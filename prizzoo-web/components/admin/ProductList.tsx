"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { deleteProduct, getProducts } from "@/lib/api/product";
import type { AdminProduct } from "@/lib/api/types";
import { imageUrl } from "@/lib/api/image";
import { ApiError } from "@/lib/api/client";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { EmptyState } from "@/components/common/EmptyState";
import { Modal } from "@/components/common/Modal";
import { CreateProductForm } from "./CreateProductForm";
import { EditProductForm } from "./EditProductForm";
import styles from "./ProductList.module.css";

type ModalState = { mode: "add" } | { mode: "edit"; id: string } | null;

// ERP-style catalog view: a scannable table rather than stacked cards,
// since the product list is expected to grow large. Add/edit open as a
// modal dialog on this same page rather than a separate route.
export function ProductList() {
  const router = useRouter();
  const { isAuthenticated, isAdmin, isReady } = useAuth();
  const [products, setProducts] = useState<AdminProduct[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [keyword, setKeyword] = useState("");
  const [actingId, setActingId] = useState<string | null>(null);
  const [modal, setModal] = useState<ModalState>(null);

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/phone-entry");
    } else if (isReady && isAuthenticated && !isAdmin) {
      router.replace("/home");
    }
  }, [isReady, isAuthenticated, isAdmin, router]);

  function load(searchKeyword?: string) {
    setLoading(true);
    setError(null);
    getProducts(searchKeyword)
      .then((result) => setProducts(result.items))
      .catch((err) => setError(err instanceof ApiError ? err.message : "Something went wrong."))
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    if (!isReady || !isAuthenticated) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect
    load();
  }, [isReady, isAuthenticated]);

  function handleSearch(event: FormEvent) {
    event.preventDefault();
    load(keyword || undefined);
  }

  function handleSaved() {
    setModal(null);
    load(keyword || undefined);
  }

  async function handleDelete(product: AdminProduct) {
    if (!window.confirm(`Delete "${product.name}"? This cannot be undone.`)) return;
    setActingId(product.id);
    setError(null);
    try {
      await deleteProduct(product.id);
      setProducts((prev) => prev?.filter((p) => p.id !== product.id) ?? null);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not delete this product.");
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
          <h1 className={styles.heading}>Products</h1>
          <p className={styles.subheading}>
            Shared public catalog - shows up in search/compare once a price is submitted for it.
          </p>
        </div>
        <button type="button" className={styles.addButton} onClick={() => setModal({ mode: "add" })}>
          + Add product
        </button>
      </div>

      <form className={styles.searchRow} onSubmit={handleSearch}>
        <input
          className={styles.searchField}
          placeholder="Search by name or barcode"
          value={keyword}
          onChange={(e) => setKeyword(e.target.value)}
        />
        <button type="submit" className={styles.secondary}>
          Search
        </button>
      </form>

      {error && <ErrorBanner message={error} />}
      {loading && <LoadingSpinner />}
      {!loading && (!products || products.length === 0) && <EmptyState message="No products yet." />}

      {!loading && products && products.length > 0 && (
        <div className={styles.tableWrap}>
          <table className={styles.table}>
            <thead>
              <tr>
                <th className={styles.thImage} />
                <th>Name</th>
                <th>Barcode</th>
                <th>Category</th>
                <th>Unit</th>
                <th>Status</th>
                <th className={styles.thActions}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {products.map((product) => (
                <tr key={product.id}>
                  <td className={styles.thImage}>
                    {product.imageId ? (
                      // eslint-disable-next-line @next/next/no-img-element
                      <img src={imageUrl(product.imageId)} alt="" className={styles.thumb} />
                    ) : (
                      <div className={styles.thumbPlaceholder} aria-hidden="true" />
                    )}
                  </td>
                  <td className={styles.nameCell}>{product.name}</td>
                  <td className={styles.mutedCell}>{product.barcode ?? "—"}</td>
                  <td className={styles.mutedCell}>{product.categoryName ?? "—"}</td>
                  <td className={styles.mutedCell}>{product.unitName ?? "—"}</td>
                  <td>
                    <span className={product.isActive ? styles.badgeActive : styles.badgeInactive}>
                      {product.isActive ? "Active" : "Inactive"}
                    </span>
                  </td>
                  <td className={styles.actionsCell}>
                    <button
                      type="button"
                      className={styles.editButton}
                      onClick={() => router.push(`/admin/prices?productId=${product.id}`)}
                    >
                      Add price
                    </button>
                    <button
                      type="button"
                      className={styles.editButton}
                      onClick={() => setModal({ mode: "edit", id: product.id })}
                    >
                      Edit
                    </button>
                    <button
                      type="button"
                      className={styles.deleteButton}
                      disabled={actingId === product.id}
                      onClick={() => handleDelete(product)}
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
        <Modal title="Add a product" onClose={() => setModal(null)}>
          <CreateProductForm onSaved={handleSaved} />
        </Modal>
      )}
      {modal?.mode === "edit" && (
        <Modal title="Edit product" onClose={() => setModal(null)}>
          <EditProductForm productId={modal.id} onSaved={handleSaved} />
        </Modal>
      )}
    </div>
  );
}
