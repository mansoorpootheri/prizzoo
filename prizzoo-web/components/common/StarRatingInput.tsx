"use client";

import { useEffect, useState } from "react";
import { getMyRating, rateProduct } from "@/lib/api/ratings";
import styles from "./StarRatingInput.module.css";

interface StarRatingInputProps {
  productId: string;
  averageRating: number | null;
  ratingCount: number;
}

const STAR_VALUES = [1, 2, 3, 4, 5];

export function StarRatingInput({ productId, averageRating, ratingCount }: StarRatingInputProps) {
  const [myStars, setMyStars] = useState<number | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    let cancelled = false;
    getMyRating(productId)
      .then((r) => {
        if (!cancelled) setMyStars(r.stars);
      })
      .catch(() => {
        // Not fatal - the widget just starts unrated.
      });
    return () => {
      cancelled = true;
    };
  }, [productId]);

  async function handleRate(stars: number) {
    if (submitting) return;
    setSubmitting(true);
    const previous = myStars;
    setMyStars(stars);
    try {
      await rateProduct({ productId, stars });
    } catch {
      setMyStars(previous);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className={styles.root}>
      <span className={styles.summary}>
        {ratingCount > 0 ? (
          <>
            <span className={styles.star} aria-hidden="true">★</span>
            {averageRating!.toFixed(1)} ({ratingCount} rating{ratingCount === 1 ? "" : "s"})
          </>
        ) : (
          "No ratings yet"
        )}
      </span>
      <div className={styles.stars} role="radiogroup" aria-label="Rate this product">
        {STAR_VALUES.map((value) => (
          <button
            key={value}
            type="button"
            role="radio"
            aria-checked={myStars === value}
            className={myStars != null && value <= myStars ? styles.starButtonActive : styles.starButton}
            onClick={() => handleRate(value)}
            disabled={submitting}
          >
            ★
          </button>
        ))}
      </div>
      <span className={styles.hint}>{myStars ? "Tap to change your rating" : "Tap to rate this product"}</span>
    </div>
  );
}
