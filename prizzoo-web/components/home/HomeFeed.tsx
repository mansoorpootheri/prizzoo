import type { HomeFeedSection, StorePriceResult } from "@/lib/api/types";
import { PriceResultCard } from "@/components/common/PriceResultCard";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { EmptyState } from "@/components/common/EmptyState";
import { homeFeedSectionId } from "@/lib/utils/slugify";
import styles from "./HomeFeed.module.css";

interface HomeFeedProps {
  sections: HomeFeedSection[] | null;
  loading: boolean;
  error: string | null;
  onSelect: (result: StorePriceResult) => void;
}

export function HomeFeed({ sections, loading, error, onSelect }: HomeFeedProps) {
  if (loading && !sections) {
    return (
      <div className={styles.loading}>
        <LoadingSpinner />
      </div>
    );
  }

  if (error) {
    return (
      <div className={styles.root}>
        <ErrorBanner message={error} />
      </div>
    );
  }

  if (!sections || sections.length === 0) {
    return <EmptyState message="No products listed near you yet." />;
  }

  return (
    <div className={styles.root}>
      {sections.map((section) => (
        <section key={section.title} id={homeFeedSectionId(section.title)} className={styles.section}>
          <h2 className={styles.sectionTitle}>{section.title}</h2>
          <div className={styles.row}>
            {section.items.map((item) => (
              <div key={item.priceId} className={styles.cardWrap}>
                <PriceResultCard result={item} onClick={() => onSelect(item)} />
              </div>
            ))}
          </div>
        </section>
      ))}
    </div>
  );
}
