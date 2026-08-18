import { fetchJson } from "./client";
import type { AuthenticateResponse, RequestOtpInput, VerifyOtpInput } from "./types";

// Phone+OTP login for shoppers - the only way a shopper account is created
// or authenticated. Both calls skip auth: RequestOtp/VerifyOtp are how an
// unauthenticated visitor becomes authenticated in the first place.
export function requestOtp(phoneNumber: string): Promise<void> {
  const body: RequestOtpInput = { phoneNumber };
  return fetchJson<void>("/api/OtpAuth/RequestOtp", {
    method: "POST",
    body,
    skipAuth: true,
  });
}

export function verifyOtp(phoneNumber: string, code: string): Promise<AuthenticateResponse> {
  const body: VerifyOtpInput = { phoneNumber, code };
  return fetchJson<AuthenticateResponse>("/api/OtpAuth/VerifyOtp", {
    method: "POST",
    body,
    skipAuth: true,
  });
}
