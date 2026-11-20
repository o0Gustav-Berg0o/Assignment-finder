import { HttpParams } from '@angular/common/http';

/**
 * Adds each field to params, but only if its value is set (omits undefined/empty-string/false/0),
 * matching the optional-filter-param pattern used by every "list with filters" API call.
 */
export function withOptionalParams(
  params: HttpParams,
  fields: Record<string, string | number | boolean | undefined>,
): HttpParams {
  for (const [key, value] of Object.entries(fields)) {
    if (value) {
      params = params.set(key, value);
    }
  }
  return params;
}
