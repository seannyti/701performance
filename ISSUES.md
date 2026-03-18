# Issues Log

Ongoing record of bugs, code quality issues, and the fixes applied.

---

## Fixed

### ✅ [BUG] `loading` undefined in Settings.vue — error state never shown
**File:** `admin/src/views/Settings.vue`
**Completed:** 2026-03-17
**Issue:** Template used `v-if="loading"` on line 25 but the script only destructures `isLoading` from `useLoadingState()`. `loading` is always `undefined`, so the entire `v-if` / `v-else-if="error"` / `v-else` chain was broken — the loading state never showed, and critically the error state could never render either. If settings failed to load, the user saw a blank form with no feedback.
**Fix:** Changed `v-if="loading"` to `v-if="isLoading && settings.length === 0"` to match the condition already used by the overlay spinner above it.

---

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

### ✅ [BUG] Newsletter checkbox value never sent to the API
**Files:** `frontend/src/stores/auth.ts`, `frontend/src/views/SignUp.vue`
**Issue:** An earlier cleanup incorrectly removed `subscribeNewsletter` from `SignupData` under the assumption the backend didn't support it. The backend (`RegisterRequest` and `User` models) already had the field fully implemented.
**Fix:** Restored `subscribeNewsletter?: boolean` to `SignupData`, added `subscribeNewsletter: signupData.subscribeNewsletter ?? false` to the `axios.post` body, and restored the field at the `SignUp.vue` call-site.

---

### ✅ [TECH-DEBT] Dual API client implementations — 401 handling misaligned
**Files:** `admin/src/utils/apiClient.ts`, `frontend/src/services/api.ts`
**Issue:** The two clients serve different roles (admin uses a general auth-aware `fetch` wrapper; frontend uses an axios-based product/category service with caching), so full consolidation isn't practical without a larger refactor. The concrete divergence was in 401 handling: the admin client retries the original request after a silent token refresh; the frontend's axios response interceptor only logged the error and rejected.
**Fix:** Added a 401 retry path to the frontend's axios response interceptor. On a 401, it calls `authStore.silentRefresh()` once (guarded by `_retried` to prevent infinite loops), then replays the original request with the new token. Also exposed `silentRefresh` from the auth store's return value so the interceptor can call it.

---

### ✅ [BUG] TypeScript errors in `Settings.vue` — `apiPost` untyped return values
**Completed:** 2026-03-17
**File:** `admin/src/views/Settings.vue`
**Issue:** Two TypeScript errors caused by `apiPost` calls with no type parameter (defaults to `unknown`):
1. **Line 3884** — `data.message` in `sendTestEmail`: accessing `.message` on `unknown` is a type error.
2. **Line 3905** — `logDebug('Settings reset successful', result)` in `resetSettings`: `logDebug`'s second parameter is typed as `LogContext`, but `result` was `unknown`.
**Fix:**
1. Typed the `sendTestEmail` call as `apiPost<{ message?: string }>` so `.message` is valid.
2. Dropped the `result` capture in `resetSettings` entirely — the return value was only passed to a debug log and has no functional use. Updated `logDebug` to a message-only call.

---

### ✅ [SECURITY] Chat — unauthenticated access to session detail endpoint
**Completed:** 2026-03-17
**File:** `backend/PowersportsApi/Program.cs`
**Issue:** `GET /api/v1/chat/sessions/{id}` had no authentication. Any unauthenticated caller who knew or guessed a sequential session ID could retrieve the full conversation history including `guestName`, `guestEmail`, and all messages — a direct PII leak.
**Fix:** Added `.RequireAuthorization("AdminOnly")` to the endpoint, consistent with the list endpoint above it.

---

### ✅ [SECURITY] Chat — `JoinSession` accepted any sessionId with no ownership proof
**Completed:** 2026-03-17
**File:** `backend/PowersportsApi/Hubs/ChatHub.cs`
**Issue:** `JoinSession(int sessionId)` admitted any anonymous WebSocket connection to any session group. Session IDs are sequential integers, making enumeration trivial. An attacker could silently subscribe to every active customer conversation.
**Fix:** Changed signature to `JoinSession(int sessionId, string token)`. The hub looks up the session, compares the supplied token against the `SessionToken` column (a UUID generated at session creation), and rejects the connection if they don't match. A new `ConcurrentDictionary<string, int>` tracks `connectionId → sessionId` for admitted customers.

---

### ✅ [SECURITY] Chat — `SendMessage` allowed any connection to inject messages into any session
**Completed:** 2026-03-17
**File:** `backend/PowersportsApi/Hubs/ChatHub.cs`
**Issue:** Non-agent connections could call `SendMessage(42, "text")` and post to any open session regardless of whether they created it, enabling message injection and customer impersonation.
**Fix:** Non-agent callers are verified against the `_customerSessions` dictionary before a message is accepted. Connections not mapped to the target session receive an `Error` event and the message is discarded.

---

### ✅ [SECURITY] Chat — `StartChatRequest` had no server-side input validation
**Completed:** 2026-03-17
**Files:** `backend/PowersportsApi/Program.cs`
**Issue:** `StartChatRequest(string? Name, string? Email)` had no length or format constraints at the API level. The frontend `maxlength` attributes are trivially bypassed, allowing arbitrarily large strings to reach the database.
**Fix:** Added `[Required]`, `[MaxLength(80)]` on `Name` and `[MaxLength(200)]`, `[EmailAddress]` on `Email`. The endpoint now runs `MiniValidator.TryValidate` before processing and returns a 400 ValidationProblem on failure.

