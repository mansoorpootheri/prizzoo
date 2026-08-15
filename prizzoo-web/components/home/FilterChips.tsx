import styles from "./FilterChips.module.css";

export type SortMode = "all" | "nearby" | "latest";

const SORT_CHIPS: { mode: SortMode; label: string }[] = [
  { mode: "all", label: "All" },
  { mode: "nearby", label: "Nearby" },
  { mode: "latest", label: "Latest" },
];

// StorePriceResultDto carries no category field, so these can only set the
// search keyword rather than filter structured category data.
const KEYWORD_CHIPS = ["Grocery", "Electronics", "Fashion", "Food"];

interface FilterChipsProps {
  activeSort: SortMode;
  onSortChange: (mode: SortMode) => void;
  onKeywordChip: (keyword: string) => void;
}

export function FilterChips({ activeSort, onSortChange, onKeywordChip }: FilterChipsProps) {
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
      {KEYWORD_CHIPS.map((label) => (
        <button
          key={label}
          type="button"
          className={styles.chip}
          onClick={() => onKeywordChip(label)}
        >
          {label}
        </button>
      ))}
    </div>
  );
}
