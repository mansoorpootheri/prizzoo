import type { TopTab } from "./TopTabBar";
import styles from "./BottomNav.module.css";

interface BottomNavProps {
  activeTab: TopTab;
  onTabChange: (tab: TopTab) => void;
  onRefresh: () => void;
}

// Fixed bottom nav from the reference design (Home/Videos/Offers/Events +
// a center action). Shares activeTab state with TopTabBar since they
// overlap on the same three placeholder sections. The center button is a
// real action (refresh the feed) rather than a decorative dead button.
export function BottomNav({ activeTab, onTabChange, onRefresh }: BottomNavProps) {
  return (
    <nav className={styles.root}>
      <button
        type="button"
        className={activeTab === "home" ? styles.itemActive : styles.item}
        onClick={() => onTabChange("home")}
      >
        <span aria-hidden="true">⌂</span>
        Home
      </button>
      <button
        type="button"
        className={activeTab === "videos" ? styles.itemActive : styles.item}
        onClick={() => onTabChange("videos")}
      >
        <span aria-hidden="true">▶</span>
        Videos
      </button>
      <button type="button" className={styles.center} onClick={onRefresh} aria-label="Refresh">
        <span aria-hidden="true">↻</span>
      </button>
      <button
        type="button"
        className={activeTab === "offers" ? styles.itemActive : styles.item}
        onClick={() => onTabChange("offers")}
      >
        <span aria-hidden="true">◔</span>
        Offers
      </button>
      <button
        type="button"
        className={activeTab === "events" ? styles.itemActive : styles.item}
        onClick={() => onTabChange("events")}
      >
        <span aria-hidden="true">▤</span>
        Events
      </button>
    </nav>
  );
}
