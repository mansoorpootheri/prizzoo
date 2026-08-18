import { fetchJson } from "./client";
import type { RateProductInput, MyProductRating } from "./types";

export function rateProduct(input: RateProductInput): Promise<void> {
  return fetchJson<void>("/api/services/app/ProductRating/RateProduct", {
    method: "POST",
    body: input,
  });
}

export function getMyRating(productId: string): Promise<MyProductRating> {
  return fetchJson<MyProductRating>(
    `/api/services/app/ProductRating/GetMyRating?productId=${encodeURIComponent(productId)}`
  );
}
