"use client";

import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { useGeolocation } from "@/lib/geolocation/useGeolocation";
import { useComparePrices } from "@/lib/hooks/useComparePrices";
import type { StorePriceResult } from "@/lib/api/types";
import { LocationBar } from "@/components/home/LocationBar";
import { SearchBar } from "@/components/home/SearchBar";
import { FilterChips, type SortMode } from "@/components/home/FilterChips";
import { ResultsList } from "@/components/common/ResultsList";
import styles from "./page.module.css";

export default function Page() {
  const router = useRouter();
  const { isAuthenticated, isReady } = useAuth();
  const { coordinates, isLocating, isFallback } = useGeolocation();
  const { data, loading, error, search } = useComparePrices();
  const [sortMode, setSortMode] = useState<SortMode>("all");
  const [hasSearched, setHasSearched] = useState(false);

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/login");
    }
  }, [isReady, isAuthenticated, router]);

  function runSearch(keyword: string) {
    setHasSearched(true);
    void search(keyword, coordinates);
  }

  const sortedData = useMemo<StorePriceResult[] | null>(() => {
    if (!data) return data;
    const copy = [...data];
    if (sortMode === "nearby") {
      copy.sort((a, b) => a.distanceKm - b.distanceKm);
    } else if (sortMode === "latest") {
      copy.sort((a, b) => new Date(b.observedAt).getTime() - new Date(a.observedAt).getTime());
    }
    return copy;
  }, [data, sortMode]);

  function handleSelect(result: StorePriceResult) {
    router.push(`/product/${encodeURIComponent(result.productName)}`);
  }

  if (!isReady || !isAuthenticated) {
    return null;
  }

  return (
    <div className={styles.root}>
      <img src="/assets/splash/radial-glow.png" alt="" aria-hidden="true" className={styles.glow} />
      <div className={styles.content}>
        <LocationBar isLocating={isLocating} isFallback={isFallback} />
        <SearchBar onSearch={runSearch} loading={loading} />
        <FilterChips
          activeSort={sortMode}
          onSortChange={setSortMode}
          onKeywordChip={runSearch}
        />
        <ResultsList
          data={hasSearched ? sortedData : []}
          loading={loading}
          error={error}
          emptyMessage={
            hasSearched
              ? "No prices found for that search yet."
              : "Search for a product to compare prices nearby."
          }
          onSelect={handleSelect}
        />
      </div>
    </div>
  );
}
