import styles from "./TopTabBar.module.css";

export type TopTab = "home" | "videos" | "offers" | "events";
export type SortMode = "all" | "nearby" | "latest";

const SORT_CHIPS: { mode: SortMode; label: string }[] = [  
  { mode: "nearby", label: "Nearby" },
  { mode: "latest", label: "Latest" },
];

interface TopTabBarProps {
  categories: string[];
  sortMode: SortMode;
  onSortChange: (mode: SortMode) => void;
  onHome: () => void;
  onCategorySelect: (categoryName: string) => void;
}

// Real Category master data (plus the sort chips that used to live in their
// own row below) fills the top bar instead of the reference design's
// Videos/Offers/Events tabs - those have no backend at all (see
// PRIZZOO_MIGRATION_NOTES "Known gaps"), so working shortcuts here beat
// faking pages that go nowhere. Retailers isn't a separate screen either -
// it's a real section already on the Home tab, so tapping it just scrolls
// to that section; likewise each category tap jumps to that category's row
// in the home feed rather than opening a new page.
export function TopTabBar({
  categories,
  sortMode,
  onSortChange,
  onHome,
  onCategorySelect,
}: TopTabBarProps) {
  return (
    <div className={styles.root}>
      <button
        type="button"
        className={styles.home}
        onClick={() => {
          onSortChange("all");
          onHome();
        }}
        aria-label="Home"
      >
        <span aria-hidden="true">⌂</span>
      </button>
      {SORT_CHIPS.map(({ mode, label }) => (
        <button
          key={mode}
          type="button"
          className={sortMode === mode ? styles.tabActive : styles.tab}
          onClick={() => onSortChange(mode)}
        >
          {label}
        </button>
      ))}
      {categories.map((name) => (
        <button key={name} type="button" className={styles.tab} onClick={() => onCategorySelect(name)}>
          {name}
        </button>
      ))}
    </div>
  );
}
