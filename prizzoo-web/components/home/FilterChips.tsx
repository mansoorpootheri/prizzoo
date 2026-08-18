import styles from "./FilterChips.module.css";

export type SortMode = "all" | "nearby" | "latest";

const SORT_CHIPS: { mode: SortMode; label: string }[] = [
  { mode: "all", label: "All" },
  { mode: "nearby", label: "Nearby" },
  { mode: "latest", label: "Latest" },
];

interface FilterChipsProps {
  activeSort: SortMode;
  onSortChange: (mode: SortMode) => void;
}

export function FilterChips({ activeSort, onSortChange }: FilterChipsProps) {
  return (
    <div className={styles.root}>
      {SORT_CHIPS.map(({ mode, label }) => (
        <button
          key={mode}
          type="button"
          className={activeSort === mode ? styles.chipActive : styles.chip}
          onClick={() => onSortChange(mode)}
        >
          {label}
        </button>
      ))}
    </div>
  );
}
