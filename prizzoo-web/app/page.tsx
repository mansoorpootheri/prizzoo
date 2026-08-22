"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { SplashScreen } from "@/components/splash/SplashScreen";

const SPLASH_DURATION_MS = 1500;

export default function Page() {
  const router = useRouter();
  const { isAuthenticated, isReady } = useAuth();

  useEffect(() => {
    if (!isReady) return;
    const timer = setTimeout(() => {
      // Admin and shopper share the same landing page now (see
      // AuthContext's isAdmin - admin reaches /admin/dashboard via the
      // account menu, not a separate redirect here), so an already
      // logged-in visit of either role goes straight to /home instead of
      // being bounced back to the login screen every time.
      router.replace(isAuthenticated ? "/home" : "/phone-entry");
    }, SPLASH_DURATION_MS);
    return () => clearTimeout(timer);
  }, [router, isReady, isAuthenticated]);

  return <SplashScreen />;
}
