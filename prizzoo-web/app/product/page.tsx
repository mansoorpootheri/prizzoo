"use client";

import { Suspense, useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { useGeolocation } from "@/lib/geolocation/useGeolocation";
import { useComparePrices } from "@/lib/hooks/useComparePrices";
import { ProductHeader } from "@/components/product/ProductHeader";
import { ResultsList } from "@/components/common/ResultsList";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import styles from "./page.module.css";

const PRODUCT_DETAIL_MAX_RESULTS = 50;

// keyword comes from a query string (?keyword=...) rather than a dynamic
// route segment ([keyword]) so this page can be statically exported for
// Azure Static Web Apps - a route segment would need every possible search
// term known at build time (generateStaticParams), which is impossible for
// arbitrary user-typed keywords. useSearchParams() requires a Suspense
// boundary during static builds, hence the split below.
function ProductPageContent() {
  const searchParams = useSearchParams();
  const productName = searchParams.get("keyword") ?? "";

  const router = useRouter();
  const { isAuthenticated, isReady } = useAuth();
  const { coordinates, isLocating } = useGeolocation();
  const { data, loading, error, search } = useComparePrices();

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/phone-entry");
    }
  }, [isReady, isAuthenticated, router]);

  useEffect(() => {
    if (isReady && isAuthenticated && !isLocating && productName) {
      void search(productName, coordinates, PRODUCT_DETAIL_MAX_RESULTS);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isReady, isAuthenticated, isLocating, productName]);

  if (!isReady || !isAuthenticated) {
    return null;
  }

  return (
    <div className={styles.root}>
      <img src="/assets/splash/radial-glow.png" alt="" aria-hidden="true" className={styles.glow} />
      <div className={styles.content}>
        <ProductHeader productName={productName} results={data} />
        <ResultsList
          data={data}
          loading={loading || isLocating}
          error={error}
          emptyMessage="No stores currently carry this product."
          onSelect={() => {}}
        />
      </div>
    </div>
  );
}

export default function Page() {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <ProductPageContent />
    </Suspense>
  );
}
