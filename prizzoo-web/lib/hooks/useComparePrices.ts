"use client";

import { useCallback, useState } from "react";
import { comparePrices } from "../api/priceCompare";
import type { Coordinates } from "../geolocation/default-location";
import type { StorePriceResult } from "../api/types";
import { ApiError } from "../api/client";

interface UseComparePricesResult {
  data: StorePriceResult[] | null;
  loading: boolean;
  error: string | null;
  search: (keyword: string, coordinates: Coordinates, maxResults?: number) => Promise<void>;
  // Category chip browsing - a separate path from keyword search, since a
  // category name (e.g. "Electronics & Mobiles") is never a substring of a
  // product name and must not be sent as productKeyword.
  searchByCategory: (categoryName: string, coordinates: Coordinates, maxResults?: number) => Promise<void>;
  // Retailer strip browsing - everything one specific store sells.
  searchByStore: (storeId: string, coordinates: Coordinates, maxResults?: number) => Promise<void>;
}

export function useComparePrices(): UseComparePricesResult {
  const [data, setData] = useState<StorePriceResult[] | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const runCompare = useCallback(
    async (
      input: { productKeyword?: string; categoryName?: string; storeId?: string },
      coordinates: Coordinates,
      maxResults = 20
    ) => {
      setLoading(true);
      setError(null);
      try {
        const results = await comparePrices({
          ...input,
          latitude: coordinates.latitude,
          longitude: coordinates.longitude,
          maxResults,
        });
        setData(results);
      } catch (err) {
        setError(err instanceof ApiError ? err.message : "Something went wrong.");
        setData(null);
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const search = useCallback(
    (keyword: string, coordinates: Coordinates, maxResults?: number) =>
      runCompare({ productKeyword: keyword }, coordinates, maxResults),
    [runCompare]
  );

  const searchByCategory = useCallback(
    (categoryName: string, coordinates: Coordinates, maxResults?: number) =>
      runCompare({ categoryName }, coordinates, maxResults),
    [runCompare]
  );

  const searchByStore = useCallback(
    (storeId: string, coordinates: Coordinates, maxResults?: number) =>
      runCompare({ storeId }, coordinates, maxResults),
    [runCompare]
  );

  return { data, loading, error, search, searchByCategory, searchByStore };
}
