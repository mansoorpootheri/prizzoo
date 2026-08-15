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
}

export function useComparePrices(): UseComparePricesResult {
  const [data, setData] = useState<StorePriceResult[] | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const search = useCallback(
    async (keyword: string, coordinates: Coordinates, maxResults = 20) => {
      setLoading(true);
      setError(null);
      try {
        const results = await comparePrices({
          productKeyword: keyword,
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

  return { data, loading, error, search };
}
