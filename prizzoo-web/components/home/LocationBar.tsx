"use client";

import { FormEvent, RefObject, useState } from "react";
import styles from "./LocationBar.module.css";

interface LocationBarProps {
  isLocating: boolean;
  isFallback: boolean;
  searchOpen: boolean;
  onToggleSearch: () => void;
  onSearch: (keyword: string) => void;
  searchLoading: boolean;
  searchInputRef: RefObject<HTMLInputElement | null>;
  menuOpen: boolean;
  onToggleMenu: () => void;
  onLogout: () => void;
  /** True when the logged-in user has the Admin role (see AuthContext.isAdmin) - shows the "Admin" menu item below. */
  isAdmin: boolean;
  onGoToAdmin: () => void;
}

// Matches DOCS/topbar.png: pin badge + city/country on the left, always
// visible - search/profile icons on the right. Tapping the search icon
// swaps that right-hand area for an inline search field that grows out
// from its own left edge (right where the location block ends), rather
// than a separate row below or an overlay that hides the location. The
// profile circle is a real account menu - Logout always, plus "Admin" when
// the logged-in user has that role (there's no separate admin login/portal
// entry point anymore - everyone lands here, admin reaches the admin
// screens from this menu).
export function LocationBar({
  isLocating,
  isFallback,
  searchOpen,
  onToggleSearch,
  onSearch,
  searchLoading,
  searchInputRef,
  menuOpen,
  onToggleMenu,
  onLogout,
  isAdmin,
  onGoToAdmin,
}: LocationBarProps) {
  const [keyword, setKeyword] = useState("");

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (keyword.trim()) {
      onSearch(keyword.trim());
    }
  }

  function handleCancel() {
    setKeyword("");
    onToggleSearch();
  }

  return (
    <div className={styles.root}>
      <div className={styles.row}>
        <div className={styles.left}>
          <span className={styles.pinBadge} aria-hidden="true">📍</span>
          <div className={styles.text}>
            <span className={styles.city}>{isLocating ? "Locating…" : "Kozhikode"}</span>
            <span className={styles.country}>
              {isFallback ? "Showing default results" : "India"}
            </span>
          </div>
        </div>
        <div className={styles.rightArea}>
          <div className={`${styles.icons} ${searchOpen ? styles.iconsHidden : ""}`} aria-hidden={searchOpen}>
            <button type="button" className={styles.iconButton} onClick={onToggleSearch} aria-label="Search">
              <img src="/assets/home/search-icon.svg" alt="" aria-hidden="true" />
            </button>
            <div className={styles.menuWrap}>
              <button type="button" className={styles.avatar} onClick={onToggleMenu} aria-label="Account menu">
                ☺
              </button>
              {menuOpen && (
                <div className={styles.menu}>
                  {isAdmin && (
                    <button type="button" className={styles.menuItem} onClick={onGoToAdmin}>
                      Admin
                    </button>
                  )}
                  <button type="button" className={styles.menuItem} onClick={onLogout}>
                    Logout
                  </button>
                </div>
              )}
            </div>
          </div>
          <form
            className={`${styles.searchInline} ${searchOpen ? styles.searchInlineOpen : ""}`}
            onSubmit={handleSubmit}
            aria-hidden={!searchOpen}
          >
            <span className={styles.searchIcon} aria-hidden="true">
              <img src="/assets/home/search-icon.svg" alt="" />
            </span>
            <input
              ref={searchInputRef}
              className={styles.searchInput}
              type="text"
              placeholder="Search for a product…"
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
              disabled={searchLoading}
              tabIndex={searchOpen ? 0 : -1}
            />
            <button
              type="button"
              className={styles.cancelButton}
              onClick={handleCancel}
              tabIndex={searchOpen ? 0 : -1}
            >
              Cancel
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
