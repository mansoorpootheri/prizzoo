import styles from "./SpotlightCarousel.module.css";

// The reference design's "In the spotlight" promo carousel. There's no
// promo/banner backend at all (no CMS, no merchant-submitted content), so
// this stays a single generic brand slide rather than inventing fake
// merchant offers or discount claims - matches the visual pattern without
// asserting anything untrue.
export function SpotlightCarousel() {
  return (
    <div className={styles.root}>
      <div className={styles.slide}>
        <span className={styles.eyebrow}>PriZzoO.com</span>
        <span className={styles.tagline}>Compare. Locate. Save.</span>
      </div>
    </div>
  );
}
