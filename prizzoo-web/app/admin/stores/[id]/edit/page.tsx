"use client";

import { use } from "react";
import { EditStoreForm } from "@/components/admin/EditStoreForm";

export default function Page({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  return <EditStoreForm storeId={id} />;
}
