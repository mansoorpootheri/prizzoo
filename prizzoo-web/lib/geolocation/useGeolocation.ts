"use client";

import { useEffect, useState } from "react";
import { DEFAULT_LOCATION, type Coordinates } from "./default-location";

interface GeolocationState {
  coordinates: Coordinates;
  isLocating: boolean;
  isFallback: boolean;
}

export function useGeolocation(): GeolocationState {
  const [state, setState] = useState<GeolocationState>({
    coordinates: DEFAULT_LOCATION,
    isLocating: true,
    isFallback: false,
  });

  useEffect(() => {
    if (typeof navigator === "undefined" || !navigator.geolocation) {
      // No synchronous browser API to subscribe to here - just report the
      // fallback once after mount.
      // eslint-disable-next-line react-hooks/set-state-in-effect
      setState({
        coordinates: DEFAULT_LOCATION,
        isLocating: false,
        isFallback: true,
      });
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        setState({
          coordinates: {
            latitude: position.coords.latitude,
            longitude: position.coords.longitude,
          },
          isLocating: false,
          isFallback: false,
        });
      },
      () => {
        setState({
          coordinates: DEFAULT_LOCATION,
          isLocating: false,
          isFallback: true,
        });
      },
      // Without a timeout, a slow/unresponsive OS location service leaves
      // isLocating stuck true forever with no fallback ever firing.
      { timeout: 8000 }
    );
  }, []);

  return state;
}