---

### ✅ [SECURITY] Chat — no server-side message body length cap
**Completed:** 2026-03-17
**File:** `backend/PowersportsApi/Hubs/ChatHub.cs`
**Issue:** `SendMessage` only checked for whitespace. A direct WebSocket call bypassing the frontend `maxlength="2000"` could submit arbitrarily large messages, filling the database and potentially causing memory pressure during broadcast.
**Fix:** Added an explicit `body.Length > 2000` check at the top of `SendMessage`. Oversized messages are rejected with an `Error` event; nothing is written to the database.

---

### ✅ [BUG] CORS error on login is actually a rate-limit 429 — per-IP limit was missing
**Completed:** 2026-03-17
**File:** `backend/PowersportsApi/Program.cs`
**Issue:** Two compounding bugs caused an unusable login screen after every rebuild:
1. **Wrong pipeline order** — `UseRateLimiter()` ran before `UseCors()`. When the rate limiter rejected a request with 429, the response had no CORS headers, so the browser reported it as a CORS error (`No 'Access-Control-Allow-Origin'`) instead of a 429, masking the real cause entirely.
2. **Global not per-IP limit** — `AddFixedWindowLimiter("auth", 5/min)` was a single shared bucket across ALL clients, not partitioned by IP. During local testing (container rebuilds, multiple quick reloads, the auth heartbeat firing), 5 total requests per minute for the entire server was trivially exceeded, locking out all users.
**Fix:**
1. Moved `app.UseCors("AllowFrontend")` before `app.UseRateLimiter()` so rate-limit rejections include CORS headers and the browser reports them as actual 429s.
2. Replaced both `AddFixedWindowLimiter` calls with `AddPolicy` using `RateLimitPartition.GetFixedWindowLimiter` partitioned by `RemoteIpAddress`. Limits: auth = 20/IP/min, api = 120/IP/min.

---

### ✅ [BUG] Site Settings — `logo_header_height` / `logo_footer_height` not seeded; created with `IsPublic = false`
**Completed:** 2026-03-17
**Files:** `backend/PowersportsApi/Program.cs`, `frontend/src/components/Header.vue`, `frontend/src/components/Footer.vue`, `admin/src/views/Settings.vue`
**Issue:** `Header.vue` and `Footer.vue` both read `logo_header_height` and `logo_footer_height` from the public `/api/v1/settings` API. These keys were missing from the backend seed defaults entirely. When an admin saved General Settings for the first time, `saveSection` would POST them via `POST /api/v1/admin/settings` — but that endpoint creates settings with `IsPublic = false` by default. The public API never returned them, so logo heights always fell back to hard-coded defaults regardless of what was set in the admin.
**Fix:** Added both keys to the backend seed defaults under the General category (SortOrder 4 and 5). Fixed a SortOrder collision on the contact settings that followed (bumped 4/5/6 → 6/7/8). The seed loop already sets `IsPublic = true` for all non-Email/Advanced/Security/System categories, so both new rows are seeded public automatically.

---

### ✅ [BUG] Site Settings — `saving` computed missing 3 action IDs; Advanced tab Save buttons never disabled
**Completed:** 2026-03-17
**File:** `admin/src/views/Settings.vue`
**Issue:** The `saving` computed ref — used to show "Saving..." and disable Save buttons during in-flight requests — listed action IDs for every section except `save-security`, `save-performance`, and `save-system`. Save buttons in the Advanced tab (Security & Access, Performance & Caching, System Settings) never disabled while the request was in flight, allowing double-submits.
**Fix:** Added the three missing IDs to the `saving` computed.

---

