// Turns a home-feed section title (e.g. "Electronics & Mobiles") into a DOM
// id-safe slug. Shared between HomeFeed (which sets the id) and the home
// page (which scrolls to it), so category taps land on the exact section.
export function slugify(text: string): string {
  return text
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "");
}

export function homeFeedSectionId(categoryName: string): string {
  return `home-section-${slugify(categoryName)}`;
}
