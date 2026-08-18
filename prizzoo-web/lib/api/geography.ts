import { fetchJson } from "./client";
import type { ComboboxItem } from "./types";

// Country/State are fixed constants for MVP (India/Kerala, auto-seeded by
// DefaultGeographyCreator) - no admin master data needed for those. District
// plays the "city" role in the store-creation form (e.g. "Kozhikode").
const BASE = "/api/services/app/Geography";

export function getCountriesForCombobox(): Promise<ComboboxItem[]> {
  return fetchJson<ComboboxItem[]>(`${BASE}/GetCountriesForCombobox`);
}

export function getStatesByCountryForCombobox(countryId: number): Promise<ComboboxItem[]> {
  return fetchJson<ComboboxItem[]>(`${BASE}/GetStatesByCountryForCombobox?countryId=${countryId}`);
}

export function getDistrictsByStateForCombobox(stateId: number): Promise<ComboboxItem[]> {
  return fetchJson<ComboboxItem[]>(`${BASE}/GetDistrictsByStateForCombobox?stateId=${stateId}`);
}

// Convenience: resolves straight to Kerala's districts, since Country=India/
// State=Kerala are the only seeded values for MVP - avoids every caller
// re-doing the two-step country->state lookup just to get the district list.
export async function getKeralaDistrictsForCombobox(): Promise<ComboboxItem[]> {
  const countries = await getCountriesForCombobox();
  const india = countries.find((c) => c.displayText === "India") ?? countries[0];
  if (!india) return [];

  const states = await getStatesByCountryForCombobox(Number(india.value));
  const kerala = states.find((s) => s.displayText === "Kerala") ?? states[0];
  if (!kerala) return [];

  return getDistrictsByStateForCombobox(Number(kerala.value));
}
