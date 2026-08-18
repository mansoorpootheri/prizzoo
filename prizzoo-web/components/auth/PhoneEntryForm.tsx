"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import { requestOtp } from "@/lib/api/otpAuth";
import { ApiError } from "@/lib/api/client";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import styles from "./PhoneEntryForm.module.css";

export function PhoneEntryForm() {
  const router = useRouter();
  const [phoneNumber, setPhoneNumber] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await requestOtp(phoneNumber);
      router.push(`/otp-verify?phone=${encodeURIComponent(phoneNumber)}`);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not send OTP.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className={styles.root}>
      <div className={styles.topBar}>
        <a className={styles.adminLink} href="/login">
          Login to admin panel
        </a>
      </div>

      <img
        src="/assets/splash/prizzoo-logo.png"
        alt="PriZzoO.com"
        className={styles.logo}
      />
      <p className={styles.tagline}>Compare. Locate. Save</p>

      <h1 className={styles.heading}>Enter your mobile number</h1>
      <p className={styles.subheading}>We&apos;ll send you a one-time code to verify it&apos;s you.</p>

      <form className={styles.form} onSubmit={handleSubmit}>
        <input
          className={styles.field}
          type="tel"
          name="phoneNumber"
          placeholder="Mobile number"
          autoComplete="tel"
          value={phoneNumber}
          onChange={(e) => setPhoneNumber(e.target.value)}
          required
        />

        {error && <ErrorBanner message={error} />}

        <button className={styles.submit} type="submit" disabled={submitting}>
          {submitting ? <LoadingSpinner /> : "Send OTP"}
        </button>
      </form>
    </div>
  );
}
