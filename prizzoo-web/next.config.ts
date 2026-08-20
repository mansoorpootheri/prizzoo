import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Azure Static Web Apps serves a static build - see app/product/page.tsx
  // and app/admin/stores/edit/page.tsx for why the two former dynamic
  // route segments moved to query params to make this possible.
  output: "export",
};

export default nextConfig;