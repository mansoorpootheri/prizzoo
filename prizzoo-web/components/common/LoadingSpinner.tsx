import styles from "./LoadingSpinner.module.css";

export function LoadingSpinner() {
  return <span className={styles.root} role="status" aria-label="Loading" />;
}
