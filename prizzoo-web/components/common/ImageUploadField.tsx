"use client";

import { ChangeEvent, useState } from "react";
import { imageUrl, uploadImage } from "@/lib/api/image";
import { ApiError } from "@/lib/api/client";
import { ImageCropModal } from "./ImageCropModal";
import styles from "./ImageUploadField.module.css";

interface ImageUploadFieldProps {
  label: string;
  value: string | null;
  onChange: (imageId: string | null) => void;
}

export function ImageUploadField({ label, value, onChange }: ImageUploadFieldProps) {
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [pendingFile, setPendingFile] = useState<{ src: string; name: string } | null>(null);

  function handleFileChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    // Reset so picking the same file again still fires onChange.
    event.target.value = "";
    if (!file) return;

    setError(null);
    setPendingFile({ src: URL.createObjectURL(file), name: file.name });
  }

  function closeCropModal() {
    if (pendingFile) URL.revokeObjectURL(pendingFile.src);
    setPendingFile(null);
  }

  async function handleCropped(croppedFile: File) {
    closeCropModal();
    setUploading(true);
    try {
      const { imageId } = await uploadImage(croppedFile);
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

      {pendingFile && (
        <ImageCropModal
          imageSrc={pendingFile.src}
          fileName={pendingFile.name}
          onCancel={closeCropModal}
          onCropped={handleCropped}
        />
      )}
    </label>
  );
}
