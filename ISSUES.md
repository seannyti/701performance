# Issues Log

Ongoing record of bugs, code quality issues, and the fixes applied.

---

## Fixed

### ✅ [CLEANUP] Indentation inconsistency in `refreshAccessToken`
**File:** `admin/src/stores/auth.ts`
**Issue:** The first line of `refreshAccessToken` was indented 6 spaces instead of 4, inconsistent with every other function in the file.
**Fix:** Corrected to 4-space indent.

---

### ✅ [CLEANUP] Trivial `handleOverlayClick` wrapper in AppointmentModal
**File:** `admin/src/components/AppointmentModal.vue`
**Issue:** A function `handleOverlayClick` existed solely to call `closeModal()` with no added logic, adding indirection for no reason.
**Fix:** Deleted the wrapper function. Updated `@click="handleOverlayClick"` in the template to `@click="closeModal"` directly.

---

### ✅ [CLEANUP] Duplicated `previousMonth`/`nextMonth` logic in Calendar
**File:** `admin/src/views/Calendar.vue`
**Issue:** `previousMonth()` and `nextMonth()` had identical ~20-line bodies differing only by `+1`/`-1` in the month offset.
**Fix:** Extracted shared logic into `navigateMonth(delta: number)`. Both functions are kept as one-liner wrappers so the template is unchanged.

---

### ✅ [BUG] `subscribeNewsletter` silently dropped on signup
**Files:** `frontend/src/stores/auth.ts`, `frontend/src/views/SignUp.vue`
**Issue:** `subscribeNewsletter` was declared in the `SignupData` interface and passed by `SignUp.vue`, but the `axios.post` to `/auth/register` never included it — the field was silently dropped and never reached the backend.
**Fix:** Removed `subscribeNewsletter` from the `SignupData` type and from the `authStore.signup(...)` call-site. The checkbox remains in the UI's local `signupForm` state until the backend supports the feature.

---

### ✅ [BUG] `logout()` duplicates maintenance mode check already in the router guard
**Files:** `frontend/src/stores/auth.ts`, `frontend/src/components/Header.vue`
**Issue:** `logout()` made a network GET to `/api/v1/settings` after clearing auth in order to return `/maintenance` as a redirect path. This is wrong for three reasons:
1. It's a side effect callers don't expect from a logout function.
2. The router's `beforeEach` guard already checks maintenance mode on every navigation — this is redundant.
3. If the settings endpoint ever becomes protected, logout silently breaks.
**Fix:** Stripped the network call from `logout()`, changed its return type to `void`. Updated `Header.vue` to always push to `/` after logout — the router guard handles the maintenance redirect automatically.

---

## Backend Audit (2026-03-17)

### ✅ [BUG] #1 — Session timeout conflates access token and refresh token lifetime
**Completed:** 2026-03-17
**Files:** `backend/Services/AuthService.cs`
**Issue:** `GetAccessTokenExpiryMinutesAsync()` reads `session_timeout` from the DB (480 min) and uses it for **both** the JWT access token expiry and the refresh token expiry. Both tokens expire at the same time, making the refresh mechanism pointless. Worse, if an admin lowers the setting (e.g. 30 min), both tokens expire in 30 min with no grace period — the user cannot silently refresh and gets hard-kicked out. `RefreshTokenExpiryHours: 24` in appsettings is never read.
**Fix:** Split the two concerns. `GetAccessTokenExpiryMinutesAsync()` reads `JwtSettings:AccessTokenExpiryMinutes` from config (short-lived JWT). New `GetSessionTimeoutMinutesAsync()` reads `session_timeout` from DB and is used exclusively by `GenerateAndSaveRefreshTokenAsync`.

---

### ✅ [BUG] #2 — Duplicate log calls in 8 catch blocks
**Completed:** 2026-03-17
**File:** `backend/Program.cs`
**Issue:** Eight catch blocks call both an injected `logger.LogError(...)` and `app.Logger.LogError(...)` on consecutive lines, producing duplicate log entries for every error. Affected endpoints: create/update/delete category, create/list/download/restore/delete backup.
**Fix:** Remove the redundant `app.Logger.LogError` line from each affected catch block. Keep the injected `logger.LogError` (more specific category name).

---

### ✅ [CLEANUP] #3 — `RateLimitingMiddleware.cs` is dead code
**Completed:** 2026-03-17
**File:** `backend/Middleware/RateLimitingMiddleware.cs`
**Issue:** The file exists but is never registered in the middleware pipeline. The built-in ASP.NET Core rate limiter (`app.UseRateLimiter()`) replaced it. A comment in Program.cs confirms this. The file creates false confidence that the custom middleware is active.
**Fix:** Delete the file.

---

