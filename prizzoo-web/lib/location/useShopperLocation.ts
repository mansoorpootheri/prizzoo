"use client";

import { useCallback, useEffect, useState } from "react";
import {
  clearShopperLocation,
  getShopperLocation,
  setShopperLocation,
  type ShopperLocation,
} from "./shopperLocation";

interface UseShopperLocationResult {
  location: ShopperLocation | null;
  // True once the initial (client-only) localStorage read has happened -
  // same "reveal after mount" pattern as AuthContext's isReady, since
  // localStorage isn't available during SSR/the first client render.
  isReady: boolean;
  setLocation: (location: ShopperLocation) => void;
  clearLocation: () => void;
}

export function useShopperLocation(): UseShopperLocationResult {
  const [location, setLocationState] = useState<ShopperLocation | null>(null);
  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setLocationState(getShopperLocation());
    setIsReady(true);
  }, []);

  const setLocation = useCallback((loc: ShopperLocation) => {
    setShopperLocation(loc);
    setLocationState(loc);
  }, []);

  const clearLocation = useCallback(() => {
    clearShopperLocation();
    setLocationState(null);
  }, []);

  return { location, isReady, setLocation, clearLocation };
}
