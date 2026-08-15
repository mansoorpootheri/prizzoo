import { fetchJson } from "./client";
import type { ComparePricesInput, StorePriceResult } from "./types";

export function comparePrices(
  input: ComparePricesInput
): Promise<StorePriceResult[]> {
  return fetchJson<StorePriceResult[]>(
    "/api/services/app/PriceCompare/ComparePrices",
    { method: "POST", body: input }
  );
}
