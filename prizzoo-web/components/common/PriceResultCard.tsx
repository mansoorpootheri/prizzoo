import { imageUrl } from "@/lib/api/image";
import type { StorePriceResult } from "@/lib/api/types";
import { ImageWithFallback } from "@/components/common/ImageWithFallback";
import styles from "./PriceResultCard.module.css";

interface PriceResultCardProps {
  result: StorePriceResult;
  onClick?: () => void;
}

export function PriceResultCard({ result, onClick }: PriceResultCardProps) {
  return (
    <button type="button" className={styles.root} onClick={onClick}>
      <span className={styles.imageWrap}>
        {result.productImageId ? (
          <ImageWithFallback
            src={imageUrl(result.productImageId)}
            alt=""
            className={styles.image}
            fallback={<span className={styles.placeholderIcon} aria-hidden="true">🛒</span>}
          />
        ) : (
          <span className={styles.placeholderIcon} aria-hidden="true">
            🛒
          </span>
        )}
        {result.discountPercent != null && (
          <span className={styles.discountBadge}>↓{result.discountPercent}%</span>
        )}
        {result.isStale && <span className={styles.staleBadge}>Stale</span>}
      </span>

      <span className={styles.rating}>
        {result.ratingCount > 0 ? (
          <>
            <span className={styles.star} aria-hidden="true">★</span>
            {result.averageRating!.toFixed(1)}
            <span className={styles.ratingCount}>({result.ratingCount})</span>
          </>
        ) : (
          <span className={styles.noRating}>No ratings yet</span>
        )}
      </span>

      <span className={styles.name}>{result.productName}</span>

      <span className={styles.priceRow}>
        <span className={styles.price}>
          {result.currency} {result.amount.toFixed(2)}
        </span>
        {result.originalAmount != null && result.originalAmount > result.amount && (
          <span className={styles.originalPrice}>
            {result.currency} {result.originalAmount.toFixed(2)}
          </span>
        )}
      </span>

      <span className={styles.storeRow}>
        {result.storeImageId ? (
          <ImageWithFallback
            src={imageUrl(result.storeImageId)}
            alt=""
            className={styles.storeLogo}
            fallback={<span className={styles.storeLogoPlaceholder} aria-hidden="true">🏬</span>}
          />
        ) : (
          <span className={styles.storeLogoPlaceholder} aria-hidden="true">🏬</span>
        )}
        <span className={styles.storeInfo}>
          <span className={styles.storeName}>{result.storeName}</span>
          <span className={styles.distance}>
            📍 {result.storeAddress ? `${result.storeAddress} · ` : ""}
            {result.distanceKm.toFixed(1)} km
          </span>
        </span>
      </span>
    </button>
  );
}
