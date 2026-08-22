import { fetchJson } from "./client";
import type {
  AdminCategory,
  AdminLocation,
  ComboboxItem,
  CreateEditCategoryInput,
  CreateEditLocationInput,
} from "./types";

// Host admin only (Pages_Products_Categories is host-side) - manages the
// shared public category master data used across every product.
const CATEGORY_BASE = "/api/services/app/Category";

export function getCategories(): Promise<AdminCategory[]> {
  return fetchJson<AdminCategory[]>(`${CATEGORY_BASE}/GetAll`);
}

export function getCategoriesForCombobox(): Promise<ComboboxItem[]> {
  return fetchJson<ComboboxItem[]>(`${CATEGORY_BASE}/GetForCombobox`);
}

export function createOrEditCategory(input: CreateEditCategoryInput): Promise<void> {
  return fetchJson<void>(`${CATEGORY_BASE}/CreateOrEdit`, { method: "POST", body: input });
}

export function deleteCategory(id: string): Promise<void> {
  // GET/DELETE-verb ABP dynamic-API actions bind parameters from the query
  // string, not a JSON body - a DELETE with a body would be silently ignored.
  return fetchJson<void>(`${CATEGORY_BASE}/Delete?Id=${encodeURIComponent(id)}`, { method: "DELETE" });
}

// Host admin only (Pages_Locations is host-side) - a locality within a
// Geography District, e.g. "Feroke" within "Kozhikode". Feeds the store
// creation form's District ("City") -> Location pickers.
const LOCATION_BASE = "/api/services/app/Location";

export function getLocations(districtId?: number): Promise<AdminLocation[]> {
  const query = districtId != null ? `?districtId=${districtId}` : "";
  return fetchJson<AdminLocation[]>(`${LOCATION_BASE}/GetAll${query}`);
}

export function getLocationsForCombobox(districtId: number): Promise<ComboboxItem[]> {
  return fetchJson<ComboboxItem[]>(`${LOCATION_BASE}/GetForCombobox?districtId=${districtId}`);
}

export function createOrEditLocation(input: CreateEditLocationInput): Promise<void> {
  return fetchJson<void>(`${LOCATION_BASE}/CreateOrEdit`, { method: "POST", body: input });
}

export function deleteLocation(id: string): Promise<void> {
  return fetchJson<void>(`${LOCATION_BASE}/Delete?Id=${encodeURIComponent(id)}`, { method: "DELETE" });
}

// Host admin only (Pages_Products) - the full active catalog, for the
// flyer form's "pick an existing product" option per item row.
export function getProductsForCombobox(): Promise<ComboboxItem[]> {
  return fetchJson<ComboboxItem[]>("/api/services/app/Product/GetForCombobox");
}
