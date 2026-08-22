"use client";

import { Suspense } from "react";
import { PriceList } from "@/components/admin/PriceList";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";

// PriceList reads an optional ?productId= query param (to preselect the
// Add-price modal when arriving from the product list), which requires
// useSearchParams() to be wrapped in Suspense for static export - see
// app/admin/products/edit/page.tsx for the same underlying requirement.
export default function Page() {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <PriceList />
    </Suspense>
  );
}
