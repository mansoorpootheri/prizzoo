"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
} from "react";
import { verifyOtp } from "../api/otpAuth";
import { clearToken, getToken, setToken } from "./token-storage";
import { decodeJwtRoles } from "./jwt";
import { clearShopperLocation } from "../location/shopperLocation";

interface AuthContextValue {
  isAuthenticated: boolean;
  // Derived from the JWT's role claim - lets the UI show admin-only
  // affordances (the "Admin" account-menu item, the /admin/* guards).
  // Never used for an actual authorization decision - the backend enforces
  // [AbpAuthorize] independently regardless of what the UI shows/hides.
  isAdmin: boolean;
  isReady: boolean;
  // Phone+OTP - the only login path for everyone, admin included. Whether
  // a phone number logs in as Admin or Shopper is decided server-side
  // (OtpAuthController) by whether that phone number already has the
  // Admin role, not by anything the client sends.
  login: (phoneNumber: string, code: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function computeIsAdmin(): boolean {
  const token = getToken();
  return token !== null && decodeJwtRoles(token).includes("Admin");
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);
  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    // localStorage isn't available during SSR, so the real auth state can
    // only be read after mount - this deliberately renders "unauthenticated"
    // on both the server and the first client pass, then reveals afterward.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setIsAuthenticated(getToken() !== null);
    setIsAdmin(computeIsAdmin());
    setIsReady(true);
  }, []);

  const login = useCallback(async (phoneNumber: string, code: string) => {
    const result = await verifyOtp(phoneNumber, code);
    setToken(result.accessToken, result.expireInSeconds);
    setIsAuthenticated(true);
    setIsAdmin(computeIsAdmin());
  }, []);

  const logout = useCallback(() => {
    clearToken();
    // A picked location is shopper-specific - don't let it silently carry
    // over to a different shopper who logs in next on this browser/device.
    clearShopperLocation();
    setIsAuthenticated(false);
    setIsAdmin(false);
  }, []);

  return (
    <AuthContext.Provider value={{ isAuthenticated, isAdmin, isReady, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
