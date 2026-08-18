"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { getMyProductsAndPrices, getMyStore } from "@/lib/api/shopOwner";
import { imageUrl } from "@/lib/api/image";
import { PriceStatus, type MyStore, type MyStoreProduct } from "@/lib/api/types";
import { ApiError } from "@/lib/api/client";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { EmptyState } from "@/components/common/EmptyState";
import styles from "./ShopOwnerDashboard.module.css";

const STATUS_LABEL: Record<PriceStatus, string> = {
  [PriceStatus.Pending]: "Pending review",
  [PriceStatus.Approved]: "Live",
  [PriceStatus.Flagged]: "Flagged",
  [PriceStatus.Rejected]: "Rejected",
};

export function ShopOwnerDashboard() {
  const router = useRouter();
  const { isAuthenticated, isReady, logout } = useAuth();
  const [store, setStore] = useState<MyStore | null>(null);
  const [products, setProducts] = useState<MyStoreProduct[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/login");
    }
  }, [isReady, isAuthenticated, router]);

  function handleLogout() {
    logout();
    router.replace("/login");
  }

  useEffect(() => {
    if (!isReady || !isAuthenticated) return;

    Promise.all([getMyStore(), getMyProductsAndPrices()])
      .then(([storeResult, productsResult]) => {
        setStore(storeResult);
        setProducts(productsResult);
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : "Something went wrong."))
      .finally(() => setLoading(false));
  }, [isReady, isAuthenticated]);

  if (!isReady || !isAuthenticated) {
    return null;
  }

  if (loading) {
    return (
      <div className={styles.root}>
        <LoadingSpinner />
      </div>
    );
  }

  if (error || !store) {
    return (
      <div className={styles.root}>
        <ErrorBanner message={error ?? "Could not load your shop."} />
      </div>
    );
  }

  return (
    <div className={styles.root}>
      <div className={styles.topBar}>
        <h1 className={styles.heading}>{store.name}</h1>
        <button type="button" className={styles.logoutButton} onClick={handleLogout}>
          Logout
        </button>
      </div>
      <div className={styles.card}>
        {store.address && <div className={styles.meta}>{store.address}</div>}
        {store.city && <div className={styles.meta}>{store.city}</div>}
        {store.phone && <div className={styles.meta}>{store.phone}</div>}
        {store.categoryTags && <div className={styles.meta}>{store.categoryTags}</div>}
      </div>

      <a className={styles.addLink} href="/shop-owner/products/new">
        Add a product
      </a>
      <a className={styles.changePasswordLink} href="/shop-owner/change-password">
        Change password
      </a>

      <h2 className={styles.subheading}>My products</h2>
      {!products || products.length === 0 ? (
        <EmptyState message="You haven't added any products yet." />
      ) : (
        <div className={styles.list}>
          {products.map((product) => (
            <div key={product.productId} className={styles.productCard}>
              {product.imageId && (
                // eslint-disable-next-line @next/next/no-img-element
                <img src={imageUrl(product.imageId)} alt="" className={styles.thumb} />
              )}
              <div className={styles.info}>
                <div className={styles.name}>{product.productName}</div>
                <div className={styles.meta}>
                  {product.latestPriceAmount != null
                    ? `₹${product.latestPriceAmount.toFixed(2)}`
                    : "No price yet"}
                </div>
              </div>
              {product.latestPriceStatus != null && (
                <span className={styles.badge}>{STATUS_LABEL[product.latestPriceStatus]}</span>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