### ✅ [CLEANUP] #4 — Inconsistent logging: `app.Logger` vs injected `ILogger<Program>`
**Completed:** 2026-03-17
**File:** `backend/Program.cs`
**Issue:** Some endpoint lambdas inject `ILogger<Program> logger` and use it; others capture `app.Logger` from the outer scope. After fixing #2, the remaining inconsistency is in endpoints that never injected a logger at all (create/delete product, stock patch, user management, settings, appointments).
**Fix:** For endpoints that use injected `logger` in their success paths, ensure catch blocks also use it (fixed by #2). For endpoints using only `app.Logger`, this is acceptable but noted for future cleanup.

---

### ✅ [CLEANUP] #5 — `RefreshTokenExpiryHours` is a dead config key
**Completed:** 2026-03-17
**Files:** `backend/appsettings.json`, `backend/appsettings.Production.json`
**Issue:** `JwtSettings:RefreshTokenExpiryHours` is defined in both appsettings files but never read anywhere in the codebase. The refresh token lifetime is controlled entirely by `session_timeout` from the DB.
**Fix:** Remove the key from both files. Resolved as part of #1 (once session_timeout is scoped to refresh tokens, the config-based fallback is `AccessTokenExpiryMinutes`).

---

### ✅ [BUG] #6 — HTTPS redirect fires after CORS and static files
**Completed:** 2026-03-17
**File:** `backend/Program.cs`
**Issue:** `app.UseHttpsRedirection()` is placed after `app.UseCors()` and `app.UseStaticFiles()`. Static files and CORS preflight responses are served over HTTP before the redirect fires, meaning images/JS/CSS can leak over plain HTTP in production.
**Fix:** Move `UseHttpsRedirection()` block to before `UseCors()`.

---

### ✅ [CONFIG] #7 — Production server IP hardcoded in base `appsettings.json`
**Completed:** 2026-03-17
**Files:** `backend/appsettings.json`, `backend/appsettings.Production.json`
**Issue:** `23.239.26.52` and `:81` are in `CorsSettings:AllowedOrigins` in the base appsettings which is committed to source control. Also, `appsettings.Production.json` uses a `CorsOrigins` flat-string key that the code never reads — production CORS is not actually being overridden.
**Fix:** Remove production IPs from base appsettings. Add proper `CorsSettings:AllowedOrigins` array to `appsettings.Production.json`.

---

### ✅ [CLEANUP] #9 — `FileUploadSettings` config vs hardcoded constants (scan flag)
**Completed:** 2026-03-17 — assessed, no change needed
**File:** `backend/Services/FileService.cs`
**Issue (from scan):** Scan reported constants hardcoded in FileService. Requires verification.
**Finding:** FALSE ALARM. FileService already injects `IConfiguration` and reads `MaxImageWidth`, `ThumbnailSize`, `ImageQuality` etc. from `FileUploadSettings` at construction time (lines 42–48). No change needed.

---

### ✅ [BUG] #10 — Silent exception swallow in `ProductService.GetAllProductsAsync`
**Completed:** 2026-03-17
**File:** `backend/Services/ProductService.cs`
**Issue:** The outermost catch block silently falls back to in-memory products with no log call. Any exception — including auth failures, connection pool exhaustion, or programming errors — is swallowed invisibly in production.
**Fix:** Add `_logger.LogWarning(ex, "Database unavailable or error in GetAllProductsAsync, returning fallback products")`.

---

## Open

### ✅ [BUG] Newsletter checkbox value never sent to the API
**Files:** `frontend/src/stores/auth.ts`, `frontend/src/views/SignUp.vue`
**Issue:** An earlier cleanup incorrectly removed `subscribeNewsletter` from `SignupData` under the assumption the backend didn't support it. The backend (`RegisterRequest` and `User` models) already had the field fully implemented.
**Fix:** Restored `subscribeNewsletter?: boolean` to `SignupData`, added `subscribeNewsletter: signupData.subscribeNewsletter ?? false` to the `axios.post` body, and restored the field at the `SignUp.vue` call-site.

---

### ✅ [TECH-DEBT] Dual API client implementations — 401 handling misaligned
**Files:** `admin/src/utils/apiClient.ts`, `frontend/src/services/api.ts`
**Issue:** The two clients serve different roles (admin uses a general auth-aware `fetch` wrapper; frontend uses an axios-based product/category service with caching), so full consolidation isn't practical without a larger refactor. The concrete divergence was in 401 handling: the admin client retries the original request after a silent token refresh; the frontend's axios response interceptor only logged the error and rejected.
**Fix:** Added a 401 retry path to the frontend's axios response interceptor. On a 401, it calls `authStore.silentRefresh()` once (guarded by `_retried` to prevent infinite loops), then replays the original request with the new token. Also exposed `silentRefresh` from the auth store's return value so the interceptor can call it.
