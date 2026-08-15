"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { register } from "@/lib/api/account";
import { ApiError } from "@/lib/api/client";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import styles from "../login/LoginForm.module.css";

export function RegisterForm() {
  const router = useRouter();
  const { login } = useAuth();
  const [name, setName] = useState("");
  const [surname, setSurname] = useState("");
  const [userName, setUserName] = useState("");
  const [emailAddress, setEmailAddress] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await register({ name, surname, userName, emailAddress, password });
      // Login by email, not username - see lib/api/auth.ts: username lookup
      // for tenant-scoped users like this one always resolves host-only.
      await login(emailAddress, password);
      router.replace("/home");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Registration failed.");
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

      <h1 className={styles.heading}>Create your account</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <input
          className={styles.field}
          type="text"
          name="name"
          placeholder="First name"
          autoComplete="given-name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
        />
        <input
          className={styles.field}
          type="text"
          name="surname"
          placeholder="Last name"
          autoComplete="family-name"
          value={surname}
          onChange={(e) => setSurname(e.target.value)}
          required
        />
        <input
          className={styles.field}
          type="text"
          name="userName"
          placeholder="Username"
          autoComplete="username"
          value={userName}
          onChange={(e) => setUserName(e.target.value)}
          required
        />
        <input
          className={styles.field}
          type="email"
          name="emailAddress"
          placeholder="Email address"
          autoComplete="email"
          value={emailAddress}
          onChange={(e) => setEmailAddress(e.target.value)}
          required
        />
        <input
          className={styles.field}
          type="password"
          name="password"
          placeholder="Password"
          autoComplete="new-password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />

        {error && <ErrorBanner message={error} />}

        <button className={styles.submit} type="submit" disabled={submitting}>
          {submitting ? <LoadingSpinner /> : "Create account"}
        </button>
      </form>

      <p className={styles.footer}>
        Already have an account?{" "}
        <a className={styles.footerLink} href="/login">
          Log in
        </a>
      </p>
    </div>
  );
}
