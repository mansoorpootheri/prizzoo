"use client";

import { FormEvent, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { requestOtp } from "@/lib/api/otpAuth";
import { ApiError } from "@/lib/api/client";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import styles from "./OtpVerifyForm.module.css";

export function OtpVerifyForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { loginWithOtp } = useAuth();
  const phoneNumber = searchParams.get("phone") ?? "";

  const [code, setCode] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [resending, setResending] = useState(false);
  const [resent, setResent] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await loginWithOtp(phoneNumber, code);
      router.replace("/home");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Incorrect OTP.");
    } finally {
      setSubmitting(false);
    }
  }

  async function handleResend() {
    setError(null);
    setResending(true);
    try {
      await requestOtp(phoneNumber);
      setResent(true);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not resend OTP.");
    } finally {
      setResending(false);
    }
  }

  if (!phoneNumber) {
    return (
      <div className={styles.root}>
        <ErrorBanner message="No phone number to verify. Please start again." />
        <a className={styles.backLink} href="/phone-entry">
          ← Back to phone entry
        </a>
      </div>
    );
  }

  return (
    <div className={styles.root}>
      <h1 className={styles.heading}>Enter the code</h1>
      <p className={styles.subheading}>We sent a 6-digit code to {phoneNumber}.</p>

      <form className={styles.form} onSubmit={handleSubmit}>
        <input
          className={styles.field}
          type="text"
          inputMode="numeric"
          maxLength={6}
          placeholder="6-digit code"
          autoComplete="one-time-code"
          value={code}
          onChange={(e) => setCode(e.target.value)}
          required
        />

        {error && <ErrorBanner message={error} />}

        <button className={styles.submit} type="submit" disabled={submitting || code.length !== 6}>
          {submitting ? <LoadingSpinner /> : "Verify"}
        </button>
      </form>

      <button
        type="button"
        className={styles.resend}
        onClick={handleResend}
        disabled={resending}
      >
        {resending ? "Sending…" : resent ? "Code resent" : "Resend code"}
      </button>

      <a className={styles.backLink} href="/phone-entry">
        ← Change number
      </a>
    </div>
  );
}
