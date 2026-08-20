"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { useGeolocation } from "@/lib/geolocation/useGeolocation";
import { useComparePrices } from "@/lib/hooks/useComparePrices";
import { useHomeFeed } from "@/lib/hooks/useHomeFeed";
import { getActiveCategories } from "@/lib/api/priceCompare";
import { homeFeedSectionId } from "@/lib/utils/slugify";
import type { StorePriceResult } from "@/lib/api/types";
import { LocationBar } from "@/components/home/LocationBar";
import { TopTabBar, type TopTab, type SortMode } from "@/components/home/TopTabBar";
import { BottomNav } from "@/components/home/BottomNav";
import { SpotlightCarousel } from "@/components/home/SpotlightCarousel";
import { RetailerStrip, type Retailer } from "@/components/home/RetailerStrip";
import { HomeFeed } from "@/components/home/HomeFeed";
import { ResultsList } from "@/components/common/ResultsList";
import { EmptyState } from "@/components/common/EmptyState";
import styles from "./page.module.css";

export default function Page() {
  const router = useRouter();
  const { isAuthenticated, isReady, logout } = useAuth();
  const { coordinates, isLocating, isFallback } = useGeolocation();
  const { data, loading, error, search, searchByStore } = useComparePrices();
  const { data: feedSections, loading: feedLoading, error: feedError, load: loadFeed } = useHomeFeed();
  const [sortMode, setSortMode] = useState<SortMode>("all");
  const [hasSearched, setHasSearched] = useState(false);
  const [categoryChips, setCategoryChips] = useState<string[]>([]);
  const [activeTab, setActiveTab] = useState<TopTab>("home");
  const [searchOpen, setSearchOpen] = useState(false);
  const [accountMenuOpen, setAccountMenuOpen] = useState(false);
  const searchInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/phone-entry");
    }
  }, [isReady, isAuthenticated, router]);

  useEffect(() => {
    if (!isReady || !isAuthenticated) return;
    getActiveCategories()
      .then(setCategoryChips)
      .catch(() => {
        // Empty chip row just means no category shortcuts - search still works.
      });
  }, [isReady, isAuthenticated]);

  useEffect(() => {
    if (!isReady || !isAuthenticated || isLocating) return;
    void loadFeed(coordinates);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isReady, isAuthenticated, isLocating]);

  function handleLogout() {
    logout();
    router.replace("/phone-entry");
  }

  function runSearch(keyword: string) {
    setHasSearched(true);
    void search(keyword, coordinates);
  }

  // Category taps browse in place - they jump to that category's row inside
  // the existing home feed (which already has one section per real
  // category) rather than replacing the whole page with a flat results
  // list. setTimeout (not requestAnimationFrame - rAF is throttled/paused
  // on background or non-foreground tabs and can simply never fire) gives
  // React a tick to remount the home feed, if search results were showing
  // instead, before we look up the target id.
  function jumpToCategory(categoryName: string) {
    setHasSearched(false);
    const id = homeFeedSectionId(categoryName);
    setTimeout(() => {
      document.getElementById(id)?.scrollIntoView({ behavior: "smooth", block: "start" });
    }, 50);
  }

  function runStoreSearch(retailer: Retailer) {
    setHasSearched(true);
    void searchByStore(retailer.storeId, coordinates);
  }

  function goHome() {
    setActiveTab("home");
    setHasSearched(false);
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

  // Real stores, deduped from whatever's already in the home feed - not a
  // separate fake retailer directory.
  const retailers = useMemo<Retailer[]>(() => {
    if (!feedSections) return [];
    const seen = new Map<string, Retailer>();
    for (const section of feedSections) {
      for (const item of section.items) {
        if (!seen.has(item.storeId)) {
          seen.set(item.storeId, {
            storeId: item.storeId,
            storeName: item.storeName,
            storeImageId: item.storeImageId,
          });
        }
      }
    }
    return Array.from(seen.values());
  }, [feedSections]);

  function handleSelect(result: StorePriceResult) {
    router.push(`/product?keyword=${encodeURIComponent(result.productName)}`);
  }

  if (!isReady || !isAuthenticated) {
    return null;
  }

  return (
    <div className={styles.root}>
      <img src="/assets/splash/radial-glow.png" alt="" aria-hidden="true" className={styles.glow} />
      <div className={styles.content}>
        <LocationBar
          isLocating={isLocating}
          isFallback={isFallback}
          searchOpen={searchOpen}
          onToggleSearch={() => {
            setSearchOpen((open) => {
              const next = !open;
              if (next) setTimeout(() => searchInputRef.current?.focus(), 50);
              return next;
            });
          }}
          onSearch={runSearch}
          searchLoading={loading}
          searchInputRef={searchInputRef}
          menuOpen={accountMenuOpen}
          onToggleMenu={() => setAccountMenuOpen((open) => !open)}
          onLogout={handleLogout}
        />
        <TopTabBar
          categories={categoryChips}
          sortMode={sortMode}
          onSortChange={setSortMode}
          onHome={goHome}
          onCategorySelect={jumpToCategory}
        />

        {activeTab !== "home" ? (
          <EmptyState message={`${activeTab[0].toUpperCase()}${activeTab.slice(1)} are coming soon.`} />
        ) : (
          <>
            <RetailerStrip retailers={retailers} onSelect={runStoreSearch} />
            {/* <SpotlightCarousel /> */}
            {hasSearched ? (
              <ResultsList
                data={sortedData}
                loading={loading}
                error={error}
                emptyMessage="No prices found for that search yet."
                onSelect={handleSelect}
              />
            ) : (
              <HomeFeed
                sections={feedSections}
                loading={feedLoading}
                error={feedError}
                onSelect={handleSelect}
              />
            )}
          </>
        )}
      </div>
      {/* <BottomNav
        activeTab={activeTab}
        onTabChange={(tab) => {
          setActiveTab(tab);
          if (tab === "home") setHasSearched(false);
        }}
        onRefresh={() => void loadFeed(coordinates)}
      /> */}
    </div>
  );
}
