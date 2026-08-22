"use client";

import { useRef, useState } from "react";
import { imageUrl } from "@/lib/api/image";
import type { FlyerDetail } from "@/lib/api/types";
import styles from "./StoreFlyerBanner.module.css";

interface StoreFlyerBannerProps {
  flyers: FlyerDetail[];
  onSelect: (flyer: FlyerDetail) => void;
}

const SLIDE_GAP = 12;

// Shown above a store's results as soon as it's selected, if that store has
// any flyers - a peek carousel (neighboring slides visible at the edges,
// scroll-snap driven so it works with native touch scrolling, no drag
// logic needed) with a dot progress row below, matching the reference
// design's "In the spotlight" banner carousel.
export function StoreFlyerBanner({ flyers, onSelect }: StoreFlyerBannerProps) {
  const trackRef = useRef<HTMLDivElement>(null);
  const [activeIndex, setActiveIndex] = useState(0);

  if (flyers.length === 0) return null;

  function handleScroll() {
    const track = trackRef.current;
    if (!track || track.children.length === 0) return;
    const slideWidth = (track.children[0] as HTMLElement).offsetWidth + SLIDE_GAP;
    const index = Math.round(track.scrollLeft / slideWidth);
    setActiveIndex(Math.min(flyers.length - 1, Math.max(0, index)));
  }

  function scrollToIndex(index: number) {
    const track = trackRef.current;
    if (!track || !track.children[index]) return;
    (track.children[index] as HTMLElement).scrollIntoView({ behavior: "smooth", inline: "center", block: "nearest" });
  }

  return (
    <div className={styles.root}>
      <div className={styles.track} ref={trackRef} onScroll={handleScroll}>
        {flyers.map((flyer) => (
          <button
            key={flyer.id}
            type="button"
            className={styles.slide}
            onClick={() => onSelect(flyer)}
          >
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={imageUrl(flyer.imageId)} alt="" className={styles.slideImage} />
            {flyer.storeName && <span className={styles.slideLabel}>{flyer.storeName}</span>}
          </button>
        ))}
      </div>

      {flyers.length > 1 && (
        <div className={styles.dots}>
          {flyers.map((flyer, i) => (
            <button
              key={flyer.id}
              type="button"
              className={`${styles.dot} ${i === activeIndex ? styles.dotActive : ""}`}
              onClick={() => scrollToIndex(i)}
              aria-label={`Flyer ${i + 1} of ${flyers.length}`}
            />
          ))}
        </div>
      )}
    </div>
  );
}
