"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import styles from "./AdminDashboard.module.css";

export function AdminDashboard() {
  const router = useRouter();
  const { isAuthenticated, isReady, logout } = useAuth();

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/login");
    }
  }, [isReady, isAuthenticated, router]);

  function handleLogout() {
    logout();
    router.replace("/login");
  }

  if (!isReady || !isAuthenticated) {
    return null;
  }

  return (
    <div className={styles.root}>
      <div className={styles.topBar}>
        <h1 className={styles.heading}>Admin</h1>
        <button type="button" className={styles.logoutButton} onClick={handleLogout}>
          Logout
        </button>
      </div>
      <div className={styles.linkList}>
        <a className={styles.linkCard} href="/admin/stores/new">
          Create a store + owner
        </a>
        <a className={styles.linkCard} href="/admin/stores">
          Manage stores
        </a>
        <a className={styles.linkCard} href="/admin/moderation/prices">
          Price moderation queue
        </a>
        <a className={styles.linkCard} href="/admin/categories">
          Category master
        </a>
        <a className={styles.linkCard} href="/admin/locations">
          Location master
        </a>
        <a className={styles.linkCard} href="/admin/change-password">
          Change password
        </a>
      </div>
    </div>
  );
}
