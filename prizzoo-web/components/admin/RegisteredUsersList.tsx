"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { getShoppers } from "@/lib/api/registeredUsers";
import type { RegisteredUser } from "@/lib/api/types";
import { ApiError } from "@/lib/api/client";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { EmptyState } from "@/components/common/EmptyState";
import styles from "./CategoryMaster.module.css";

// Read-only - shoppers self-register via OTP login, so there's nothing to
// add or edit here, unlike every other admin master-data page.
export function RegisteredUsersList() {
  const router = useRouter();
  const { isAuthenticated, isAdmin, isReady } = useAuth();
  const [users, setUsers] = useState<RegisteredUser[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/phone-entry");
    } else if (isReady && isAuthenticated && !isAdmin) {
      router.replace("/home");
    }
  }, [isReady, isAuthenticated, isAdmin, router]);

  useEffect(() => {
    if (!isReady || !isAuthenticated) return;
    getShoppers()
      .then(setUsers)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Something went wrong."))
      .finally(() => setLoading(false));
  }, [isReady, isAuthenticated]);

  if (!isReady || !isAuthenticated || !isAdmin) {
    return null;
  }

  return (
    <div className={styles.root}>
      <button type="button" className={styles.back} onClick={() => router.back()}>
        ← Back
      </button>
      <h1 className={styles.heading}>Registered users</h1>
      <p className={styles.subheading}>
        Every shopper who has logged in with phone + OTP.
      </p>

      {loading && <LoadingSpinner />}
      {error && <ErrorBanner message={error} />}
      {!loading && !error && (!users || users.length === 0) && (
        <EmptyState message="No shoppers have registered yet." />
      )}
      {!loading && !error && users && users.length > 0 && (
        <div className={styles.list}>
          {users.map((user) => (
            <div key={user.id} className={styles.card}>
              <div className={styles.info}>
                <div className={styles.name}>{user.phoneNumber ?? "Unknown number"}</div>
                <div className={styles.meta}>
                  Registered {new Date(user.creationTime).toLocaleDateString()}
                </div>
              </div>
              <span className={user.isActive ? styles.badgeActive : styles.badgeInactive}>
                {user.isActive ? "Active" : "Inactive"}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
