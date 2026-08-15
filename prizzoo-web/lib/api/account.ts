import { fetchJson } from "./client";
import { DEFAULT_TENANT_ID } from "./config";
import type { RegisterRequest, RegisterResponse } from "./types";

// The only call in the app that needs an explicit tenant header - every
// other call resolves tenancy from the JWT after login.
export function register(input: RegisterRequest): Promise<RegisterResponse> {
  return fetchJson<RegisterResponse>("/api/services/app/Account/Register", {
    method: "POST",
    body: input,
    headers: { "Abp-TenantId": String(DEFAULT_TENANT_ID) },
    skipAuth: true,
  });
}
