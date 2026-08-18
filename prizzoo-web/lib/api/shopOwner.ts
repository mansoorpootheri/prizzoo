import { fetchJson } from "./client";
import type {
  ComboboxItem,
  CreateProductInput,
  MyProduct,
  MyStore,
  MyStoreProduct,
  SubmitMyPriceInput,
} from "./types";

const BASE = "/api/services/app/ShopOwner";

export function getMyStore(): Promise<MyStore> {
  return fetchJson<MyStore>(`${BASE}/GetMyStore`);
}

export function getMyProductsAndPrices(): Promise<MyStoreProduct[]> {
  return fetchJson<MyStoreProduct[]>(`${BASE}/GetMyProductsAndPrices`);
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

export function submitPriceForMyStore(input: SubmitMyPriceInput): Promise<void> {
  return fetchJson<void>(`${BASE}/SubmitPriceForMyStore`, { method: "POST", body: input });
}
