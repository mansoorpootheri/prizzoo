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
  flyerId?: string;
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

export interface AdminUser {
  id: number;
  phoneNumber: string | null;
  creationTime: string;
}

export interface AddAdminInput {
  phoneNumber: string;
}

export interface RegisteredUser {
  id: number;
  phoneNumber: string | null;
  creationTime: string;
  isActive: boolean;
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
export interface CreateStoreInput {
  name: string;
  address?: string;
  city?: string;
  // The specific locality within a district, e.g. "Feroke" within
  // "Kozhikode" - mandatory, it's now the store's only source of
  // coordinates (the backend derives both City and Latitude/Longitude from
  // it; there is no separate lat/lng input any more).
  locationId: string;
  phone?: string;
  openingHours?: string;
  categoryTags?: string;
  imageId?: string;
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

export interface AdminStoreDetail {
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

export interface UpdateStoreInput {
  id: string;
  name: string;
  address?: string;
  city?: string;
  // Mandatory - see CreateStoreInput.locationId.
  locationId: string;
  phone?: string;
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
  // The locality's approximate centroid, captured via the admin's "use my
  // current location" action - the sole source of a store's coordinates
  // (see StoreAppService) and of a shopper's picked-location coordinates
  // (see the home-screen LocationPickerModal). Null only for legacy rows
  // created before this field was required.
  latitude: number | null;
  longitude: number | null;
  isActive: boolean;
}

export interface CreateEditLocationInput {
  id?: string;
  name: string;
  districtId: number;
  // Required - captured only via device geolocation, never typed by hand.
  latitude: number;
  longitude: number;
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

export interface AdminUnit {
  id: string;
  name: string;
  code: string;
  decimalPlaces: number;
  isActive: boolean;
}

export interface CreateEditUnitInput {
  id?: string;
  name: string;
  code: string;
  decimalPlaces: number;
  isActive: boolean;
}

export interface AdminProduct {
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

export interface CreateEditProductInput {
  id?: string;
  name: string;
  barcode?: string;
  description?: string;
  categoryId?: string;
  unitId?: string;
  imageId?: string;
  isActive: boolean;
}

export interface SubmitPriceInput {
  productId: string;
  storeId: string;
  amount: number;
  originalAmount?: number;
  proofImageId?: string;
}

// Matches EnterpriseBase.Pricing.PriceSource - sent/received as the raw
// numeric value on the wire, same convention as PriceStatus above.
export enum PriceSourceKind {
  RetailerReported = 1,
  Crowdsourced = 2,
  OndcFeed = 3,
  FieldCollected = 4,
  Flyer = 5,
}

export interface AdminPrice {
  id: string;
  productId: string;
  productName: string;
  storeId: string;
  storeName: string;
  amount: number;
  originalAmount: number | null;
  status: PriceStatus;
  source: PriceSourceKind;
  observedAt: string;
}

export interface UpdatePriceInput {
  id: string;
  amount: number;
  originalAmount?: number;
  status: PriceStatus;
}

export interface RateProductInput {
  productId: string;
  stars: number;
}

export interface MyProductRating {
  productId: string;
  stars: number | null;
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

export interface FlyerLineItemInput {
  // Exactly one of productId or name must be set: productId picks an
  // existing catalog product directly; name (+ optional categoryId)
  // creates a new one, or reuses an exact case-insensitive name match.
  productId?: string;
  name?: string;
  categoryId?: string;
  price: number;
  originalAmount?: number;
}

export interface CreateFlyerForStoreInput {
  storeId: string;
  imageId: string;
  items: FlyerLineItemInput[];
}

// Add more items to a flyer that's already live - see FlyerAppService.AddItemsToFlyerAsync.
export interface AddFlyerItemsInput {
  flyerId: string;
  items: FlyerLineItemInput[];
}

export interface Flyer {
  id: string;
  storeId: string;
  imageId: string;
  uploadedAt: string;
  itemCount: number;
}

export interface FlyerLineItemResult {
  productName: string;
  amount: number;
  originalAmount: number | null;
}

// Live view of a store's flyer - the photo plus every item typed in
// alongside it, each a real (Approved) Price row. See
// FlyerAppService.GetLiveFlyerForStoreAsync.
export interface FlyerDetail {
  id: string;
  storeId: string;
  storeName: string | null;
  imageId: string;
  items: FlyerLineItemResult[];
}
