"use client";

import { FormEvent, useEffect, useState } from "react";
import { addItemsToFlyer } from "@/lib/api/flyer";
import { getProductsForCombobox } from "@/lib/api/adminCatalog";
import { getPrices } from "@/lib/api/priceSubmission";
import { ApiError } from "@/lib/api/client";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { PriceStatus, type ComboboxItem } from "@/lib/api/types";
import fieldStyles from "./AdminForm.module.css";
import itemStyles from "./UploadFlyerForStoreForm.module.css";

interface ItemRow {
  productId: string;
  price: string;
  originalAmount: string;
}

function emptyRow(): ItemRow {
  return { productId: "", price: "", originalAmount: "" };
}

interface AddFlyerItemsFormProps {
  flyerId: string;
  // The flyer's store - used to look up that product's last approved price
  // at this store, so picking a product can prefill price/MRP.
  storeId: string;
  // Called after every successful save (stays open, mirroring
  // AddStorePriceForm - an admin adding items is usually about to add
  // several in a row). The parent should re-run its list load() here.
  onSaved: () => void;
}

// Existing-products-only item picker for adding items to a flyer that's
// already live. Deliberately doesn't offer "create a new product" the way
// the original upload form does - in practice a flyer's items are almost
// always products that were already created via Manage products, so this
// form just maps to one directly rather than risking an accidental
// duplicate-ish new product here.
export function AddFlyerItemsForm({ flyerId, storeId, onSaved }: AddFlyerItemsFormProps) {
  const [products, setProducts] = useState<ComboboxItem[]>([]);
  const [loadingOptions, setLoadingOptions] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [rows, setRows] = useState<ItemRow[]>([emptyRow()]);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    getProductsForCombobox()
      .then(setProducts)
      .catch((err) => setLoadError(err instanceof ApiError ? err.message : "Could not load products."))
      .finally(() => setLoadingOptions(false));
  }, []);

  function updateRow(index: number, patch: Partial<ItemRow>) {
    setRows((prev) => prev.map((row, i) => (i === index ? { ...row, ...patch } : row)));
  }

  function addRow() {
    setRows((prev) => [...prev, emptyRow()]);
  }

  // Picking a product prefills price/MRP from that product's most recent
  // approved price at this store, if any - most of the time a flyer item is
  // just re-confirming (or lightly correcting) a price that's already on
  // record, so retyping it from scratch is unnecessary. Clears both fields
  // first so a stale prefill never lingers when switching products.
  async function handleProductChange(index: number, productId: string) {
    updateRow(index, { productId, price: "", originalAmount: "" });
    if (!productId) return;

    try {
      const result = await getPrices({ storeId, productId, status: PriceStatus.Approved });
      const latest = result.items[0];
      if (latest) {
        updateRow(index, {
          price: String(latest.amount),
          originalAmount: latest.originalAmount != null ? String(latest.originalAmount) : "",
        });
      }
    } catch {
      // No prefill if this lookup fails - the admin can still type the price manually.
    }
  }

  function removeRow(index: number) {
    setRows((prev) => prev.filter((_, i) => i !== index));
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setSaved(false);

    const completeRows = rows.filter((row) => row.productId && row.price);
    if (completeRows.length === 0) {
      setError("Add at least one item - pick a product and a price.");
      return;
    }

    setSubmitting(true);
    try {
      await addItemsToFlyer({
        flyerId,
        items: completeRows.map((row) => ({
          productId: row.productId,
          price: Number(row.price),
          originalAmount: row.originalAmount ? Number(row.originalAmount) : undefined,
        })),
      });
      setRows([emptyRow()]);
      setSaved(true);
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not add these items.");
    } finally {
      setSubmitting(false);
    }
  }

  if (loadingOptions) {
    return <LoadingSpinner />;
  }

  if (loadError) {
    return <ErrorBanner message={loadError} />;
  }

  return (
    <form className={fieldStyles.form} onSubmit={handleSubmit}>
      <p className={fieldStyles.body}>
        Adds more items to this flyer - goes live immediately, same as the original upload.
      </p>

      <div className={itemStyles.itemList}>
        {rows.map((row, index) => (
          <div key={index} className={itemStyles.itemCard}>
            <div className={itemStyles.itemCardTop}>
              <select
                className={fieldStyles.field}
                value={row.productId}
                onChange={(e) => void handleProductChange(index, e.target.value)}
              >
                <option value="" disabled>
                  Select a product
                </option>
                {products.map((p) => (
                  <option key={p.value} value={p.value}>
                    {p.displayText}
                  </option>
                ))}
              </select>
              {rows.length > 1 && (
                <button type="button" className={fieldStyles.itemRemove} onClick={() => removeRow(index)}>
                  Remove
                </button>
              )}
            </div>

            <div className={itemStyles.itemRow}>
              <input
                className={fieldStyles.field}
                type="number"
                min="0.01"
                step="0.01"
                placeholder="Price"
                value={row.price}
                onChange={(e) => updateRow(index, { price: e.target.value })}
              />
              <input
                className={fieldStyles.field}
                type="number"
                min="0.01"
                step="0.01"
                placeholder="MRP (optional)"
                value={row.originalAmount}
                onChange={(e) => updateRow(index, { originalAmount: e.target.value })}
              />
            </div>
          </div>
        ))}
      </div>
      <button type="button" className={fieldStyles.addItemButton} onClick={addRow}>
        + Add another item
      </button>

      {error && <ErrorBanner message={error} />}
      {saved && !error && <p className={fieldStyles.body}>Added - close this dialog or add more.</p>}

      <button className={fieldStyles.submit} type="submit" disabled={submitting}>
        {submitting ? <LoadingSpinner /> : "Add to flyer"}
      </button>
    </form>
  );
}
