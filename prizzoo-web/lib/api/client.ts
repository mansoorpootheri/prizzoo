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
  options: { method?: "GET" | "POST"; body?: unknown } = {}
): Promise<T> {
  const { method = "GET", body } = options;
  const token = getToken();

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  let payload: AbpAjaxResponse<T>;
  try {
    payload = await response.json();
  } catch {
    throw new ApiError(`Request failed with status ${response.status}`);
  }

  if (!response.ok || !payload.success) {
    throw new ApiError(
      payload.error?.message ?? `Request failed with status ${response.status}`,
      payload.error?.details
    );
  }

  return payload.result;
}
