"use client";

import { use, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { useGeolocation } from "@/lib/geolocation/useGeolocation";
import { useComparePrices } from "@/lib/hooks/useComparePrices";
import { ProductHeader } from "@/components/product/ProductHeader";
import { ResultsList } from "@/components/common/ResultsList";
import styles from "./page.module.css";

const PRODUCT_DETAIL_MAX_RESULTS = 50;

export default function Page({ params }: { params: Promise<{ keyword: string }> }) {
  const { keyword } = use(params);
  const productName = decodeURIComponent(keyword);

  const router = useRouter();
  const { isAuthenticated, isReady } = useAuth();
  const { coordinates, isLocating } = useGeolocation();
  const { data, loading, error, search } = useComparePrices();

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/login");
    }
  }, [isReady, isAuthenticated, router]);

  useEffect(() => {
    if (isReady && isAuthenticated && !isLocating) {
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
