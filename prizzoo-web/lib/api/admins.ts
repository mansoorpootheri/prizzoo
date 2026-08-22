import { fetchJson } from "./client";
import type { AddAdminInput, AdminUser } from "./types";

const BASE = "/api/services/app/Admin";

export function getAdmins(): Promise<AdminUser[]> {
  return fetchJson<AdminUser[]>(`${BASE}/GetAdmins`);
}

export function addAdmin(input: AddAdminInput): Promise<void> {
  return fetchJson<void>(`${BASE}/AddAdmin`, { method: "POST", body: input });
}
