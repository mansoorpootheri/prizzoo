"use client";

import { useRouter } from "next/navigation";
import { imageUrl } from "@/lib/api/image";
import type { StorePriceResult } from "@/lib/api/types";
import { StarRatingInput } from "@/components/common/StarRatingInput";
import styles from "./ProductHeader.module.css";

interface ProductHeaderProps {
  productName: string;
  results: StorePriceResult[] | null;
}

export function ProductHeader({ productName, results }: ProductHeaderProps) {
  const router = useRouter();
  const cheapest = results && results.length > 0 ? results[0] : null;
  // All results share the same product, so any row's productImageId works.
  const productImageId = results?.find((r) => r.productImageId)?.productImageId;

  return (
    <div className={styles.root}>
      <button type="button" className={styles.back} onClick={() => router.back()}>
        ← Back
      </button>
      {productImageId && (
        // eslint-disable-next-line @next/next/no-img-element
        <img src={imageUrl(productImageId)} alt="" className={styles.thumb} />
      )}
      <h1 className={styles.title}>{productName}</h1>
      <p className={styles.subtitle}>
        {results ? `${results.length} store${results.length === 1 ? "" : "s"} found` : "Searching…"}
        {cheapest && (
          <>
            {" · from "}
            <span className={styles.cheapest}>
              {cheapest.currency} {cheapest.amount.toFixed(2)}
            </span>
          </>
        )}
      </p>
      {cheapest && (
        <StarRatingInput
          productId={cheapest.productId}
          averageRating={cheapest.averageRating}
          ratingCount={cheapest.ratingCount}
        />
      )}
    </div>
  );
}
