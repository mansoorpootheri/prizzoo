// Persists the location a shopper picked (see LocationPickerModal) - the
// sole source of coordinates for /home and /product searches now, replacing
// the old raw-device-geolocation flow. Mirrors lib/auth/token-storage.ts's
// localStorage pattern exactly, including the SSR-safe window guards.
export interface ShopperLocation {
  locationId: string;
  locationName: string;
  districtId: number;
  districtName: string;
  latitude: number;
  longitude: number;
}

const KEY = "prizzoo.shopperLocation";

export function getShopperLocation(): ShopperLocation | null {
  if (typeof window === "undefined") return null;
  const raw = window.localStorage.getItem(KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as ShopperLocation;
  } catch {
    return null; // corrupt/old-shape value - treat as "not picked yet"
  }
}

export function setShopperLocation(location: ShopperLocation): void {
  if (typeof window === "undefined") return;
  window.localStorage.setItem(KEY, JSON.stringify(location));
}

export function clearShopperLocation(): void {
  if (typeof window === "undefined") return;
  window.localStorage.removeItem(KEY);
}
