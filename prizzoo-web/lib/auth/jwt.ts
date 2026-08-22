// Minimal JWT payload decode - just enough to read the role claim so the UI
// can show admin-only affordances (see AuthContext's isAdmin). Not used for
// any security decision (the backend enforces [AbpAuthorize] independently),
// only for UI navigation/visibility.
const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

export function decodeJwtRoles(token: string): string[] {
  try {
    const payloadBase64 = token.split(".")[1];
    const base64 = payloadBase64.replace(/-/g, "+").replace(/_/g, "/");
    const json = JSON.parse(
      decodeURIComponent(
        atob(base64)
          .split("")
          .map((c) => "%" + c.charCodeAt(0).toString(16).padStart(2, "0"))
          .join("")
      )
    );
    const raw = json[ROLE_CLAIM] ?? json.role ?? json.roles;
    if (!raw) return [];
    return Array.isArray(raw) ? raw : [raw];
  } catch {
    return [];
  }
}
