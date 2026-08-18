import { API_BASE_URL } from "./config";
import { getToken } from "../auth/token-storage";

export class ApiError extends Error {
  details?: string;

  constructor(message: string, details?: string) {
    super(message);
    this.name = "ApiError";
    this.details = details;
  }
}

// ABP wraps every dynamic-API/controller response - success or failure - in
// this envelope. Callers only ever see the unwrapped `result`.
interface AbpAjaxResponse<T> {
  result: T;
  targetUrl: string | null;
  success: boolean;
  error: { message: string; details?: string } | null;
  unAuthorizedRequest: boolean;
}

export async function fetchJson<T>(
  path: string,
  options: {
    // ABP's dynamic-API convention derives HTTP method from the app-service
    // method name prefix: Get*->GET, Create*/Insert*->POST, Update*->PUT,
    // Delete*->DELETE. Confirmed empirically: Product/Update returns 405 for
    // POST, 200 for PUT.
    method?: "GET" | "POST" | "PUT" | "DELETE";
    body?: unknown;
    headers?: Record<string, string>;
    // Registration must never carry a leftover Authorization token from a
    // different logged-in session - ABP resolves tenancy from an
    // authenticated session ahead of any Abp-TenantId header, which silently
    // hijacks the request onto the wrong (or host) account.
    skipAuth?: boolean;
  } = {}
): Promise<T> {
  const { method = "GET", body, headers, skipAuth = false } = options;
  const token = skipAuth ? null : getToken();
  const isFormData = body instanceof FormData;

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers: {
      // FormData sets its own multipart Content-Type (with boundary) - an
      // explicit header here would override it and break the upload.
      ...(isFormData ? {} : { "Content-Type": "application/json" }),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...headers,
    },
    body: isFormData ? body : body !== undefined ? JSON.stringify(body) : undefined,
  });

  let payload: AbpAjaxResponse<T>;
  try {
    payload = await response.json();
  } catch {
    throw new ApiError(`Request failed with status ${response.status}`);
  }

  if (!response.ok || !payload.success) {
    // Plain MVC controllers (e.g. ImageController) return BadRequest(string)
    // rather than a thrown exception - ABP still wraps it in this envelope,
    // but the message lands under `result` (a string) instead of
    // `error.message`, since only dynamic-API app-service methods get the
    // exception-to-error-envelope conversion. Handle both shapes.
    const fallbackMessage = typeof payload.result === "string" ? payload.result : undefined;
    throw new ApiError(
      payload.error?.message ?? fallbackMessage ?? `Request failed with status ${response.status}`,
      payload.error?.details
    );
  }

  return payload.result;
}
