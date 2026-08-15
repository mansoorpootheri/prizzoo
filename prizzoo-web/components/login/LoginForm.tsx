"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { ApiError } from "@/lib/api/client";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import styles from "./LoginForm.module.css";

export function LoginForm() {
  const router = useRouter();
  const { login } = useAuth();
  const [userNameOrEmailAddress, setUserNameOrEmailAddress] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await login(userNameOrEmailAddress, password);
      router.replace("/home");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Login failed.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className={styles.root}>
      <img
        src="/assets/splash/prizzoo-logo.png"
        alt="PriZzoO.com"
        className={styles.logo}
      />
      <p className={styles.tagline}>Compare. Locate. Save</p>

      <h1 className={styles.heading}>Enter your account</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <input
          className={styles.field}
          type="text"
          name="userNameOrEmailAddress"
          placeholder="Username or email address"
          autoComplete="username"
          value={userNameOrEmailAddress}
          onChange={(e) => setUserNameOrEmailAddress(e.target.value)}
          required
        />
        <input
          className={styles.field}
          type="password"
          name="password"
          placeholder="Password"
          autoComplete="current-password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />

        {error && <ErrorBanner message={error} />}

        <button className={styles.submit} type="submit" disabled={submitting}>
          {submitting ? <LoadingSpinner /> : "Log in"}
        </button>
      </form>

      <p className={styles.footer}>
        Don&apos;t have an account?{" "}
        <a className={styles.footerLink} href="/register">
          Create one
        </a>
        <br />
        By continuing you agree to our{" "}
        <a className={styles.footerLink} href="#">
          Terms of service
        </a>{" "}
        and{" "}
        <a className={styles.footerLink} href="#">
          Privacy Policy
        </a>
      </p>
    </div>
  );
}