### ✅ [FALSE ALARM] `session_timeout` and `max_login_attempts` stored but not enforced
**Completed:** 2026-03-17
**Finding:** Both are fully enforced. `session_timeout` controls refresh token lifetime via `GetSessionTimeoutMinutesAsync()` (covered by Backend Audit #1). `max_login_attempts` is read on every failed login in `AuthService.cs` — increments `FailedLoginAttempts`, sets `LockoutEnd = +15 min` on threshold, and the lockout is checked at the top of every login attempt.

---

### ✅ [CLEANUP] Performance & Caching — dead settings removed, functional ones wired
**Completed:** 2026-03-17
**Files:** `backend/PowersportsApi/Program.cs`, `backend/PowersportsApi/Services/FileService.cs`, `admin/src/views/Settings.vue`
**Issue:** Seven settings in the Performance & Caching section were stored in DB but never enforced.
**Fix:**
- **Removed** `enable_image_optimization`, `max_image_height`, `enable_cdn`, `cache_duration` from both seed arrays, admin UI, and local DB. `enable_image_optimization` is always-on in FileService; `max_image_height` was never used (only width is constrained); `enable_cdn` and `cache_duration` were incomplete concepts with no backing implementation.
- **Wired** `image_quality` and `max_image_width`: `FileService.UploadToMediaLibraryAsync` reads current DB values via `GetDbSettingIntAsync()` before each upload, falling back to `appsettings.json` defaults. `GetImageEncoder` updated to accept a quality parameter.
- **Wired** `enable_compression`: `AddResponseCompression(EnableForHttps = true)` registered in DI. At startup (after migrations), the setting is read from DB and `UseResponseCompression()` is conditionally added to the pipeline before security headers. Defaults to `true` on first run.

---

### ✅ [BUG] Featured products don't filter by `isFeatured`
**Completed:** 2026-03-17
**File:** `backend/PowersportsApi/Services/ProductService.cs`
**Issue:** `GetFeaturedProductsAsync` called `.Take(3)` with no `.Where` clause, returning the first 3 products in DB order regardless of their `IsFeatured` or `IsActive` flags. Inactive products could appear on the homepage.
**Fix:** Added `.Where(p => p.IsFeatured && p.IsActive)` before `.Take(3)`.

---

### ✅ [BUG] `includeInactive=true` param silently ignored on GET /products
**Completed:** 2026-03-17
**File:** `backend/PowersportsApi/Program.cs`
**Issue:** `GET /api/v1/products` only accepted `page` and `pageSize`. There was no way to request inactive products, and all products (active and inactive) were returned to every caller including unauthenticated frontend visitors.
**Fix:** Added `bool includeInactive = false` parameter. When false, filters to `.Where(p => p.IsActive)` before paginating. When true, all products are returned (for admin use).

---

### ✅ [BUG] Category status toggle spreads full object
**Completed:** 2026-03-17
**File:** `admin/src/views/Catalog.vue`
**Issue:** `toggleCategoryStatus` sent `{ ...category, isActive: !category.isActive }` — spreading the full category object including `id`, `createdAt`, `updatedAt`, and other fields not expected by `UpdateCategoryRequest`, which could cause deserialization errors or unintended field overwrites.
**Fix:** Replaced the spread with an explicit object containing only the four fields `UpdateCategoryRequest` expects: `name`, `description`, `imageUrl`, `isActive`. Also added `imageUrl?` to the `Category` TypeScript interface.

---

### ✅ [DUPLICATE] Image URL population repeated 3× in ProductService
**Completed:** 2026-03-17
**File:** `backend/PowersportsApi/Services/ProductService.cs`
**Issue:** The 3-line block that finds the main `ProductImage` and sets `product.ImageUrl` was duplicated verbatim in `GetAllProductsAsync`, `GetProductByIdAsync`, and `GetFeaturedProductsAsync`.
**Fix:** Extracted into `private static void ApplyMainImage(Product product)`. All three call sites replaced with a single method call.

---

### ✅ [DUPLICATE] Stock status and margin helpers repeat logic 3× each
**Completed:** 2026-03-17
**File:** `admin/src/views/Catalog.vue`
**Issue:** `getStockStatus`, `getStockStatusLabel`, and `getInventoryRowClass` each independently checked `stockQuantity === 0` and `<= lowStockThreshold`. Similarly, `getMarginNum`, `getMarginLabel`, and `getMarginClass` each recomputed the margin percentage independently.
**Fix:** Consolidated stock helpers into `getStockInfo(product)` returning `{ status, label, rowClass }`. Consolidated margin helpers into `getMarginInfo(price, cost)` returning `{ value, label, cssClass }`. All six original functions removed. All template and script references updated to use the new consolidated functions.

---

### ✅ [DEAD CODE] Products.vue stub
**Completed:** 2026-03-17
**Files:** `admin/src/views/Products.vue`, `admin/src/router/index.ts`
**Issue:** `Products.vue` was a placeholder stub ("Product creation/editing coming soon") with no real functionality. The router already redirected `/products` to `/catalog` without importing the component.
**Fix:** Deleted `Products.vue`. No router change was needed — the redirect route used no component import.

---

### ✅ [UX] No inline active/inactive toggle on products in the grid
**Completed:** 2026-03-17
**File:** `admin/src/views/Catalog.vue`
**Issue:** The products table had no way to quickly enable or disable a product without opening the full edit modal, unlike categories which had inline Enable/Disable buttons.
**Fix:** Added an Enable/Disable toggle button to each product row. Added `toggleProductStatus(product)` function that calls `apiPut` with all required `UpdateProductRequest` fields and `isActive: !product.isActive`, then updates the local product object directly without a full reload.

---

### ✅ [CLEANUP] Raw fetch() in loadData()
**Completed:** 2026-03-17
**File:** `admin/src/views/Catalog.vue`
**Issue:** `loadData` used raw `fetch()` calls with manual `Authorization` header construction instead of the `apiGet` wrapper already used elsewhere in the file. This bypassed the centralized auth/error handling in `apiClient`.
**Fix:** Replaced both `fetch()` calls with `apiGet('/categories?includeInactive=true')` and `apiGet('/products?includeInactive=true')`. Removed the now-unused `API_URL` import.

---

### ✅ [UX] Disabled category delete button gives no explanation
**Completed:** 2026-03-17
**File:** `admin/src/views/Catalog.vue`
**Issue:** The Delete button on categories was silently disabled when the category had products, with no tooltip or visual explanation. Admins had no way to know why the button was unresponsive or what to do about it.
**Fix:** Added a `title` attribute with the message "Cannot delete a category that has products. Move or delete the products first." when the count is > 0. Changed the button label to show `Has Products (N)` (with the product count) when disabled, so admins can see at a glance why deletion is blocked.

---

### ✅ [FEATURE] Product Active/Featured controls moved from modal to inline row buttons
**Completed:** 2026-03-17
**File:** `admin/src/views/Catalog.vue`
**Change:** Removed the "Product Options" section (Active and Featured checkboxes) from the product edit modal. Added two inline action buttons directly to each product row: Enable/Disable (already existed) and a new ☆ Feature / ⭐ Unfeature toggle. Added `toggleProductFeatured()` function mirroring the pattern of `toggleProductStatus()`. Both controls update the product immediately without a full reload.

---

## Inquiries Fixes (2026-03-17)

### ✅ [UX] #1 — Inquiries not auto-marked as Read when opened
**Completed:** 2026-03-17
**File:** `admin/src/views/Inquiries.vue`
**Issue:** Opening a "New" inquiry in the detail modal had no side effect — the submission remained in "New" status until the admin manually changed it and saved. This inflated the New count and made the status misleading.
**Fix:** Changed `viewSubmission` from a sync function to async. After opening the modal, if the submission status is "New", it immediately calls `PUT /admin/contact-submissions/{id}` to set status to "Read", then updates the local submission object and refreshes stats. No full reload required.

---

### ✅ [BUG] #2 — `ContactForm` type missing `subject` field
**Completed:** 2026-03-17
**Files:** `packages/shared-types/index.ts`, `frontend/src/views/Contact.vue`
**Issue:** The shared `ContactForm` interface only had `name`, `email`, and `message`. `Contact.vue` worked around this with `reactive<ContactForm & { subject: string }>` — an inline intersection type that duplicated the field definition outside the canonical type.
**Fix:** Added `subject?: string` to the `ContactForm` interface in `shared-types/index.ts`. Removed the `& { subject: string }` workaround in `Contact.vue`, which now types the form as simply `reactive<ContactForm>`.

---

### ✅ [VALIDATION] #3 — No minimum length validation on Subject field
**Completed:** 2026-03-17
**Files:** `backend/PowersportsApi/Models/Auth/AuthModels.cs`, `frontend/src/views/Contact.vue`
**Issue:** `ContactRequest.Subject` had `[StringLength(200)]` but no minimum length. A single-character subject could be submitted without triggering a validation error at either the frontend or backend level.
**Fix:** Changed the backend annotation to `[StringLength(200, MinimumLength = 2)]`. Added `validateSubject()` in `Contact.vue` that only validates when the subject is non-empty (since it's optional) and returns an error if it's less than 2 characters. Added `errors.subject` to the reactive errors object and wired it into `validateForm()` and `resetForm()`.

---

### ✅ [PERFORMANCE] #4 — Stats avg query loaded full entities then computed average in memory
**Completed:** 2026-03-17
**File:** `backend/PowersportsApi/Program.cs`
**Issue:** The avg response time calculation used `.OrderBy(c => c.CreatedAt)` (unnecessary for an average) and pulled rows to memory before computing. The `.OrderBy` added a sort step on the DB with no benefit.
**Fix:** Removed the redundant `.OrderBy` from the read submissions query. The projection to `new { c.CreatedAt, c.ReadAt }` was already in place and correct — only the 2 DateTime columns are transferred. Added a comment explaining why in-process averaging is used (EF Core cannot translate `TimeSpan.TotalMinutes` to MySQL SQL).

---

### ✅ [VALIDATION] #5 — Admin notes field had no length cap
**Completed:** 2026-03-17
**Files:** `admin/src/views/Inquiries.vue`, `backend/PowersportsApi/Models/ContactSubmission.cs`, `backend/PowersportsApi/Models/Admin/AdminModels.cs`
**Issue:** The Admin Notes textarea in the inquiry modal had no `maxlength` attribute, and the `ContactSubmission` model had no `[StringLength]` constraint on `AdminNotes`. An admin could submit arbitrarily large notes. The existing snapshot showed the column as `varchar(1000)` but neither the model nor the DTO enforced this.
**Fix:** Added `maxlength="2000"` to the textarea in `Inquiries.vue`. Added `[StringLength(2000)]` to `AdminNotes` on `ContactSubmission`. Added `[StringLength(2000)]` to `AdminNotes` on `UpdateContactSubmissionRequest`.

---

### ✅ [SECURITY] #6 — Contact endpoint not explicitly rate limited
**Completed:** 2026-03-17
**File:** `backend/PowersportsApi/Program.cs`
**Issue:** The `POST /api/v1/contact` endpoint was registered on the `v1Routes` group which carries `RequireRateLimiting("api")` as a group-level policy, but the endpoint had no explicit rate limiting declaration of its own. Relying solely on the implicit group inheritance makes the intent unclear and could be accidentally lost if the endpoint is moved.
**Fix:** Added `.RequireRateLimiting("api")` explicitly to the endpoint's fluent chain.

---

### ✅ [UX] #7 — No search in admin inquiries panel
**Completed:** 2026-03-17
**File:** `admin/src/views/Inquiries.vue`
**Issue:** The inquiries panel only supported filtering by status and date range. There was no way to search for a specific submitter by name, email, or message content.
**Fix:** Added a `searchQuery` ref and a `filteredSubmissions` computed property that filters `submissions` by name, email, message, or subject (case-insensitive). Added a search text input at the top of the filters bar. The table and empty-state now bind to `filteredSubmissions` instead of `submissions` directly. `clearFilters()` also resets the search query.

---

### ✅ [UX] #8 — No confirmation email sent to contact form submitter
**Completed:** 2026-03-17
**File:** `backend/PowersportsApi/Services/EmailService.cs`
**Issue:** `SendContactFormEmailAsync` only sent a notification to the admin's contact email. The person who submitted the form received no acknowledgement that their message was received.
**Fix:** After successfully sending the admin notification, the method sends a second email to the submitter's address. Subject: "We received your message — [SiteName]". Body includes a greeting, confirmation that the message was received, and a truncated preview (max 300 chars) of the submitted message. If the confirmation email fails, a warning is logged but the original success response is returned unchanged — a confirmation failure does not degrade the submitter's experience.

---

### ✅ [FEATURE] #9 — `ContactSubmission` model missing `UpdatedAt` timestamp
**Completed:** 2026-03-17
**Files:** `backend/PowersportsApi/Models/ContactSubmission.cs`, `backend/PowersportsApi/Program.cs`, `backend/PowersportsApi/Migrations/20260317160000_AddUpdatedAtToContactSubmissions.cs`, `backend/PowersportsApi/Migrations/PowersportsDbContextModelSnapshot.cs`
**Issue:** The `ContactSubmission` entity had no `UpdatedAt` column, making it impossible to know when a submission was last modified (status change, note added, reassignment).
**Fix:** Added `public DateTime? UpdatedAt { get; set; }` to the model. The PUT endpoint now sets `submission.UpdatedAt = DateTime.UtcNow` on every update. Created migration `20260317160000_AddUpdatedAtToContactSubmissions` that adds the nullable `datetime(6)` column to `ContactSubmissions`. Updated `PowersportsDbContextModelSnapshot` to include the new property.

---

## Orders Fixes (2026-03-17)

### ✅ [BUG] #1 — Payment method options inconsistent between Create and Edit modals
**Completed:** 2026-03-17
**File:** `admin/src/views/Orders.vue`
**Issue:** The Create Order modal offered `CreditCard` and `DebitCard`. The Edit Order modal offered `CreditCardPhone` and `Financing` instead — neither `CreditCard` nor `DebitCard` was present. An order created with `CreditCard` would open in the edit modal with no matching option, silently changing the payment method on the next save.
**Fix:** Unified both dropdowns to the same 8-option list: `CreditCard`, `DebitCard`, `CreditCardPhone`, `Cash`, `Check`, `BankTransfer`, `Financing`, `Other`.

---

### ✅ [BUG] #2 — `clearFilters` doesn't reset page to 1
**Completed:** 2026-03-17
**File:** `admin/src/views/Orders.vue`
**Issue:** `clearFilters()` reset all filter values and called `loadOrders()`, but did not reset `page.value`. If the admin was on page 5 when clearing, `loadOrders` would fetch page 5 of the unfiltered results — potentially an empty or incomplete page.
**Fix:** Added `page.value = 1` before the `loadOrders()` call in `clearFilters`.

---

### ✅ [BUG] #3 — `deliveredDate` field missing from edit modal
**Completed:** 2026-03-17
**File:** `admin/src/views/Orders.vue`
**Issue:** `editForm.deliveredDate` was populated from the loaded order in `viewOrder()` and included in the PUT payload in `saveOrder()`, but there was no `<input>` for it in the edit modal template. Admins had no way to set or clear the delivered date from the UI.
**Fix:** Added a `datetime-local` input for Delivered Date in the Order Status section of the edit modal, alongside the existing Shipped Date field.

---

### ✅ [PERFORMANCE] #4 — Orders list endpoint eager-loads Items + Product joins it doesn't use
**Completed:** 2026-03-17
**File:** `backend/PowersportsApi/Program.cs`
**Issue:** `GET /admin/orders` applied `.Include(o => o.Items).ThenInclude(i => i.Product)` before the query. The final `Select` projection only used `ItemCount = o.Items.Count` — EF Core can translate `.Count` in a projection without needing the Includes. The unnecessary joins were executed on every list load.
**Fix:** Removed both `.Include` calls from the list query. The `GET /admin/orders/{id}` detail endpoint retains its Includes as it returns full item data.

---

### ✅ [BUG] #5 — Stats endpoint fetches `recentOrders` that the frontend never uses
**Completed:** 2026-03-17
**File:** `backend/PowersportsApi/Program.cs`
**Issue:** `GET /admin/orders/stats` queried 5 recent orders and returned them in the response. `loadStats()` in `Orders.vue` does `Object.assign(stats, data)` into a reactive object with only the 7 numeric stat fields — `recentOrders` was silently dropped. The query ran on every page load and filter change with no consumer.
**Fix:** Removed the `recentOrders` query and the field from the stats response.

---

### ✅ [UX] #6 — No success toast on order save or delete
**Completed:** 2026-03-17
**File:** `admin/src/views/Orders.vue`
**Issue:** `saveOrder()` closed the modal and reloaded the list on success with no user feedback. `confirmDeleteOrder()` similarly reloaded silently. Admins had no confirmation that the action completed.
**Fix:** Added `toast.success('Order saved successfully')` in `saveOrder` on successful response. Added `toast.success('Order deleted successfully')` in `confirmDeleteOrder` on successful response. Added `toast.error(...)` in both catch blocks and on non-ok responses.

---

## Calendar Fixes (2026-03-17)

### ✅ [BUG] #1 — Edit button disabled globally while any action is loading
**Completed:** 2026-03-17
**File:** `admin/src/views/Calendar.vue`
**Issue:** The Edit button on appointment cards used `:disabled="hasActiveAction()"`, which returns `true` if *any* keyed loading action is in progress system-wide. The Complete and Delete buttons on the same card correctly used `:disabled="isActionLoading(appointment.id)"` — scoped to that specific appointment. The result: completing or deleting appointment #1 would silently disable the Edit button on every other appointment on the page, preventing the admin from editing unrelated appointments.
**Fix:** Changed `:disabled="hasActiveAction()"` to `:disabled="isActionLoading(appointment.id)"` to match the Complete and Delete buttons. Removed the now-unused `hasActiveAction` from the `useLoadingState` destructure.

---

### ✅ [PERFORMANCE] #2 — `GET /appointments` list fetches `CreatedBy` join that is never displayed
**Completed:** 2026-03-17
**File:** `backend/PowersportsApi/Program.cs`
**Issue:** The appointment list endpoint applied `.Include(a => a.CreatedBy)` and projected `CreatedBy = a.CreatedBy != null ? new { a.CreatedBy.FirstName, a.CreatedBy.LastName } : null` into every response. Neither Calendar.vue nor AppointmentModal.vue displays the creator name anywhere in the UI. This JOIN executed on every calendar load and every month navigation with no consumer.
**Fix:** Removed `.Include(a => a.CreatedBy)` and the `CreatedBy` projection from the list endpoint. The `GET /appointments/{id}` detail endpoint retains it.

---

### ✅ [BUG] #3 — `loadAppointments` uses `console.error` instead of `logError`
**Completed:** 2026-03-17
**File:** `admin/src/views/Calendar.vue`
**Issue:** The catch block in `loadAppointments` called `console.error(...)` directly, bypassing the app's `logError` service from `@/services/logger`. Every other error path in the admin codebase uses `logError` for centralised error capture. This one would be silently dropped from any production logging.
**Fix:** Replaced `console.error('Failed to load appointments:', error)` with `logError('Failed to load appointments', error)`. Added the missing `import { logError } from '@/services/logger'`.

---

## Media Library Fixes (2026-03-17)

### ✅ [BUG] #1 — Uploaded files never appear after a successful upload
**Completed:** 2026-03-17
**File:** `admin/src/views/MediaLibrary.vue`
**Issue:** In `uploadFilesToLibrary`, the success path called `closeUploadModal()` before checking whether to reload section files. `closeUploadModal()` sets `currentUploadSectionId.value = null`. The immediately following `if (currentUploadSectionId.value)` guard therefore always evaluated to `false`, so `loadSectionFiles()` and `loadSections()` were never called. Uploaded files did not appear in the grid without a manual page refresh.
**Fix:** Captured the section ID in a local variable (`const uploadedSectionId = currentUploadSectionId.value!`) before calling `closeUploadModal()`. Replaced the null-guarded block with direct calls to `loadSectionFiles(uploadedSectionId)` and `loadSections()`.

---

### ✅ [BUG] #2 — Re-selecting the same file in the upload dialog does nothing
**Completed:** 2026-03-17
**File:** `admin/src/views/MediaLibrary.vue`
**Issue:** `handleFileSelect` read files from the input but never reset its value. If an admin removed a file from the pending list and then tried to re-add it by opening the file dialog and selecting it again, the `@change` event did not fire because the input's value was still set to that file path.
**Fix:** Added `target.value = ''` after reading the selected files, clearing the input so the same file can be selected again.

---

### ✅ [DEAD CODE] #3 — `isUploading` ref declared but never used
**Completed:** 2026-03-17
**File:** `admin/src/views/MediaLibrary.vue`
**Issue:** `const isUploading = ref(false)` was declared alongside the other state refs but was never read or written anywhere in the component. Upload loading state is tracked entirely through `isActionLoading('upload')` via `executeWithLoading`. The ref was a leftover from an earlier implementation.
**Fix:** Removed the declaration.

---

### ✅ [BUG] `loading` undefined in Backup.vue — ReferenceError on every page load
**Completed:** 2026-03-17
**File:** `admin/src/views/Backup.vue`
**Issue:** `onMounted` wrapped `loadBackupSettings()` + `refreshBackupList()` in a `try/finally` and called `loading.value = false` in the `finally` block. `loading` was never declared — only `isLoading` from `useLoadingState()` exists. This threw a ReferenceError on every Backup page load (caught silently by Vue but present in the console).
**Fix:** Removed the `try/finally` wrapper; called both functions sequentially. `executeWithLoading` inside each function already manages `isLoading` correctly — the `finally` block was redundant.

---

### ✅ [BUG] `loadingBackups` ref used in template but never declared — inline backup list spinner never shown
**Completed:** 2026-03-17
**File:** `admin/src/views/Backup.vue`
**Issue:** The "Available Backups" section had `v-if="loadingBackups"` on its inline loading state div, but `loadingBackups` was never declared in the script. The value is always `undefined` (falsy), so the loading spinner was never rendered — users saw no feedback while the backup list was loading.
**Fix:** Replaced `v-if="loadingBackups"` with `v-if="isActionLoading('loadBackups')"`, which is the keyed action used by `refreshBackupList`.

---

### ✅ [BUG] Logout redirects to hardcoded `localhost:3000` — breaks in production
**Completed:** 2026-03-17
**File:** `admin/src/components/AdminLayout.vue`
**Issue:** `handleLogout` always redirected to `http://localhost:3000`, regardless of environment. In any non-local deployment the logout button would land on the wrong host.
**Fix:** Changed to `import.meta.env.VITE_FRONTEND_URL || 'http://localhost:3000'`, matching the pattern already used elsewhere in the app.

---

## QoL Pass (2026-03-17)

All items from `QOL_NOTES.md` implemented in this pass.

### ✅ [QOL] Migrated all admin views to `apiGet`/`apiPost`/`apiPut`/`apiDelete` wrappers
**Completed:** 2026-03-17
**Files:** `admin/src/views/Categories.vue`, `Calendar.vue`, `Orders.vue`, `Backup.vue`, `MediaLibrary.vue`, `LiveChat.vue`, `Users.vue`, `Music.vue`, `Dashboard.vue`
**Issue:** Multiple views used raw `fetch()` or manually constructed `Authorization` headers instead of the centralised `apiClient` wrappers. Auth header logic was duplicated, error handling inconsistent, and `API_URL` was imported and referenced independently in each file.
**Fix:** Replaced all raw fetch calls with `apiGet`/`apiPost`/`apiPut`/`apiDelete`/`apiClient` from `@/utils/apiClient`. Removed all per-view `API_URL` imports and manual `Authorization` header construction.

---

### ✅ [QOL] Migrated manual loading refs to `useLoadingState` composable
**Completed:** 2026-03-17
**Files:** `admin/src/views/Music.vue`, `Orders.vue`
**Issue:** Views declared manual `loading`/`saving` refs and set them in try/finally blocks. The `useLoadingState` composable already handles this pattern with `executeWithLoading` and keyed action loading via `isActionLoading`.
**Fix:** Replaced all manual loading refs with `useLoadingState`. Async operations wrapped in `executeWithLoading`. Per-action spinners on Save/Delete buttons use `isActionLoading(key)`.

---

### ✅ [QOL] Added `useToast` success/error feedback to all mutating operations
**Completed:** 2026-03-17
**Files:** `admin/src/views/Categories.vue`, `Orders.vue`, `Users.vue`, `MediaLibrary.vue`, `Backup.vue`, `LiveChat.vue`
**Issue:** Many create/update/delete operations completed silently without user feedback. A few showed backend error messages as raw browser alerts.
**Fix:** Added `toast.success`, `toast.error`, `toast.saveSuccess`, `toast.deleteSuccess` calls to all mutating operations across the affected views.

---

### ✅ [QOL] Replaced `console.error` with `logError` across admin views
**Completed:** 2026-03-17
**Files:** `admin/src/views/Calendar.vue`, `LiveChat.vue`, `admin/src/components/AppointmentModal.vue`
**Issue:** Remaining `console.error` calls in catch blocks bypassed the centralised `logError` service.
**Fix:** Replaced all `console.error` calls with `logError` from `@/services/logger`. Added the import where missing.

---

### ✅ [QOL] O(n²) → O(n) appointment count per calendar cell
**Completed:** 2026-03-17
**File:** `admin/src/views/Calendar.vue`
**Issue:** `calendarDays` computed called `.filter()` on the full appointments array for every cell (42 cells × N appointments = O(42×N) per render).
**Fix:** Built a `Map<string, number>` of date→count once inside the computed, then each cell does a single O(1) map lookup.

---

### ✅ [QOL] O(n²) → O(n) product count per category
**Completed:** 2026-03-17
**File:** `admin/src/views/Categories.vue`
**Issue:** `getProductCount(id)` filtered `products.value` on every call. With N categories and M products this is O(N×M) per render.
**Fix:** Replaced with a `productCountMap` computed that builds a `Map<number, number>` once (O(M)), and `getProductCount` does a single O(1) lookup.

---

### ✅ [QOL] Parallel health check requests in Dashboard
**Completed:** 2026-03-17
**File:** `admin/src/views/Dashboard.vue`
**Issue:** The health check ran endpoint requests sequentially in a for-loop, making total time the sum of all individual response times.
**Fix:** Replaced the sequential loop with `Promise.all(endpoints.map(...))` so all requests fire concurrently.

---

### ✅ [QOL] Removed `setTimeout(..., 0)` from `navigateMonth`
**Completed:** 2026-03-17
**File:** `admin/src/views/Calendar.vue`
**Issue:** `navigateMonth` used `setTimeout(..., 0)` to defer auto-selecting the first day after a month change. Vue's reactivity handles this synchronously; the delay was unnecessary and added a brief flash of no selection.
**Fix:** Removed the `setTimeout`; auto-select runs directly after updating the month ref.

---

### ✅ [QOL] `setInterval` in Users.vue skips hidden tab and open modal
**Completed:** 2026-03-17
**File:** `admin/src/views/Users.vue`
**Issue:** The polling interval called `loadUsers()` every N seconds regardless of whether the tab was visible or a modal was open, causing unnecessary network requests and potential stale-state overwrites mid-interaction.
**Fix:** Added `document.visibilityState === 'visible'` and no-modal-open guards to the interval callback.

---

### ✅ [QOL] Parallel profile+role update in Users.vue
**Completed:** 2026-03-17
**File:** `admin/src/views/Users.vue`
**Issue:** `submitEditUser` called PUT profile then PUT role sequentially, doubling latency when both needed updating.
**Fix:** Replaced sequential calls with `Promise.all([updateProfile, updateRole])` when both changes are needed.

---

### ✅ [QOL] AppointmentModal user list cached across modal openings
**Completed:** 2026-03-17
**File:** `admin/src/components/AppointmentModal.vue`
**Issue:** `loadRegisteredUsers` fetched the user list from the API every time the modal opened, even though the list changes rarely.
**Fix:** Added a module-level `cachedUsers` variable. On first load the API is called and results stored; subsequent modal opens return the cached list immediately.

---

### ✅ [QOL] `toggleSection` always reloads files on expand (stale cache fix)
**Completed:** 2026-03-17
**File:** `admin/src/views/MediaLibrary.vue`
**Issue:** `toggleSection` only called `loadSectionFiles` if the section had no cached files. After uploading to a section, collapsing and re-expanding it showed stale data.
**Fix:** Changed to always call `loadSectionFiles` on expand.

---

### ✅ [QOL] Warning toast when section has > 100 files
**Completed:** 2026-03-17
**File:** `admin/src/views/MediaLibrary.vue`
**Issue:** No feedback when a section load was implicitly truncated at 100 files.
**Fix:** Added `toast.warning` when `totalCount > 100`.

---

### ✅ [QOL] Edit Section (rename/description) added to Media Library
**Completed:** 2026-03-17
**File:** `admin/src/views/MediaLibrary.vue`
**Issue:** Section names and descriptions could not be changed after creation without a direct DB edit.
**Fix:** Added "✏️ Edit Section" button per non-system section. Added `openEditSectionModal`, `renameSection` functions. Added Edit Section modal with name + description fields calling `PUT /admin/media/sections/{id}`.

---

### ✅ [QOL] `viewOrder` per-row loading state
**Completed:** 2026-03-17
**File:** `admin/src/views/Orders.vue`
**Issue:** The View button on all order rows showed no loading state while the detail fetch was in progress, and clicking a second row before the first completed caused a race.
**Fix:** Wrapped `viewOrder` in `executeWithLoading(..., 'viewOrder-${id}')`. View button uses `:disabled="isActionLoading('viewOrder-${order.id}')"` with an inline spinner.

---

### ✅ [QOL] `maxlength` added to order notes textareas
**Completed:** 2026-03-17
**File:** `admin/src/views/Orders.vue`
**Issue:** Customer notes, admin notes, and payment notes textareas had no `maxlength` attribute, allowing submission of arbitrarily large strings.
**Fix:** Added `maxlength="1000"` to paymentNotes and `maxlength="2000"` to customerNotes/adminNotes in both edit and create modals.

---

### ✅ [QOL] LeaveSessionAsAgent called when switching sessions in Live Chat
**Completed:** 2026-03-17
**File:** `admin/src/views/LiveChat.vue`
**Issue:** `openSession` joined a new SignalR group without leaving the previous one, causing the agent to receive messages from multiple sessions simultaneously.
**Fix:** Added `previousSessionId` tracking. `openSession` calls `connection.invoke('LeaveSessionAsAgent', previousSessionId)` before joining the new session.

---

### ✅ [QOL] Dashboard stats endpoint now includes order and inquiry counts
**Completed:** 2026-03-17
**Files:** `backend/PowersportsApi/Program.cs`, `admin/src/types/index.ts`, `admin/src/views/Dashboard.vue`
**Issue:** The dashboard stats grid showed products, users, admins, and categories but nothing about orders or contact inquiries — two core operational concerns.
**Fix:** Added `totalOrders`, `pendingOrders`, `totalRevenue` (SUM of `TotalAmount`), and `newInquiries` (count of `ContactStatus.New`) to the backend `GET /admin/dashboard/stats` endpoint. Extended `DashboardStats` TypeScript interface with the 4 new fields. Added two stat cards to the dashboard grid: "Total Orders" (with pending count and total revenue in the sub-line) and "New Inquiries". Skeleton loader count updated from 4 to 6.

---

### ✅ [QOL] Duplicate `.spinner` / `@keyframes spin` CSS removed from scoped view styles
**Completed:** 2026-03-17
**Files:** `admin/src/views/Backup.vue`, `Categories.vue`, `Music.vue`, `Catalog.vue`, `Settings.vue` (×2), `admin/src/style.css`
**Issue:** Six views each declared their own `.spinner { border ... animation: spin ... }` and `@keyframes spin { ... }` blocks verbatim. These were identical to the global `.spinner` and `@keyframes spin` already defined in `style.css`.
**Fix:** Removed all local duplicate blocks. Added `.loading-state .spinner { margin: 0 auto 1rem; }` to `style.css` to preserve the centring behaviour the local versions provided via the `margin` property that the global definition omitted.

---

### ✅ [QOL] `confirm()`/`prompt()`/`alert()` dialogs replaced with inline modals
**Completed:** 2026-03-17
**Files:** `admin/src/views/Calendar.vue`, `Orders.vue`, `LiveChat.vue`, `MediaLibrary.vue`, `Backup.vue`
**Issue:** Five views used native browser `confirm()`, `prompt()`, and `alert()` dialogs. These block the main thread, cannot be styled, and are inconsistent with the rest of the admin's modal UX.
**Fix:** Added a `confirmModal` reactive object + `showConfirmModal` / `closeConfirmModal` / `executeConfirmModal` helpers to each affected view. All single-step confirms replaced with an inline `modal modal-sm` matching the existing Categories/Users pattern. For Backup's 3-step restore flow, implemented a dedicated `restoreModal` with `step` (1–3), `fileName`, `isProtected`, `typedText`, and `mismatch` state — rendering step-appropriate UI at each stage (initial warning → second warning → type-to-confirm input). Removed all `confirm()`, `prompt()`, and `alert()` calls from the codebase.
