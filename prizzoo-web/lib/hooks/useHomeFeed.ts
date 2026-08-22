"use client";

import { useCallback, useState } from "react";
import { getHomeFeed } from "../api/priceCompare";
import type { Coordinates } from "../geolocation/coordinates";
import type { HomeFeedSection } from "../api/types";
import { ApiError } from "../api/client";

interface UseHomeFeedResult {
  data: HomeFeedSection[] | null;
  loading: boolean;
  error: string | null;
  load: (coordinates: Coordinates) => Promise<void>;
}

export function useHomeFeed(): UseHomeFeedResult {
  const [data, setData] = useState<HomeFeedSection[] | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async (coordinates: Coordinates) => {
    setLoading(true);
    setError(null);
    try {
      const sections = await getHomeFeed({
        latitude: coordinates.latitude,
        longitude: coordinates.longitude,
      });
      setData(sections);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Something went wrong.");
      setData(null);
    } finally {
      setLoading(false);
    }
  }, []);

  return { data, loading, error, load };
}
