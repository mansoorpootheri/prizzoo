"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { getStores } from "@/lib/api/adminStores";
import { getCategoriesForCombobox, getProductsForCombobox } from "@/lib/api/adminCatalog";
import { createFlyerForStore } from "@/lib/api/flyer";
import { imageUrl } from "@/lib/api/image";
import type { AdminStoreDetail, ComboboxItem } from "@/lib/api/types";
import { ApiError } from "@/lib/api/client";
import { ErrorBanner } from "@/components/common/ErrorBanner";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import { ImageUploadField } from "@/components/common/ImageUploadField";
import fieldStyles from "./AdminForm.module.css";
import styles from "./UploadFlyerForStoreForm.module.css";

// Distinct from "" (unselected/no choice made yet) so the dropdown can
// start on a neutral "Select a product" placeholder rather than defaulting
// straight into "+ New product" - most flyer items map to an existing
// product, so that shouldn't be the default state.
const NEW_PRODUCT_VALUE = "__new__";

interface ItemRow {
  productId: string; // "" = nothing picked yet, NEW_PRODUCT_VALUE = "create a new product"
  name: string;
  categoryId: string;
  price: string;
}

function emptyRow(): ItemRow {
  return { productId: "", name: "", categoryId: "", price: "" };
}

// Admin uploads a flyer photo for a store and types in the item list
// alongside it, in one step - no OCR, no separate moderation queue. Each
// item either picks an existing catalog product or creates a new one
// (optionally under a category). Goes live immediately, since the admin's
// own submission is the trusted source - there is no separate moderation
// step for it.
export function UploadFlyerForStoreForm() {
  const router = useRouter();
  const { isAuthenticated, isAdmin, isReady } = useAuth();
  const [stores, setStores] = useState<AdminStoreDetail[]>([]);
  const [products, setProducts] = useState<ComboboxItem[]>([]);
  const [categories, setCategories] = useState<ComboboxItem[]>([]);
  const [storeId, setStoreId] = useState("");
  const [imageId, setImageId] = useState<string | null>(null);
  const [items, setItems] = useState<ItemRow[]>([emptyRow()]);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/phone-entry");
    } else if (isReady && isAuthenticated && !isAdmin) {
      router.replace("/home");
    }
  }, [isReady, isAuthenticated, isAdmin, router]);

  useEffect(() => {
    if (!isReady || !isAuthenticated || !isAdmin) return;
    Promise.all([getStores(), getProductsForCombobox(), getCategoriesForCombobox()])
      .then(([storesResult, productsResult, categoriesResult]) => {
        setStores(storesResult.items);
        setProducts(productsResult);
        setCategories(categoriesResult);
      })
      .catch(() => {
        // Empty pickers just mean typing everything by hand isn't possible
        // yet; not worth blocking the page over.
      });
  }, [isReady, isAuthenticated, isAdmin]);

  function updateItem(index: number, patch: Partial<ItemRow>) {
    setItems((prev) => prev.map((row, i) => (i === index ? { ...row, ...patch } : row)));
  }

  function addItem() {
    setItems((prev) => [...prev, emptyRow()]);
  }

  function removeItem(index: number) {
    setItems((prev) => prev.filter((_, i) => i !== index));
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);

    if (!storeId) {
      setError("Select a store first.");
      return;
    }
    if (!imageId) {
      setError("Add a photo of the flyer first.");
      return;
    }
    const completeItems = items.filter((row) => {
      if (!row.price) return false;
      return row.productId === NEW_PRODUCT_VALUE ? row.name.trim().length > 0 : row.productId !== "";
    });
    if (completeItems.length === 0) {
      setError("Add at least one item with a price, and either a product name or an existing product.");
      return;
    }

    setSubmitting(true);
    try {
      await createFlyerForStore({
        storeId,
        imageId,
        items: completeItems.map((row) =>
          row.productId === NEW_PRODUCT_VALUE
            ? { name: row.name.trim(), categoryId: row.categoryId || undefined, price: Number(row.price) }
            : { productId: row.productId, price: Number(row.price) }
        ),
      });
      router.replace("/admin/dashboard");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not upload this flyer.");
    } finally {
      setSubmitting(false);
    }
  }

  if (!isReady || !isAuthenticated || !isAdmin) {
    return null;
  }

  return (
    <div className={styles.page}>
      <div className={styles.headerRow}>
        <button type="button" className={styles.back} onClick={() => router.back()}>
          ← Back
        </button>
        <button type="button" className={styles.back} onClick={() => router.push("/admin/dashboard")}>
          🏠 Dashboard
        </button>
      </div>
      <h1 className={styles.heading}>Upload a flyer for a store</h1>
      <p className={styles.subheading}>
        Pick a store and photo, then list the items as you see them on the flyer. It goes live for
        shoppers as soon as you submit.
      </p>

      <form className={styles.layout} onSubmit={handleSubmit}>
        <div className={styles.left}>
          <label className={fieldStyles.label}>
            Store
            <select
              className={fieldStyles.field}
              value={storeId}
              onChange={(e) => setStoreId(e.target.value)}
              required
            >
              <option value="">Select a store</option>
              {stores.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name}
                </option>
              ))}
            </select>
          </label>

          <ImageUploadField label="Flyer photo" value={imageId} onChange={setImageId} />

          {imageId && (
            // eslint-disable-next-line @next/next/no-img-element
            <img src={imageUrl(imageId)} alt="" className={styles.preview} />
          )}
        </div>

        <div className={styles.right}>
          <span className={fieldStyles.sectionHeading}>Items</span>
          <div className={styles.itemList}>
            {items.map((row, index) => (
              <div key={index} className={styles.itemCard}>
                <div className={styles.itemCardTop}>
                  <select
                    className={fieldStyles.field}
                    value={row.productId}
                    onChange={(e) => updateItem(index, { productId: e.target.value })}
                  >
                    <option value="" disabled>
                      Select a product
                    </option>
                    <option value={NEW_PRODUCT_VALUE}>+ New product</option>
                    {products.map((p) => (
                      <option key={p.value} value={p.value}>
                        {p.displayText}
                      </option>
                    ))}
                  </select>
                  {items.length > 1 && (
                    <button type="button" className={fieldStyles.itemRemove} onClick={() => removeItem(index)}>
                      Remove
                    </button>
                  )}
                </div>

                {row.productId === NEW_PRODUCT_VALUE && (
                  <div className={styles.itemRow}>
                    <input
                      className={fieldStyles.field}
                      placeholder="New product name"
                      value={row.name}
                      onChange={(e) => updateItem(index, { name: e.target.value })}
                    />
                    <select
                      className={fieldStyles.field}
                      value={row.categoryId}
                      onChange={(e) => updateItem(index, { categoryId: e.target.value })}
                    >
                      <option value="">No category</option>
                      {categories.map((c) => (
                        <option key={c.value} value={c.value}>
                          {c.displayText}
                        </option>
                      ))}
                    </select>
                  </div>
                )}

                <input
                  className={fieldStyles.field}
                  type="number"
                  min="0.01"
                  step="0.01"
                  placeholder="Price"
                  value={row.price}
                  onChange={(e) => updateItem(index, { price: e.target.value })}
                />
              </div>
            ))}
          </div>
          <button type="button" className={fieldStyles.addItemButton} onClick={addItem}>
            + Add item
          </button>

          {error && <ErrorBanner message={error} />}

          <button className={fieldStyles.submit} type="submit" disabled={submitting}>
            {submitting ? <LoadingSpinner /> : "Upload flyer"}
          </button>
        </div>
      </form>
    </div>
  );
}
