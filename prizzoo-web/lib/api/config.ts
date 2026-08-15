export const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5001";

// Confirmed empirically against the running dev backend (decoded a login
// JWT's tenantId claim, then verified the "Abp-TenantId" header/cookie name
// - note the hyphen, not "Abp.TenantId" as ABP docs commonly show) - only
// needed for registration; every other call resolves tenancy from the JWT.
export const DEFAULT_TENANT_ID = Number(
  process.env.NEXT_PUBLIC_DEFAULT_TENANT_ID ?? 1
);
