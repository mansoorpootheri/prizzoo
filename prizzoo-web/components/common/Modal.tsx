"use client";

import { ReactNode, useEffect } from "react";
import { createPortal } from "react-dom";
import styles from "./Modal.module.css";

interface ModalProps {
  title: string;
  onClose: () => void;
  children: ReactNode;
  // False for a mandatory dialog that can't be dismissed without completing
  // it (e.g. the shopper's first-run location picker) - suppresses Escape
  // and the × button too. Defaults to true (today's behavior). Clicking the
  // backdrop never closes a modal, dismissible or not - only Escape/× or
  // completing the dialog does, so an accidental click outside never loses
  // in-progress form input.
  dismissible?: boolean;
}

// Shared add/edit dialog for the admin panel - portaled to document.body so
// it always escapes .appShell's overflow:hidden (see app/globals.css)
// regardless of which page mounts it. Closes on Escape (when dismissible),
// never on backdrop click.
export function Modal({ title, onClose, children, dismissible = true }: ModalProps) {
  useEffect(() => {
    if (!dismissible) return;
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") onClose();
    }
    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [onClose, dismissible]);

  if (typeof document === "undefined") return null;

  return createPortal(
    <div className={styles.overlay}>
      <div className={styles.dialog}>
        <div className={styles.header}>
          <h2 className={styles.title}>{title}</h2>
          {dismissible && (
            <button type="button" className={styles.closeButton} aria-label="Close" onClick={onClose}>
              ×
            </button>
          )}
        </div>
        <div className={styles.body}>{children}</div>
      </div>
    </div>,
    document.body
  );
}
