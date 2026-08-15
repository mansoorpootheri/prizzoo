"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
} from "react";
import { authenticate } from "../api/auth";
import { clearToken, getToken, setToken } from "./token-storage";

interface AuthContextValue {
  isAuthenticated: boolean;
  isReady: boolean;
  login: (userNameOrEmailAddress: string, password: string) => Promise<void>;
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

  const logout = useCallback(() => {
    clearToken();
    setIsAuthenticated(false);
  }, []);

  return (
    <AuthContext.Provider value={{ isAuthenticated, isReady, login, logout }}>
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
