import { fetchJson } from "./client";
import type { ComparePricesInput, HomeFeedInput, HomeFeedSection, StorePriceResult } from "./types";

export function comparePrices(
  input: ComparePricesInput
): Promise<StorePriceResult[]> {
  return fetchJson<StorePriceResult[]>(
    "/api/services/app/PriceCompare/ComparePrices",
    { method: "POST", body: input }
  );
}

// Home screen's default, no-search-needed listing - "Top Picks for You" plus
// one row per real catalog category, populated from actual nearby prices.
// GET (per ABP's Get*->GET convention), so params go in the query string,
// not a JSON body.
export function getHomeFeed(input: HomeFeedInput): Promise<HomeFeedSection[]> {
  const params = new URLSearchParams({
    latitude: String(input.latitude),
    longitude: String(input.longitude),
    ...(input.radiusKm != null ? { radiusKm: String(input.radiusKm) } : {}),
  });
  return fetchJson<HomeFeedSection[]>(
    `/api/services/app/PriceCompare/GetHomeFeed?${params.toString()}`
  );
}

// Real catalog category names for the home page's browse chips - not a
// hardcoded list, reflects whatever the admin has added in Category master.
export function getActiveCategories(): Promise<string[]> {
  return fetchJson<string[]>("/api/services/app/PriceCompare/GetActiveCategories");
}
