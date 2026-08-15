import { fetchJson } from "./client";
import type { SubmitPriceInput } from "./types";

export function submitPrice(input: SubmitPriceInput): Promise<void> {
  return fetchJson<void>("/api/services/app/PriceSubmission/Submit", {
    method: "POST",
    body: input,
  });
}
