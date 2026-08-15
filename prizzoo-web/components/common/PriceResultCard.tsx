import type { StorePriceResult } from "@/lib/api/types";
import styles from "./PriceResultCard.module.css";

interface PriceResultCardProps {
  result: StorePriceResult;
  onClick?: () => void;
}

export function PriceResultCard({ result, onClick }: PriceResultCardProps) {
  return (
    <button type="button" className={styles.root} onClick={onClick}>
      <span className={styles.placeholderIcon} aria-hidden="true">
        🏬
      </span>
      <span className={styles.info}>
        <span className={styles.storeName}>{result.storeName}</span>
        <span className={styles.address}>{result.storeAddress}</span>
        <span className={styles.meta}>
          <span>{result.distanceKm.toFixed(1)} km away</span>
          {result.isStale && <span className={styles.staleBadge}>Stale</span>}
        </span>
      </span>
      <span className={styles.price}>
        {result.currency} {result.amount.toFixed(2)}
      </span>
    </button>
  );
}
