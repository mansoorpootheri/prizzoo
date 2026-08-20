"use client";

import { Suspense } from "react";
import { useSearchParams } from "next/navigation";
import { EditStoreForm } from "@/components/admin/EditStoreForm";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";

// id comes from a query string (?id=...) rather than a dynamic route segment
// ([id]) so this page can be statically exported for Azure Static Web Apps -
// see app/product/page.tsx for the same pattern and why.
function EditStorePageContent() {
  const searchParams = useSearchParams();
  const storeId = searchParams.get("id") ?? "";
  return <EditStoreForm storeId={storeId} />;
}

export default function Page() {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <EditStorePageContent />
    </Suspense>
  );
}
