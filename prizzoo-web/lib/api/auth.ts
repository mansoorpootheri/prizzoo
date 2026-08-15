import { fetchJson } from "./client";
import type { AuthenticateRequest, AuthenticateResponse } from "./types";

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
  });
}
