export interface Coordinates {
  latitude: number;
  longitude: number;
}

// TODO: set to the MVP launch city's coordinates once chosen.
export const DEFAULT_LOCATION: Coordinates = {
  latitude: Number(process.env.NEXT_PUBLIC_DEFAULT_LATITUDE ?? 0),
  longitude: Number(process.env.NEXT_PUBLIC_DEFAULT_LONGITUDE ?? 0),
};
