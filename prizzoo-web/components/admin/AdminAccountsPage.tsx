"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { addAdmin, getAdmins } from "@/lib/api/admins";
import type { AdminUser } from "@/lib/api/types";
import { ApiError } from "@/lib/api/client";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { EmptyState } from "@/components/common/EmptyState";
import { Modal } from "@/components/common/Modal";
import styles from "./CategoryMaster.module.css";

// Add further admin phone numbers - there's only ever the single seeded
// EnterpriseBaseConsts.InitialAdminPhoneNumber account otherwise, and no
// database access is needed to grow beyond it.
export function AdminAccountsPage() {
  const router = useRouter();
  const { isAuthenticated, isAdmin, isReady } = useAuth();
  const [admins, setAdmins] = useState<AdminUser[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [phoneNumber, setPhoneNumber] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/phone-entry");
    } else if (isReady && isAuthenticated && !isAdmin) {
      router.replace("/home");
    }
  }, [isReady, isAuthenticated, isAdmin, router]);

  function load() {
    setLoading(true);
    getAdmins()
      .then(setAdmins)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Something went wrong."))
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    if (!isReady || !isAuthenticated || !isAdmin) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect
    load();
  }, [isReady, isAuthenticated, isAdmin]);

  function openAdd() {
    setPhoneNumber("");
    setModalOpen(true);
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await addAdmin({ phoneNumber });
      setModalOpen(false);
      load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not add this admin.");
    } finally {
      setSubmitting(false);
    }
  }

  if (!isReady || !isAuthenticated || !isAdmin) {
    return null;
  }

  return (
    <div className={styles.root}>
      <div className={styles.headerRow}>
        <button type="button" className={styles.back} onClick={() => router.back()}>
          ← Back
        </button>
        <button type="button" className={styles.back} onClick={() => router.push("/admin/dashboard")}>
          🏠 Dashboard
        </button>
      </div>

      <div className={styles.topBar}>
        <div className={styles.topBarHeadings}>
          <h1 className={styles.heading}>Admins</h1>
          <p className={styles.subheading}>
            Anyone added here can log in with their phone number and the same OTP flow as a shopper,
            and lands on the admin screens via the account menu.
          </p>
        </div>
        <button type="button" className={styles.addButton} onClick={openAdd}>
          + Add admin
        </button>
      </div>

      {loading && <LoadingSpinner />}
      {!loading && (!admins || admins.length === 0) && <EmptyState message="No admins yet." />}
      {!loading && admins && admins.length > 0 && (
        <div className={styles.list}>
          {admins.map((admin) => (
            <div key={admin.id} className={styles.card}>
              <div className={styles.info}>
                <div className={styles.name}>{admin.phoneNumber}</div>
                <div className={styles.meta}>Added {new Date(admin.creationTime).toLocaleDateString()}</div>
              </div>
            </div>
          ))}
        </div>
      )}

      {modalOpen && (
        <Modal title="Add an admin" onClose={() => setModalOpen(false)}>
          <form className={styles.form} onSubmit={handleSubmit}>
            <label className={styles.label}>
              Phone number
              <input
                className={styles.field}
                type="tel"
                value={phoneNumber}
                onChange={(e) => setPhoneNumber(e.target.value)}
                required
              />
            </label>

            {error && <ErrorBanner message={error} />}

            <button className={styles.submit} type="submit" disabled={submitting}>
              {submitting ? <LoadingSpinner /> : "Add admin"}
            </button>
          </form>
        </Modal>
      )}
    </div>
  );
}
