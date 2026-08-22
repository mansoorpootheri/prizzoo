import { fetchJson } from "./client";
import type {
  AdminPrice,
  PagedResult,
  PendingPrice,
  PriceStatus,
  SubmitPriceInput,
  UpdatePriceInput,
} from "./types";

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

// Admin-only shortcut: attach a price for a product at a store that goes
// live immediately, skipping the pending-moderation queue (the admin is
// already the trusted party entering it). See
// PriceSubmissionAppService.CreateApprovedAsync.
export function createApprovedPrice(input: SubmitPriceInput): Promise<void> {
  return fetchJson<void>("/api/services/app/PriceSubmission/CreateApproved", {
    method: "POST",
    body: input,
  });
}

// Admin's "which store sells this at what price" view - every price ever
// recorded, any status. See PriceSubmissionAppService.GetAllAsync.
export function getPrices(filters?: {
  keyword?: string;
  storeId?: string;
  productId?: string;
  status?: PriceStatus;
}): Promise<PagedResult<AdminPrice>> {
  const params = new URLSearchParams({ MaxResultCount: "200" });
  if (filters?.keyword) params.set("Keyword", filters.keyword);
  if (filters?.storeId) params.set("StoreId", filters.storeId);
  if (filters?.productId) params.set("ProductId", filters.productId);
  if (filters?.status != null) params.set("Status", String(filters.status));
  return fetchJson<PagedResult<AdminPrice>>(`/api/services/app/PriceSubmission/GetAll?${params.toString()}`);
}

export function updatePrice(input: UpdatePriceInput): Promise<void> {
  return fetchJson<void>("/api/services/app/PriceSubmission/Update", {
    method: "PUT",
    body: input,
  });
}

export function deletePrice(id: string): Promise<void> {
  return fetchJson<void>(`/api/services/app/PriceSubmission/Delete?Id=${encodeURIComponent(id)}`, {
    method: "DELETE",
  });
}
