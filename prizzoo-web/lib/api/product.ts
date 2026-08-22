import { fetchJson } from "./client";
import type { AdminProduct, CreateEditProductInput, PagedResult } from "./types";

// Host admin only (Pages_Products) - direct catalog management, independent
// of the flyer-upload flow that otherwise auto-creates products by name.
const PRODUCT_BASE = "/api/services/app/Product";

export function getProducts(keyword?: string): Promise<PagedResult<AdminProduct>> {
  const query = keyword ? `&Keyword=${encodeURIComponent(keyword)}` : "";
  return fetchJson<PagedResult<AdminProduct>>(`${PRODUCT_BASE}/GetAll?MaxResultCount=200${query}`);
}

export function getProduct(id: string): Promise<AdminProduct> {
  return fetchJson<AdminProduct>(`${PRODUCT_BASE}/Get?Id=${encodeURIComponent(id)}`);
}

export function createProduct(input: CreateEditProductInput): Promise<AdminProduct> {
  return fetchJson<AdminProduct>(`${PRODUCT_BASE}/Create`, { method: "POST", body: input });
}

export function updateProduct(input: CreateEditProductInput & { id: string }): Promise<AdminProduct> {
  return fetchJson<AdminProduct>(`${PRODUCT_BASE}/Update`, { method: "PUT", body: input });
}

export function deleteProduct(id: string): Promise<void> {
  return fetchJson<void>(`${PRODUCT_BASE}/Delete?Id=${encodeURIComponent(id)}`, { method: "DELETE" });
}
