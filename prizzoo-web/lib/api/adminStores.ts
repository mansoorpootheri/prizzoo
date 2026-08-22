import { fetchJson } from "./client";
import type {
  AdminStoreDetail,
  CreateStoreInput,
  PagedResult,
  UpdateStoreInput,
} from "./types";

// Admin creates a store directly - no owner login to provision (admin
// manages every store; there is no separate shop-owner portal).
export function createStore(input: CreateStoreInput): Promise<AdminStoreDetail> {
  return fetchJson<AdminStoreDetail>("/api/services/app/Store/Create", {
    method: "POST",
    body: input,
  });
}

export function getStores(keyword?: string): Promise<PagedResult<AdminStoreDetail>> {
  const query = keyword ? `&Keyword=${encodeURIComponent(keyword)}` : "";
  return fetchJson<PagedResult<AdminStoreDetail>>(
    `/api/services/app/Store/GetAll?MaxResultCount=200${query}`
  );
}

export function getStore(id: string): Promise<AdminStoreDetail> {
  return fetchJson<AdminStoreDetail>(`/api/services/app/Store/Get?Id=${encodeURIComponent(id)}`);
}

export function updateStore(input: UpdateStoreInput): Promise<AdminStoreDetail> {
  return fetchJson<AdminStoreDetail>("/api/services/app/Store/Update", {
    method: "PUT",
    body: input,
  });
}
