export interface AuthenticateRequest {
  userNameOrEmailAddress: string;
  password: string;
  rememberClient: boolean;
}

export interface AuthenticateResponse {
  accessToken: string;
  encryptedAccessToken: string;
  expireInSeconds: number;
  userId: number;
}

export interface ComparePricesInput {
  productKeyword: string;
  latitude: number;
  longitude: number;
  radiusKm?: number;
  maxResults?: number;
}

export interface StorePriceResult {
  priceId: string;
  productId: string;
  productName: string;
  storeId: string;
  storeName: string;
  storeAddress: string;
  latitude: number;
  longitude: number;
  distanceKm: number;
  amount: number;
  currency: string;
  observedAt: string;
  isStale: boolean;
}
