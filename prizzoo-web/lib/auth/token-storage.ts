const TOKEN_KEY = "prizzoo.accessToken";
const EXPIRES_AT_KEY = "prizzoo.accessTokenExpiresAt";

export function getToken(): string | null {
  if (typeof window === "undefined") return null;

  const token = window.localStorage.getItem(TOKEN_KEY);
  const expiresAt = window.localStorage.getItem(EXPIRES_AT_KEY);
  if (!token || !expiresAt) return null;

  if (Date.now() >= Number(expiresAt)) {
    clearToken();
    return null;
  }

  return token;
}

export function setToken(accessToken: string, expireInSeconds: number): void {
  if (typeof window === "undefined") return;

  window.localStorage.setItem(TOKEN_KEY, accessToken);
  window.localStorage.setItem(
    EXPIRES_AT_KEY,
    String(Date.now() + expireInSeconds * 1000)
  );
}

export function clearToken(): void {
  if (typeof window === "undefined") return;

  window.localStorage.removeItem(TOKEN_KEY);
  window.localStorage.removeItem(EXPIRES_AT_KEY);
}
