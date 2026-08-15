"use client";

import { ChangeEvent, useState } from "react";
import { imageUrl, uploadImage } from "@/lib/api/image";
import { ApiError } from "@/lib/api/client";
import styles from "./ImageUploadField.module.css";

interface ImageUploadFieldProps {
  label: string;
  value: string | null;
  onChange: (imageId: string | null) => void;
}

export function ImageUploadField({ label, value, onChange }: ImageUploadFieldProps) {
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleFileChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    if (!file) return;

    setError(null);
    setUploading(true);
    try {
      const { imageId } = await uploadImage(file);
      onChange(imageId);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not upload image.");
    } finally {
      setUploading(false);
    }
  }

  return (
    <label className={styles.label}>
      {label}
      <div className={styles.uploadRow}>
        {value && (
          // eslint-disable-next-line @next/next/no-img-element
          <img src={imageUrl(value)} alt="" className={styles.preview} />
        )}
        <input
          className={styles.input}
          type="file"
          accept="image/jpeg,image/png,image/webp"
          onChange={handleFileChange}
          disabled={uploading}
        />
      </div>
      {uploading && <span className={styles.status}>Uploading…</span>}
      {error && <span className={styles.error}>{error}</span>}
    </label>
  );
}
