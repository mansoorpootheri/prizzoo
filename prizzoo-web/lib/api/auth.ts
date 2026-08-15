import { fetchJson } from "./client";
import type { AuthenticateRequest, AuthenticateResponse } from "./types";

// Confirmed empirically: TokenAuthController's username lookup always
// resolves host-only regardless of any tenant header sent (unlike
// Account/Register, which does respect it) - a self-registered tenant user
// can only log in by email, not username. Callers should collect/reuse the
// user's email address, not username, for login.
export function authenticate(
  userNameOrEmailAddress: string,
  password: string
): Promise<AuthenticateResponse> {
  const body: AuthenticateRequest = {
    userNameOrEmailAddress,
    password,
    rememberClient: false,
  };

  return fetchJson<AuthenticateResponse>("/api/TokenAuth/Authenticate", {
    method: "POST",
    body,
    skipAuth: true,
  });
}
