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
  // Exactly one of these is normally set: productKeyword for a free-text
  // search, categoryName for browsing a category chip. A category name is
  // never a substring of a product name, so it must go in its own field -
  // sending it as productKeyword always returns zero results.
  productKeyword?: string;
  categoryName?: string;
  storeId?: string;
  latitude: number;
  longitude: number;
  radiusKm?: number;
  maxResults?: number;
}

export interface StorePriceResult {
  priceId: string;
  productId: string;
  productName: string;
  productImageId: string | null;
  storeId: string;
  storeName: string;
  storeImageId: string | null;
  storeAddress: string;
  latitude: number;
  longitude: number;
  distanceKm: number;
  amount: number;
  // MRP/original price and derived discount %, only present when the
  // submitter provided an original price and it exceeds the current amount.
  originalAmount: number | null;
  discountPercent: number | null;
  currency: string;
  observedAt: string;
  isStale: boolean;
  // Average of all shopper star ratings (1-5) for this product. Null/0 means
  // no one has rated it yet - render "No ratings yet", not a fake 0-star.
  averageRating: number | null;
  ratingCount: number;
}

export interface HomeFeedInput {
  latitude: number;
  longitude: number;
  radiusKm?: number;
}

// One horizontal row on the home screen: "Top Picks for You" or a real
// catalog category name - never hardcoded, built server-side from actual
// nearby approved prices. See PriceCompareAppService.GetHomeFeedAsync.
export interface HomeFeedSection {
  title: string;
  items: StorePriceResult[];
}

export interface RequestOtpInput {
  phoneNumber: string;
}

export interface VerifyOtpInput {
  phoneNumber: string;
  code: string;
}

// Host admin creates the shop AND its owner login in one action - there is
// no self-service retailer signup. See StoreAppService.CreateAsync.
export interface CreateStoreWithOwnerInput {
  name: string;
  address?: string;
  city?: string;
  // The specific locality within a district, e.g. "Feroke" within
  // "Kozhikode" - when set, the backend derives City from it automatically.
  locationId?: string;
  phone?: string;
  latitude: number;
  longitude: number;
  openingHours?: string;
  categoryTags?: string;
  imageId?: string;
  ownerName: string;
  ownerSurname?: string;
  ownerEmail: string;
  ownerPhoneNumber?: string;
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
  locationId: string | null;
  locationName: string | null;
  // Location's parent district id - lets an edit form preselect the same
  // City -> Location cascade used at creation.
  districtId: number | null;
  phone: string | null;
  latitude: number;
  longitude: number;
  openingHours: string | null;
  categoryTags: string | null;
  isVerified: boolean;
  isActive: boolean;
  imageId: string | null;
}

// Response from Store/Create - only that call ever populates
// ownerTemporaryPassword, and only once, for the admin to relay manually.
export interface CreatedStore extends MyStore {
  ownerTemporaryPassword: string | null;
}

// Admin store list/detail view - same shape as MyStore (ShopOwner's own
// store) since both come from the same StoreDto on the backend.
export type AdminStoreDetail = MyStore;

export interface UpdateStoreInput {
  id: string;
  name: string;
  address?: string;
  city?: string;
  locationId?: string;
  phone?: string;
  latitude: number;
  longitude: number;
  openingHours?: string;
  categoryTags?: string;
  imageId?: string;
  isVerified: boolean;
  isActive: boolean;
}

export interface ComboboxItem {
  value: string;
  displayText: string;
}

export interface AdminLocation {
  id: string;
  name: string;
  districtId: number;
  districtName: string | null;
  // The locality's approximate centroid - lets a shopper's live GPS
  // position be matched against these to auto-detect which Location
  // they're in. Null for locations added before this field existed.
  latitude: number | null;
  longitude: number | null;
  isActive: boolean;
}

export interface CreateEditLocationInput {
  id?: string;
  name: string;
  districtId: number;
  latitude?: number;
  longitude?: number;
  isActive: boolean;
}

export interface AdminCategory {
  id: string;
  name: string;
  code: string | null;
  description: string | null;
  isActive: boolean;
}

export interface CreateEditCategoryInput {
  id?: string;
  name: string;
  code?: string;
  description?: string;
  isActive: boolean;
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
  originalAmount?: number;
  proofImageId?: string;
}

// Shop owner: no storeId - the server always resolves the caller's own
// store, so a shop owner can never submit a price for a store they don't own.
export interface SubmitMyPriceInput {
  productId: string;
  amount: number;
  originalAmount?: number;
  proofImageId?: string;
}

export interface RateProductInput {
  productId: string;
  stars: number;
}

export interface MyProductRating {
  productId: string;
  stars: number | null;
}

export interface MyStoreProduct {
  productId: string;
  productName: string | null;
  categoryName: string | null;
  unitName: string | null;
  imageId: string | null;
  latestPriceAmount: number | null;
  latestPriceStatus: PriceStatus | null;
  latestPriceObservedAt: string | null;
}

export interface PendingPrice {
  id: string;
  productName: string;
  storeName: string;
  amount: number;
  proofImageId: string | null;
  observedAt: string;
}

// Matches EnterpriseBase.Pricing.PriceStatus - values must match the backend
// enum exactly, this is sent as the raw numeric value on the wire.
export enum PriceStatus {
  Pending = 1,
  Approved = 2,
  Flagged = 3,
  Rejected = 4,
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
}
