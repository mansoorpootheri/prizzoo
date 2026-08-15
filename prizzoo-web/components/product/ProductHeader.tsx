"use client";

import { useRouter } from "next/navigation";
import type { StorePriceResult } from "@/lib/api/types";
import styles from "./ProductHeader.module.css";

interface ProductHeaderProps {
  productName: string;
  results: StorePriceResult[] | null;
}

export function ProductHeader({ productName, results }: ProductHeaderProps) {
  const router = useRouter();
  const cheapest = results && results.length > 0 ? results[0] : null;

  return (
    <div className={styles.root}>
      <button type="button" className={styles.back} onClick={() => router.back()}>
        ← Back
      </button>
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
    </div>
  );
}
