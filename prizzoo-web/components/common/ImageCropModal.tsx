"use client";

import { PointerEvent as ReactPointerEvent, useEffect, useRef, useState } from "react";
import styles from "./ImageCropModal.module.css";

interface ImageCropModalProps {
  /** Object URL of the just-selected file. */
  imageSrc: string;
  /** Output filename/mime hints for the exported File. */
  fileName: string;
  onCancel: () => void;
  onCropped: (file: File) => void;
}

const VIEWPORT_SIZE = 280; // CSS px, square preview
const OUTPUT_SIZE = 1000; // px, square exported image - also caps upload size
const MIN_ZOOM = 1;
const MAX_ZOOM = 3;

interface Offset {
  x: number;
  y: number;
}

/**
 * A minimal, dependency-free crop/resize step shown between picking a file
 * and uploading it. Fixed 1:1 output (product/store photos are always shown
 * as squares elsewhere in the app) - pan by dragging, zoom via the slider,
 * "cover" fit like Instagram's classic single-image crop. Exporting always
 * downsamples to OUTPUT_SIZE, which also keeps upload payloads small
 * regardless of how large the original photo was.
 */
export function ImageCropModal({ imageSrc, fileName, onCancel, onCropped }: ImageCropModalProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const imgRef = useRef<HTMLImageElement | null>(null);
  const [imageReady, setImageReady] = useState(false);
  const [zoom, setZoom] = useState(MIN_ZOOM);
  const [offset, setOffset] = useState<Offset>({ x: 0, y: 0 });
  const dragState = useRef<{ startPointer: Offset; startOffset: Offset } | null>(null);

  useEffect(() => {
    const img = new Image();
    img.onload = () => {
      imgRef.current = img;
      setImageReady(true);
    };
    img.src = imageSrc;
  }, [imageSrc]);

  function computeDraw(containerSize: number) {
    const img = imgRef.current;
    if (!img) return null;

    const coverScale = Math.max(containerSize / img.naturalWidth, containerSize / img.naturalHeight);
    const scale = coverScale * zoom;
    const drawW = img.naturalWidth * scale;
    const drawH = img.naturalHeight * scale;

    const k = containerSize / VIEWPORT_SIZE;
    const maxOffsetX = Math.max(0, (drawW - containerSize) / 2);
    const maxOffsetY = Math.max(0, (drawH - containerSize) / 2);
    const clampedX = Math.min(maxOffsetX, Math.max(-maxOffsetX, offset.x * k));
    const clampedY = Math.min(maxOffsetY, Math.max(-maxOffsetY, offset.y * k));

    const imgX = containerSize / 2 - drawW / 2 + clampedX;
    const imgY = containerSize / 2 - drawH / 2 + clampedY;

    return { drawW, drawH, imgX, imgY };
  }

  function clampOffsetForViewport(candidate: Offset): Offset {
    const img = imgRef.current;
    if (!img) return candidate;

    const coverScale = Math.max(VIEWPORT_SIZE / img.naturalWidth, VIEWPORT_SIZE / img.naturalHeight);
    const scale = coverScale * zoom;
    const drawW = img.naturalWidth * scale;
    const drawH = img.naturalHeight * scale;
    const maxOffsetX = Math.max(0, (drawW - VIEWPORT_SIZE) / 2);
    const maxOffsetY = Math.max(0, (drawH - VIEWPORT_SIZE) / 2);

    return {
      x: Math.min(maxOffsetX, Math.max(-maxOffsetX, candidate.x)),
      y: Math.min(maxOffsetY, Math.max(-maxOffsetY, candidate.y)),
    };
  }

  useEffect(() => {
    if (!imageReady) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setOffset((prev) => clampOffsetForViewport(prev));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [imageReady, zoom]);

  useEffect(() => {
    const canvas = canvasRef.current;
    const img = imgRef.current;
    if (!canvas || !img || !imageReady) return;

    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    const draw = computeDraw(VIEWPORT_SIZE);
    if (!draw) return;

    ctx.clearRect(0, 0, VIEWPORT_SIZE, VIEWPORT_SIZE);
    ctx.drawImage(img, draw.imgX, draw.imgY, draw.drawW, draw.drawH);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [imageReady, zoom, offset]);

  function handlePointerDown(event: ReactPointerEvent<HTMLCanvasElement>) {
    event.currentTarget.setPointerCapture(event.pointerId);
    dragState.current = {
      startPointer: { x: event.clientX, y: event.clientY },
      startOffset: offset,
    };
  }

  function handlePointerMove(event: ReactPointerEvent<HTMLCanvasElement>) {
    if (!dragState.current) return;
    const dx = event.clientX - dragState.current.startPointer.x;
    const dy = event.clientY - dragState.current.startPointer.y;
    setOffset(
      clampOffsetForViewport({
        x: dragState.current.startOffset.x + dx,
        y: dragState.current.startOffset.y + dy,
      })
    );
  }

  function handlePointerUp() {
    dragState.current = null;
  }

  function handleUsePhoto() {
    const img = imgRef.current;
    if (!img) return;

    const outputCanvas = document.createElement("canvas");
    outputCanvas.width = OUTPUT_SIZE;
    outputCanvas.height = OUTPUT_SIZE;
    const ctx = outputCanvas.getContext("2d");
    if (!ctx) return;

    const draw = computeDraw(OUTPUT_SIZE);
    if (!draw) return;

    ctx.drawImage(img, draw.imgX, draw.imgY, draw.drawW, draw.drawH);
    outputCanvas.toBlob(
      (blob) => {
        if (!blob) return;
        const croppedFile = new File([blob], fileName.replace(/\.[^.]+$/, "") + ".jpg", {
          type: "image/jpeg",
        });
        onCropped(croppedFile);
      },
      "image/jpeg",
      0.85
    );
  }

  return (
    <div className={styles.overlay}>
      <div className={styles.dialog}>
        <h2 className={styles.heading}>Adjust photo</h2>
        <p className={styles.subheading}>Drag to reposition, use the slider to zoom.</p>

        <canvas
          ref={canvasRef}
          width={VIEWPORT_SIZE}
          height={VIEWPORT_SIZE}
          className={styles.canvas}
          onPointerDown={handlePointerDown}
          onPointerMove={handlePointerMove}
          onPointerUp={handlePointerUp}
          onPointerLeave={handlePointerUp}
        />

        <input
          className={styles.zoomSlider}
          type="range"
          min={MIN_ZOOM}
          max={MAX_ZOOM}
          step={0.01}
          value={zoom}
          onChange={(e) => setZoom(Number(e.target.value))}
        />

        <div className={styles.actions}>
          <button type="button" className={styles.cancelButton} onClick={onCancel}>
            Cancel
          </button>
          <button
            type="button"
            className={styles.confirmButton}
            onClick={handleUsePhoto}
            disabled={!imageReady}
          >
            Use photo
          </button>
        </div>
      </div>
    </div>
  );
}
