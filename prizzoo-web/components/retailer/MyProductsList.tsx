"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { getMyProducts } from "@/lib/api/retailer";
import { imageUrl } from "@/lib/api/image";
import type { MyProduct } from "@/lib/api/types";
import { ApiError } from "@/lib/api/client";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { EmptyState } from "@/components/common/EmptyState";
import styles from "./MyProductsList.module.css";

export function MyProductsList() {
  const router = useRouter();
  const { isAuthenticated, isReady } = useAuth();
  const [products, setProducts] = useState<MyProduct[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/login");
    }
  }, [isReady, isAuthenticated, router]);

  useEffect(() => {
    if (!isReady || !isAuthenticated) return;

    getMyProducts()
      .then(setProducts)
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
      <h1 className={styles.heading}>My products</h1>

      {loading && <LoadingSpinner />}
      {error && <ErrorBanner message={error} />}

      {!loading && !error && (
        <>
          {!products || products.length === 0 ? (
            <EmptyState message="You haven't added any products yet." />
          ) : (
            <div className={styles.list}>
              {products.map((product) => (
                <div key={product.id} className={styles.card}>
                  {product.imageId && (
                    // eslint-disable-next-line @next/next/no-img-element
                    <img src={imageUrl(product.imageId)} alt="" className={styles.thumb} />
                  )}
                  <div className={styles.info}>
                    <div className={styles.name}>{product.name}</div>
                    <div className={styles.meta}>
                      {product.categoryName ?? "Uncategorized"}
                      {product.unitName ? ` · ${product.unitName}` : ""}
                    </div>
                  </div>
                  <span className={product.isActive ? styles.badgeLive : styles.badgePending}>
                    {product.isActive ? "Live" : "Pending review"}
                  </span>
                </div>
              ))}
            </div>
          )}
          <a className={styles.addLink} href="/retailer/products/new">
            Add a product
          </a>
        </>
      )}
    </div>
  );
}
