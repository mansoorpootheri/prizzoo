"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { getMyApplicationStatus } from "@/lib/api/retailer";
import type { RetailerApplicationStatus } from "@/lib/api/types";
import { ApiError } from "@/lib/api/client";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import styles from "./RetailerDashboard.module.css";

export function RetailerDashboard() {
  const router = useRouter();
  const { isAuthenticated, isReady } = useAuth();
  const [status, setStatus] = useState<RetailerApplicationStatus | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/login");
    }
  }, [isReady, isAuthenticated, router]);

  useEffect(() => {
    if (!isReady || !isAuthenticated) return;

    getMyApplicationStatus()
      .then(setStatus)
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

  if (error) {
    return (
      <div className={styles.root}>
        <ErrorBanner message={error} />
      </div>
    );
  }

  if (!status?.hasApplied) {
    return (
      <div className={styles.root}>
        <h1 className={styles.heading}>Sell on Prizzoo</h1>
        <p className={styles.body}>
          Register your shop to list your products and prices for nearby shoppers to find.
        </p>
        <a className={styles.cta} href="/retailer/apply">
          Apply to become a retailer
        </a>
      </div>
    );
  }

  if (!status.isApproved) {
    return (
      <div className={styles.root}>
        <h1 className={styles.heading}>Application under review</h1>
        <div className={styles.card}>
          <div className={styles.storeName}>{status.storeName}</div>
          <p className={styles.body}>
            We&apos;re reviewing your shop details. This usually doesn&apos;t take long -
            check back soon.
          </p>
          <span className={styles.badgePending}>Pending review</span>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.root}>
      <h1 className={styles.heading}>{status.storeName}</h1>
      <span className={styles.badgeApproved}>Approved</span>
      <div className={styles.linkList}>
        <a className={styles.linkCard} href="/retailer/products">
          My products
        </a>
        <a className={styles.linkCard} href="/retailer/products/new">
          Add a product
        </a>
        <a className={styles.linkCard} href="/retailer/prices/new">
          Submit a price
        </a>
      </div>
    </div>
  );
}
