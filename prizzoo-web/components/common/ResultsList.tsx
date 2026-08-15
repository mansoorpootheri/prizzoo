import type { StorePriceResult } from "@/lib/api/types";
import { PriceResultCard } from "./PriceResultCard";
import { EmptyState } from "./EmptyState";
import { LoadingSpinner } from "./LoadingSpinner";
import { ErrorBanner } from "./ErrorBanner";
import styles from "./ResultsList.module.css";

interface ResultsListProps {
  data: StorePriceResult[] | null;
  loading: boolean;
  error: string | null;
  emptyMessage: string;
  onSelect: (result: StorePriceResult) => void;
}

export function ResultsList({ data, loading, error, emptyMessage, onSelect }: ResultsListProps) {
  if (loading) {
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

  if (!data || data.length === 0) {
    return <EmptyState message={emptyMessage} />;
  }

  return (
    <div className={styles.root}>
      {data.map((result) => (
        <PriceResultCard
          key={result.priceId}
          result={result}
          onClick={() => onSelect(result)}
        />
      ))}
    </div>
  );
}
