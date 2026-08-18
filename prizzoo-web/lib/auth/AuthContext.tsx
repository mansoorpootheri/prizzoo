"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
} from "react";
import { authenticate } from "../api/auth";
import { verifyOtp } from "../api/otpAuth";
import { clearToken, getToken, setToken } from "./token-storage";

interface AuthContextValue {
  isAuthenticated: boolean;
  isReady: boolean;
  // Password login - host Admin and ShopOwner only, reached via the
  // "Login to admin panel" link.
  login: (userNameOrEmailAddress: string, password: string) => Promise<void>;
  // Phone+OTP login - the only way a shopper authenticates. Both paths
  // write to the same single token slot: logging into one role's session
  // replaces the other in the same browser (no parallel dual-session
  // storage for this pass).
  loginWithOtp: (phoneNumber: string, code: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    // localStorage isn't available during SSR, so the real auth state can
    // only be read after mount - this deliberately renders "unauthenticated"
    // on both the server and the first client pass, then reveals afterward.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setIsAuthenticated(getToken() !== null);
    setIsReady(true);
  }, []);

  const login = useCallback(
    async (userNameOrEmailAddress: string, password: string) => {
      const result = await authenticate(userNameOrEmailAddress, password);
      setToken(result.accessToken, result.expireInSeconds);
      setIsAuthenticated(true);
    },
    []
  );

  const loginWithOtp = useCallback(async (phoneNumber: string, code: string) => {
    const result = await verifyOtp(phoneNumber, code);
    setToken(result.accessToken, result.expireInSeconds);
    setIsAuthenticated(true);
  }, []);

  const logout = useCallback(() => {
    clearToken();
    setIsAuthenticated(false);
  }, []);

  return (
    <AuthContext.Provider value={{ isAuthenticated, isReady, login, loginWithOtp, logout }}>
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
