import styles from "./CategoryTiles.module.css";

interface CategoryTilesProps {
  categories: string[];
  onSelect: (categoryName: string) => void;
}

// Same pill style as TopTabBar's tabs (see TopTabBar.module.css .tab) so the
// categories row reads as part of the same top-menu design language, not a
// separate icon-tile widget.
export function CategoryTiles({ categories, onSelect }: CategoryTilesProps) {
  if (categories.length === 0) return null;

  return (
    <div className={styles.root}>
      {categories.map((name) => (
        <button key={name} type="button" className={styles.tile} onClick={() => onSelect(name)}>
          {name}
        </button>
      ))}
    </div>
  );
}
