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

export interface RegisterRequest {
  name: string;
  surname: string;
  userName: string;
  emailAddress: string;
  password: string;
}

export interface RegisterResponse {
  canLogin: boolean;
}

export interface ApplyRetailerInput {
  name: string;
  address?: string;
  city?: string;
  phone?: string;
  latitude: number;
  longitude: number;
  openingHours?: string;
  categoryTags?: string;
  imageId?: string;
}

export interface RetailerApplicationStatus {
  hasApplied: boolean;
  isApproved: boolean;
  storeId: string | null;
  storeName: string | null;
}

export interface RetailerApplicationListItem {
  id: string;
  storeName: string;
  city: string | null;
  ownerUserId: number | null;
  ownerName: string | null;
  ownerEmail: string | null;
  creationTime: string;
}

export interface UpdateMyStoreInput {
  name: string;
  address?: string;
  city?: string;
  phone?: string;
  latitude: number;
  longitude: number;
  openingHours?: string;
  categoryTags?: string;
  imageId?: string;
}

export interface MyStore {
  id: string;
  name: string;
  address: string | null;
  city: string | null;
  phone: string | null;
  latitude: number;
  longitude: number;
  openingHours: string | null;
  categoryTags: string | null;
  isVerified: boolean;
  isActive: boolean;
  imageId: string | null;
}

export interface ComboboxItem {
  value: string;
  displayText: string;
}

export interface CreateProductInput {
  name: string;
  barcode?: string;
  description?: string;
  categoryId?: string;
  unitId?: string;
  imageId?: string;
}

export interface MyProduct {
  id: string;
  name: string;
  barcode: string | null;
  description: string | null;
  categoryId: string | null;
  categoryName: string | null;
  unitId: string | null;
  unitName: string | null;
  imageId: string | null;
  isActive: boolean;
}

export interface SubmitPriceInput {
  productId: string;
  storeId: string;
  amount: number;
  proofImageId?: string;
}
