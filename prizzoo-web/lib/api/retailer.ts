import { fetchJson } from "./client";
import type {
  ApplyRetailerInput,
  ComboboxItem,
  CreateProductInput,
  MyProduct,
  MyStore,
  RetailerApplicationListItem,
  RetailerApplicationStatus,
  UpdateMyStoreInput,
} from "./types";

const BASE = "/api/services/app/Retailer";

export function applyAsRetailer(input: ApplyRetailerInput): Promise<void> {
  return fetchJson<void>(`${BASE}/Apply`, { method: "POST", body: input });
}

export function getMyApplicationStatus(): Promise<RetailerApplicationStatus> {
  return fetchJson<RetailerApplicationStatus>(`${BASE}/GetMyApplicationStatus`);
}

export function getPendingApplications(): Promise<RetailerApplicationListItem[]> {
  return fetchJson<RetailerApplicationListItem[]>(`${BASE}/GetPendingApplications`);
}

export function approveApplication(id: string): Promise<void> {
  return fetchJson<void>(`${BASE}/ApproveApplication`, { method: "POST", body: { id } });
}

export function rejectApplication(id: string, reason?: string): Promise<void> {
  return fetchJson<void>(`${BASE}/RejectApplication`, { method: "POST", body: { id, reason } });
}

export function getMyStore(): Promise<MyStore> {
  return fetchJson<MyStore>(`${BASE}/GetMyStore`);
}

export function updateMyStore(input: UpdateMyStoreInput): Promise<void> {
  return fetchJson<void>(`${BASE}/UpdateMyStore`, { method: "POST", body: input });
}

export function getCategoriesForDropdown(): Promise<ComboboxItem[]> {
  return fetchJson<ComboboxItem[]>(`${BASE}/GetCategoriesForDropdown`);
}

export function getUnitsForDropdown(): Promise<ComboboxItem[]> {
  return fetchJson<ComboboxItem[]>(`${BASE}/GetUnitsForDropdown`);
}

export function createMyProduct(input: CreateProductInput): Promise<MyProduct> {
  return fetchJson<MyProduct>(`${BASE}/CreateMyProduct`, { method: "POST", body: input });
}

export function getMyProducts(): Promise<MyProduct[]> {
  return fetchJson<MyProduct[]>(`${BASE}/GetMyProducts`);
}
