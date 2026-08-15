import styles from "./EmptyState.module.css";

export function EmptyState({ message }: { message: string }) {
  return (
    <div className={styles.root}>
      <span className={styles.icon} aria-hidden="true">
        🔍
      </span>
      <p className={styles.message}>{message}</p>
    </div>
  );
}
