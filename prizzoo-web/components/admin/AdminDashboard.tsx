"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import styles from "./AdminDashboard.module.css";

interface DashboardLink {
  label: string;
  href: string;
}

interface DashboardSection {
  heading: string;
  links: DashboardLink[];
}

const SECTIONS: DashboardSection[] = [
  {
    heading: "Stores",
    links: [{ label: "Manage stores", href: "/admin/stores" }],
  },
  {
    heading: "Catalog",
    links: [
      { label: "Manage products", href: "/admin/products" },
      { label: "Category master", href: "/admin/categories" },
      { label: "Unit master", href: "/admin/units" },
    ],
  },
  {
    heading: "Pricing",
    links: [
      { label: "Store prices", href: "/admin/prices" },
      { label: "Upload a flyer for a store", href: "/admin/flyers/new" },
    ],
  },
  {
    heading: "Master data",
    links: [{ label: "Location master", href: "/admin/locations" }],
  },
  {
    heading: "Administration",
    links: [
      { label: "Manage admins", href: "/admin/admins" },
      { label: "Registered users", href: "/admin/users" },
    ],
  },
];

export function AdminDashboard() {
  const router = useRouter();
  const { isAuthenticated, isAdmin, isReady, logout } = useAuth();

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace("/phone-entry");
    } else if (isReady && isAuthenticated && !isAdmin) {
      router.replace("/home");
    }
  }, [isReady, isAuthenticated, isAdmin, router]);

  function handleLogout() {
    logout();
    router.replace("/phone-entry");
  }

  if (!isReady || !isAuthenticated || !isAdmin) {
    return null;
  }

  return (
    <div className={styles.root}>
      <div className={styles.topBar}>
        <h1 className={styles.heading}>Admin</h1>
        <div className={styles.topBarActions}>
          <button type="button" className={styles.backButton} onClick={() => router.push("/home")}>
            Back to shopping
          </button>
          <button type="button" className={styles.logoutButton} onClick={handleLogout}>
            Logout
          </button>
        </div>
      </div>

      {SECTIONS.map((section) => (
        <div key={section.heading} className={styles.section}>
          <h2 className={styles.sectionHeading}>{section.heading}</h2>
          <div className={styles.linkList}>
            {section.links.map((link) => (
              <a key={link.href} className={styles.linkCard} href={link.href}>
                {link.label}
              </a>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}
