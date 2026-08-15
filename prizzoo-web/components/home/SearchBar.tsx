"use client";

import { FormEvent, useState } from "react";
import styles from "./SearchBar.module.css";

interface SearchBarProps {
  onSearch: (keyword: string) => void;
  loading: boolean;
}

export function SearchBar({ onSearch, loading }: SearchBarProps) {
  const [keyword, setKeyword] = useState("");

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (keyword.trim()) {
      onSearch(keyword.trim());
    }
  }

  return (
    <form className={styles.root} onSubmit={handleSubmit}>
      <input
        className={styles.input}
        type="text"
        placeholder="Search for a product…"
        value={keyword}
        onChange={(e) => setKeyword(e.target.value)}
      />
      <button className={styles.submit} type="submit" disabled={loading || !keyword.trim()}>
        <img src="/assets/home/search-icon.svg" alt="Search" />
      </button>
    </form>
  );
}
