import { fetchJson } from "./client";
import type { RegisteredUser } from "./types";

// Read-only admin view of every shopper account created via OTP login -
// distinct from lib/api/admins.ts, which manages the separate Admin-role
// accounts. See RegisteredUserAppService.GetShoppersAsync.
export function getShoppers(): Promise<RegisteredUser[]> {
  return fetchJson<RegisteredUser[]>("/api/services/app/RegisteredUser/GetShoppers");
}
