import styles from "./ErrorBanner.module.css";

export function ErrorBanner({ message }: { message: string }) {
  return (
    <div className={styles.root} role="alert">
      {message}
    </div>
  );
}
