import { imageUrl } from "@/lib/api/image";
import { ImageWithFallback } from "@/components/common/ImageWithFallback";
import styles from "./RetailerStrip.module.css";

export interface Retailer {
  storeId: string;
  storeName: string;
  storeImageId: string | null;
}

interface RetailerStripProps {
  retailers: Retailer[];
  onSelect: (retailer: Retailer) => void;
}

// The reference design's row of retailer logos. Built from real Store data
// (deduped from whatever's already in the home feed, not a fake merchant
// list) - tapping a logo browses everything that store sells, via the same
// ComparePrices endpoint's StoreId filter used by the category chips. If
// that store has a flyer, it surfaces automatically above the results (see
// StoreFlyerBanner in app/home/page.tsx) - no separate flyer control here.
export function RetailerStrip({ retailers, onSelect }: RetailerStripProps) {
  if (retailers.length === 0) return null;

  return (
    <div className={styles.root}>
      {retailers.map((r) => (
        <button key={r.storeId} type="button" className={styles.item} onClick={() => onSelect(r)}>
          <span className={styles.logoWrap}>
            {r.storeImageId ? (
              <ImageWithFallback
                src={imageUrl(r.storeImageId)}
                alt=""
                className={styles.logo}
                fallback={<span className={styles.logoPlaceholder} aria-hidden="true">🏬</span>}
              />
            ) : (
              <span className={styles.logoPlaceholder} aria-hidden="true">🏬</span>
            )}
          </span>
          <span className={styles.name}>{r.storeName}</span>
        </button>
      ))}
    </div>
  );
}
