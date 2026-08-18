import { Suspense } from "react";
import { OtpVerifyForm } from "@/components/auth/OtpVerifyForm";

export default function Page() {
  return (
    <Suspense fallback={null}>
      <OtpVerifyForm />
    </Suspense>
  );
}
