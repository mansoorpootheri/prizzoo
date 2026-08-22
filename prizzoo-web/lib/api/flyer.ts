import { fetchJson } from "./client";
import type { AddFlyerItemsInput, CreateFlyerForStoreInput, Flyer, FlyerDetail } from "./types";

const BASE = "/api/services/app/Flyer";

// Admin only - uploads a flyer photo for a store together with its item
// list, typed in by hand. Goes live immediately, no moderation step.
export function createFlyerForStore(input: CreateFlyerForStoreInput): Promise<Flyer> {
  return fetchJson<Flyer>(`${BASE}/CreateFlyerForStore`, { method: "POST", body: input });
}

// Admin only - add more items to a flyer that's already live, same
// goes-live-immediately behavior as the initial upload.
export function addItemsToFlyer(input: AddFlyerItemsInput): Promise<void> {
  return fetchJson<void>(`${BASE}/AddItemsToFlyer`, { method: "POST", body: input });
}

// Shopper - every flyer uploaded for a store, newest first (browsed as a carousel), or [] if it has none.
export function getFlyersForStore(storeId: string): Promise<FlyerDetail[]> {
  return fetchJson<FlyerDetail[]>(`${BASE}/GetFlyersForStore?Id=${encodeURIComponent(storeId)}`);
}

// Shopper - the most recently uploaded flyers across every store, newest
// first - backs the home screen's carousel shown before any store is selected.
export function getRecentFlyers(): Promise<FlyerDetail[]> {
  return fetchJson<FlyerDetail[]>(`${BASE}/GetRecentFlyers`);
}

// Admin only - remove a flyer entirely. Any Price rows it caused to be
// created stay on record (see FlyerAppService.DeleteAsync) - only the
// flyer itself and its FlyerProduct links go away.
export function deleteFlyer(id: string): Promise<void> {
  return fetchJson<void>(`${BASE}/Delete?Id=${encodeURIComponent(id)}`, { method: "DELETE" });
}
