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
