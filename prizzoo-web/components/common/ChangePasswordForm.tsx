"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { changeMyPassword } from "@/lib/api/myAccount";
import { ApiError } from "@/lib/api/client";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import styles from "./ChangePasswordForm.module.css";

interface ChangePasswordFormProps {
  /** Where "Back to dashboard" goes after a successful change - the caller's own dashboard route. */
  backHref: string;
}

// Shared between the Admin and ShopOwner portals - both are password-based
// logins, both reached via /login, so one form serves either. Shoppers
// (OTP-only, no password) never see this.
export function ChangePasswordForm({ backHref }: ChangePasswordFormProps) {
  const router = useRouter();
  const { isAuthenticated, isReady } = useAuth();
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [done, setDone] = useState(false);

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/login");
    }
  }, [isReady, isAuthenticated, router]);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);

    if (newPassword !== confirmPassword) {
      setError("New password and confirmation don't match.");
      return;
    }

    setSubmitting(true);
    try {
      await changeMyPassword(currentPassword, newPassword);
      setDone(true);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not change your password.");
    } finally {
      setSubmitting(false);
    }
  }

  if (!isReady || !isAuthenticated) {
    return null;
  }

  if (done) {
    return (
      <div className={styles.root}>
        <h1 className={styles.heading}>Password changed</h1>
        <p className={styles.body}>Use your new password next time you log in.</p>
        <button type="button" className={styles.submit} onClick={() => router.push(backHref)}>
          Back to dashboard
        </button>
      </div>
    );
  }

  return (
    <div className={styles.root}>
      <button type="button" className={styles.back} onClick={() => router.back()}>
        ← Back
      </button>
      <h1 className={styles.heading}>Change password</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <label className={styles.label}>
          Current password
          <input
            className={styles.field}
            type="password"
            autoComplete="current-password"
            value={currentPassword}
            onChange={(e) => setCurrentPassword(e.target.value)}
            required
          />
        </label>
        <label className={styles.label}>
          New password
          <input
            className={styles.field}
            type="password"
            autoComplete="new-password"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            required
          />
        </label>
        <label className={styles.label}>
          Confirm new password
          <input
            className={styles.field}
            type="password"
            autoComplete="new-password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
          />
        </label>

        {error && <ErrorBanner message={error} />}

        <button className={styles.submit} type="submit" disabled={submitting}>
          {submitting ? <LoadingSpinner /> : "Change password"}
        </button>
      </form>
    </div>
  );
}
