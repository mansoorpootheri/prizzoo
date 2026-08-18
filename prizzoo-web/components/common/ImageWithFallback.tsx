"use client";

import { useState, type ReactNode } from "react";

interface ImageWithFallbackProps {
  src: string;
  alt: string;
  className?: string;
  fallback: ReactNode;
}

// Store/product images can 404 (stale seed data, deleted upload, bad id),
// or - seen in practice with some seeded store logos - load "successfully"
// as a degenerate 1x1 pixel file that just stretches into a solid color
// blob. Plain <img> shows a broken-image glyph for the first case and a
// silently-wrong blob for the second; this swaps both to the same
// placeholder used when there's no image id at all.
export function ImageWithFallback({ src, alt, className, fallback }: ImageWithFallbackProps) {
  const [failed, setFailed] = useState(false);

  if (failed) {
    return <>{fallback}</>;
  }

  return (
    // eslint-disable-next-line @next/next/no-img-element
    <img
      src={src}
      alt={alt}
      className={className}
      onError={() => setFailed(true)}
      onLoad={(e) => {
        const img = e.currentTarget;
        if (img.naturalWidth <= 1 || img.naturalHeight <= 1) {
          setFailed(true);
        }
      }}
    />
  );
}
