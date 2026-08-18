import { fetchJson } from "./client";
import type { PendingPrice, PriceStatus, SubmitPriceInput } from "./types";

export function submitPrice(input: SubmitPriceInput): Promise<void> {
  return fetchJson<void>("/api/services/app/PriceSubmission/Submit", {
    method: "POST",
    body: input,
  });
}

export function getPendingPrices(): Promise<PendingPrice[]> {
  return fetchJson<PendingPrice[]>("/api/services/app/PriceSubmission/GetPending");
}

export function moderatePrice(
  id: string,
  status: PriceStatus,
  moderationNote?: string
): Promise<void> {
  return fetchJson<void>("/api/services/app/PriceSubmission/Moderate", {
    method: "POST",
    body: { id, status, moderationNote },
  });
}
