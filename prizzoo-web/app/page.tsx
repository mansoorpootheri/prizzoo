"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { SplashScreen } from "@/components/splash/SplashScreen";

const SPLASH_DURATION_MS = 1500;

export default function Page() {
  const router = useRouter();

  useEffect(() => {
    const timer = setTimeout(() => {
      router.replace("/phone-entry");
    }, SPLASH_DURATION_MS);
    return () => clearTimeout(timer);
  }, [router]);

  return <SplashScreen />;
}
