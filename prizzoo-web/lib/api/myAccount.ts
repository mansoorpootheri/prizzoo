import { fetchJson } from "./client";

// Self-service password change - any authenticated user (Admin, ShopOwner)
// changes their own password. Backed by MyAccountAppService.ChangeMyPasswordAsync,
// which is deliberately separate from UserAppService.ChangePassword (that one
// requires the host/tenant Pages_Users permission and would 403 for ShopOwner).
export function changeMyPassword(currentPassword: string, newPassword: string): Promise<void> {
  return fetchJson<void>("/api/services/app/MyAccount/ChangeMyPassword", {
    method: "POST",
    body: { currentPassword, newPassword },
  });
}
