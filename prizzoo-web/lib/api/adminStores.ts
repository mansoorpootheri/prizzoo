import { fetchJson } from "./client";
import type {
  AdminStoreDetail,
  CreatedStore,
  CreateStoreWithOwnerInput,
  PagedResult,
  UpdateStoreInput,
} from "./types";

// Host admin only (Pages_Stores is host-side). Creates a store AND its
// owner login in one call - there is no self-service retailer signup.
export function createStoreWithOwner(input: CreateStoreWithOwnerInput): Promise<CreatedStore> {
  return fetchJson<CreatedStore>("/api/services/app/Store/Create", {
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
