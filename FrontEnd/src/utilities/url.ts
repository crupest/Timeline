export function withQuery<T>(url: string, query: T): string {
  if (query == null) return url;

  const params = new URLSearchParams();

  for (const [key, value] of Object.entries(query)) {
    if (value == null) void 0;
    else if (typeof value === "string") params.set(key, value);
    else if (typeof value === "number") params.set(key, String(value));
    else if (typeof value === "boolean") params.set(key, String(value));
    else if (value instanceof Date) params.set(key, value.toISOString());
    else {
      console.error("Unknown query parameter type. Param: ", value);
    }
  }

  const p = params.toString();

  if (p == "") {
    return url;
  }

  return url + "?" + p;
}
