import { fetchJson } from "./client";
import { API_BASE_URL } from "./config";

export interface UploadImageResponse {
  imageId: string;
}

export function uploadImage(file: File): Promise<UploadImageResponse> {
  const formData = new FormData();
  formData.append("file", file);
  return fetchJson<UploadImageResponse>("/api/Image/Upload", {
    method: "POST",
    body: formData,
  });
}

export function imageUrl(imageId: string): string {
  return `${API_BASE_URL}/api/Image/${imageId}`;
}
