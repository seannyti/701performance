# 🎭 Universal Playwright E2E Testing Blueprint
> Reference this file every time you need end-to-end browser testing
> regardless of stack, framework, or project type. Works for new and
> existing projects. Paste the prompt below into Claude in VS Code.

---

```
You have full access to my entire local codebase through VS Code.
Read every single file in this project from the root directory before
you begin. Do not ask me to paste any code. Crawl everything.

You are a world-class quality assurance engineer and test automation
architect specializing in end-to-end browser testing with Playwright.
Your job is to design and implement a complete, production-grade
Playwright test suite that covers every critical user journey, every
protected route, every form, every auth flow, and every integration
in this application — exactly as a real user would experience them
in a real browser.

This is not unit testing. This is not API testing. This is full
browser automation that launches a real browser, interacts with the
real UI, and verifies the real end-to-end behavior of the entire
application stack working together.

Before writing a single test, read and understand:
- Every page and route in the application
- Every user role and permission level
- Every authentication and authorization flow
- Every form and user input mechanism
- Every critical user journey from start to finish
- Every integration point visible in the UI
- Every error state a user could encounter
- The existing test setup if any Playwright config exists
- The frontend framework in use (React, Vue, Angular, etc.)
- The existing testing patterns in this codebase

Adapt every test to the exact application, routes, selectors,
and user flows detected in this codebase. No generic examples.
No placeholder URLs. Every test must be runnable immediately
against this actual application.

Do not begin writing tests until you have completed the full
read and planning phase below.

════════════════════════════════════════════════════════════════════════
PHASE 1 — READ & MAP THE APPLICATION
════════════════════════════════════════════════════════════════════════

Before writing anything, build these maps:

### APPLICATION MAP
- Every page and its URL path
- Every dynamic route and its parameters
- Every protected route (requires auth)
- Every public route (no auth required)
- Every role-restricted route (admin only, etc.)
- Every form and its fields
- Every modal, drawer, or overlay with a form
- Every navigation flow between pages
- Every redirect that should or should not happen

### USER ROLE MAP
- Every user role in the system
- What each role can see
- What each role can do
- What each role cannot access
- How each role logs in
- How each role is created for testing

### CRITICAL USER JOURNEY MAP
A user journey is a complete flow from start to finish
that represents real user behavior. Map every one:
- Registration → email verification → first login
- Login → dashboard → key action → logout
- Forgot password → reset → login with new password
- Every core feature workflow end to end
- Every checkout or transaction flow
- Every settings or profile update flow
- Every admin management flow
- Every error recovery flow

### INTEGRATION POINTS MAP
Every place the UI connects to an external system:
- Payment processors visible in UI
- Email verification flows
- OAuth / social login flows
- File upload and download flows
- Real-time features (websockets, live updates)
- Third-party embedded widgets

Output all maps as structured summaries before
writing any tests.

════════════════════════════════════════════════════════════════════════
PHASE 2 — PLAYWRIGHT SETUP & CONFIGURATION
════════════════════════════════════════════════════════════════════════

### 2.1 INSTALLATION & CONFIGURATION

If Playwright is not already installed, provide the exact
installation commands for this project's package manager.

Generate a complete playwright.config.ts (or .js) that:

BROWSERS:
- Run tests against Chromium (primary)
- Run tests against Firefox
- Run tests against WebKit (Safari)
- Run tests against mobile viewports:
  - Mobile Chrome (Pixel 5)
  - Mobile Safari (iPhone 12)

BASE CONFIGURATION:
- baseURL pointing to the correct local dev server
- testDir pointing to the e2e test directory
- fullyParallel: true for independent tests
- retries: 2 in CI, 0 in local development
- workers: 4 in CI, 1 in local (avoid flakiness)
- reporter: configured for CI and local output
- timeout: 30 seconds per test
- expect timeout: 5 seconds
- actionTimeout: 10 seconds

SCREENSHOTS & VIDEO:
- Screenshot on failure only
- Video on first retry only
- Trace on first retry only
- Store all artifacts in test-results/ directory

PROJECTS:
Configure separate test projects for:
- setup (auth setup runs first)
- chromium (depends on setup)
- firefox (depends on setup)
- webkit (depends on setup)
- mobile-chrome (depends on setup)
- mobile-safari (depends on setup)

GLOBAL SETUP:
- Global setup file that runs before all tests
- Authenticates each user role once and saves state
- Verifies the application is running before tests start
- Seeds any required test data

GLOBAL TEARDOWN:
- Cleans up test data created during the run
- Closes any open connections

### 2.2 DIRECTORY STRUCTURE

Generate this complete structure:

e2e/
├── tests/
│   ├── auth/
│   │   ├── login.spec.ts
│   │   ├── logout.spec.ts
│   │   ├── registration.spec.ts
│   │   ├── password-reset.spec.ts
│   │   └── mfa.spec.ts (if applicable)
│   ├── navigation/
│   │   ├── public-routes.spec.ts
│   │   ├── protected-routes.spec.ts
│   │   └── role-restricted-routes.spec.ts
│   ├── flows/
│   │   └── [one file per critical user journey]
│   ├── forms/
│   │   └── [one file per major form]
│   ├── security/
│   │   ├── auth-enforcement.spec.ts
│   │   ├── role-enforcement.spec.ts
│   │   └── sensitive-data.spec.ts
│   ├── accessibility/
│   │   └── wcag.spec.ts
│   ├── responsive/
│   │   └── mobile-layouts.spec.ts
│   └── error-states/
│       └── error-handling.spec.ts
├── pages/
│   └── [Page Object Model files — one per page]
├── fixtures/
│   ├── auth.fixture.ts
│   └── data.fixture.ts
├── helpers/
│   ├── test-data.ts
│   ├── api-helpers.ts
│   └── assertions.ts
├── auth/
│   ├── .auth/
│   │   └── (saved auth states — gitignored)
│   └── global-setup.ts
├── playwright.config.ts
└── README.md

### 2.3 PAGE OBJECT MODEL (POM)

Every page in the application gets a Page Object class.
This is non-negotiable. Page Objects:
- Encapsulate all selectors for a page in one place
- Expose methods for every action on that page
- Make tests readable and maintainable
- Mean selector changes only require updating one file

PAGE OBJECT STRUCTURE FOR EVERY PAGE:
```typescript
export class [PageName]Page {
  readonly page: Page
  
  // All selectors as readonly Locators
  readonly [element]: Locator
  
  constructor(page: Page) {
    this.page = page
    this.[element] = page.getByRole('[role]', { name: '[name]' })
    // Use accessible selectors in priority order:
    // 1. getByRole (most resilient)
    // 2. getByLabel (for form fields)
    // 3. getByText (for readable text)
    // 4. getByTestId (for elements needing data-testid)
    // 5. locator('[css]') (last resort only)
  }
  
  async goto() {
    await this.page.goto('/[path]')
    await this.waitForLoad()
  }
  
  async waitForLoad() {
    // Wait for the page-specific element that confirms
    // the page has fully loaded
  }
  
  // One method per user action on this page
  async [action]([params]) {
    // implementation
  }
}
```

SELECTOR PRIORITY RULES:
1. getByRole — always preferred (accessible, resilient)
2. getByLabel — for form inputs
3. getByText — for buttons and links with visible text
4. getByTestId — add data-testid to elements that
   have no accessible name (sparingly)
5. CSS selectors — absolute last resort, never use
   auto-generated class names or IDs

NEVER USE:
- CSS class names that look auto-generated
- XPath
- nth-child selectors that depend on DOM order
- Selectors tied to implementation details

### 2.4 AUTHENTICATION FIXTURES

Set up authentication once per role, save state,
reuse across all tests:

```typescript
// Global setup — runs once before all tests
async function globalSetup(config: FullConfig) {
  // For each user role:
  // 1. Launch browser
  // 2. Perform full login flow
  // 3. Save authenticated state to file
  // 4. Tests reuse saved state — no repeated logins
}
```

Create fixture files for each role:
- Regular authenticated user
- Admin user
- Each additional role in the system
- Unauthenticated (default browser state)

════════════════════════════════════════════════════════════════════════
MODULE 1 — AUTHENTICATION TESTS
════════════════════════════════════════════════════════════════════════

### LOGIN TESTS

HAPPY PATH:
- [ ] Valid credentials → successful login → redirect to
      correct post-login page
- [ ] Login form fields accept correct input types
- [ ] Password field masks characters
- [ ] Remember me functionality works (if applicable)
- [ ] Login with each valid user role works correctly
- [ ] Post-login redirect preserves intended destination
      (user goes to /dashboard/settings, gets redirected
      to login, logs in, lands on /dashboard/settings)

VALIDATION:
- [ ] Empty email shows validation error
- [ ] Empty password shows validation error
- [ ] Invalid email format shows validation error
- [ ] Form cannot be submitted with empty required fields
- [ ] Error messages are visible and descriptive

FAILED LOGIN:
- [ ] Wrong password shows generic error message
      (not "wrong password" — just "invalid credentials")
- [ ] Non-existent email shows same generic error
      as wrong password (enumeration prevention)
- [ ] Error message does not reveal whether email exists

ACCOUNT LOCKOUT:
- [ ] After [N] failed attempts account is locked
- [ ] Locked account shows appropriate message
- [ ] Correct credentials on locked account still blocked

SECURITY:
- [ ] Password is not visible in page source
- [ ] Password is not in any network request as plaintext
- [ ] Auth token/cookie is set after successful login
- [ ] Auth cookie has correct flags (check via evaluate)

### LOGOUT TESTS

- [ ] Logout button is accessible and visible
- [ ] Clicking logout redirects to login or home page
- [ ] After logout, browser back button cannot access
      protected pages
- [ ] After logout, direct URL to protected page
      redirects to login
- [ ] Session is fully cleared after logout

### REGISTRATION TESTS (if applicable)

HAPPY PATH:
- [ ] Complete valid registration → confirmation message
      or redirect
- [ ] All required fields accept correct input
- [ ] Password strength indicator works (if present)
- [ ] Terms acceptance works (if required)

VALIDATION:
- [ ] Every required field shows error when empty
- [ ] Invalid email format rejected
- [ ] Password too short rejected
- [ ] Password confirmation mismatch rejected
- [ ] Duplicate email shows appropriate message
      (without revealing whether email is registered
      to another user — security consideration)

EMAIL VERIFICATION (if applicable):
- [ ] Verification email flow can be simulated
- [ ] Unverified account cannot access protected features
- [ ] Resend verification works

### PASSWORD RESET TESTS

- [ ] Password reset link visible and accessible on login page
- [ ] Submitting email shows confirmation message
- [ ] Non-existent email shows same confirmation as valid
      (enumeration prevention — same message either way)
- [ ] Reset form accepts new password
- [ ] New password confirmation mismatch rejected
- [ ] After reset, login with new password works
- [ ] After reset, login with old password fails

### MFA TESTS (if applicable)

- [ ] MFA prompt appears after correct password
- [ ] Invalid MFA code shows error
- [ ] Valid MFA code completes login
- [ ] MFA setup flow works end to end

════════════════════════════════════════════════════════════════════════
MODULE 2 — NAVIGATION & ROUTE PROTECTION TESTS
════════════════════════════════════════════════════════════════════════

### PUBLIC ROUTES

For every public route in the application:
- [ ] Page loads successfully without authentication
- [ ] Page displays expected content
- [ ] Navigation elements are present
- [ ] Page title is correct
- [ ] No console errors on load

### PROTECTED ROUTES — UNAUTHENTICATED ACCESS

For every protected route:
- [ ] Direct URL access without auth redirects to login
- [ ] Redirect preserves the intended destination URL
- [ ] No protected content is briefly visible before redirect
      (flash of protected content)
- [ ] 401/403 page is shown appropriately (if no redirect)

### PROTECTED ROUTES — AUTHENTICATED ACCESS

For every protected route with each valid role:
- [ ] Page loads successfully when authenticated
- [ ] Expected content is visible
- [ ] Navigation reflects authenticated state
- [ ] User-specific content is correct for logged in user

### ROLE-RESTRICTED ROUTES

For every role-restricted route:
- [ ] Admin user can access admin pages
- [ ] Regular user accessing admin URL sees 403 or redirect
- [ ] Role-specific content only visible to correct role
- [ ] Navigation items only shown to roles that can use them

════════════════════════════════════════════════════════════════════════
MODULE 3 — CRITICAL USER JOURNEY TESTS
════════════════════════════════════════════════════════════════════════

For every critical user journey mapped in Phase 1,
generate a complete end-to-end test that:
- Starts from the beginning of the journey (often login
  or a specific landing page)
- Completes every step of the journey
- Verifies the correct outcome at each step
- Verifies the final state is correct
- Cleans up any created data after the test

JOURNEY TEST STRUCTURE:
```typescript
test.describe('[Journey Name]', () => {
  test('complete [journey] from start to finish',
  async ({ page }) => {
    // Step 1: [action]
    // Verify: [expected state]
    
    // Step 2: [action]
    // Verify: [expected state]
    
    // ... continue for every step
    
    // Final verification: [expected outcome]
  })
})
```

EVERY JOURNEY TEST MUST VERIFY:
- The correct page is shown at each step
- The correct data is displayed at each step
- The correct URL is shown at each step
- Success states are clearly visible
- No unexpected errors or console errors occur

COMMON JOURNEYS TO COVER
(adapt to what actually exists in this application):

- [ ] New user registration through first meaningful action
- [ ] Returning user login through core task completion
- [ ] Password reset through successful re-login
- [ ] Core feature creation flow (create resource → view it)
- [ ] Core feature edit flow (find resource → edit → verify)
- [ ] Core feature deletion flow (find → delete → verify gone)
- [ ] Search and filter flow (search → filter → find result)
- [ ] Settings update flow (change setting → verify applied)
- [ ] Any payment or transaction flow
- [ ] Any file upload flow
- [ ] Any export or download flow
- [ ] Any notification or alert flow
- [ ] Admin management flow (create user, assign role, etc.)
- [ ] Error recovery flow (trigger error → recover → continue)

════════════════════════════════════════════════════════════════════════
MODULE 4 — FORM TESTS
════════════════════════════════════════════════════════════════════════

For every significant form in the application:

### HAPPY PATH
- [ ] Form loads with correct fields
- [ ] All fields accept valid input
- [ ] Submission with valid data succeeds
- [ ] Success state is visible after submission
- [ ] Data appears correctly after submission
  (navigate to where it should appear and verify)

### VALIDATION
For every required field:
- [ ] Empty submission shows field-level error
- [ ] Error message is visible and descriptive
- [ ] Error is associated with the correct field
- [ ] Error clears when field is filled correctly

For every field with format requirements:
- [ ] Invalid format shows appropriate error
- [ ] Valid format is accepted
- [ ] Boundary values are handled (min/max length)

### FIELD BEHAVIOR
- [ ] Required fields are marked (asterisk or label)
- [ ] Optional fields work when left empty
- [ ] Dropdowns/selects show correct options
- [ ] Date pickers accept valid dates
- [ ] Date pickers reject invalid dates
- [ ] File inputs accept correct file types
- [ ] File inputs reject incorrect file types
- [ ] File size limits are enforced
- [ ] Character counters work (if present)
- [ ] Autocomplete suggestions appear (if applicable)

### FORM UX
- [ ] Tab order is logical through all fields
- [ ] Form submission shows loading state
- [ ] Double submission is prevented during loading
- [ ] Cancel/back works without saving

════════════════════════════════════════════════════════════════════════
MODULE 5 — SECURITY E2E TESTS
════════════════════════════════════════════════════════════════════════

These tests verify security controls work in the real browser,
complementing the API-level security tests from the
cybersecurityaudit.md suite.

### AUTH ENFORCEMENT IN THE BROWSER

- [ ] Accessing any protected URL without auth in a fresh
      browser redirects to login
- [ ] Accessing any protected URL after logout redirects
      to login (session actually cleared)
- [ ] Auth cookie/token is not accessible via JavaScript
      (HttpOnly flag enforced — test via page.evaluate
      attempting to read document.cookie)
- [ ] Auth state does not persist after browser data is cleared
- [ ] Concurrent sessions: logging out in one tab affects
      behavior in another tab (if single session policy)

### ROLE ENFORCEMENT IN THE BROWSER

- [ ] Admin UI elements not visible to regular users
- [ ] Admin routes redirect regular users
- [ ] Changing role-related parameters in URL does not
      grant elevated access
- [ ] API calls made by the UI include correct auth headers
      (verify via page.on('request') listener)

### SENSITIVE DATA IN THE BROWSER

- [ ] Passwords never appear in the DOM
- [ ] Tokens never appear in the DOM or page source
- [ ] Sensitive user data not visible in URL parameters
- [ ] Browser DevTools Network tab shows no sensitive
      data in query strings
- [ ] Autocomplete is disabled on sensitive fields
      (password, card number, CVV)

### XSS IN THE BROWSER

For every field that displays user-entered content:
- [ ] Script tags in input are not executed when displayed
- [ ] HTML tags in input are escaped when displayed
- [ ] URL inputs do not create javascript: links
- [ ] Verify by entering payload and checking the DOM
      shows escaped content, not executed script

### CONTENT SECURITY

- [ ] No mixed content warnings (HTTP resources on HTTPS)
- [ ] No console security errors on any page
- [ ] HTTPS is used for all pages (check URL bar)
- [ ] CSP violations do not appear in console

════════════════════════════════════════════════════════════════════════
MODULE 6 — ACCESSIBILITY TESTS
════════════════════════════════════════════════════════════════════════

Use @axe-core/playwright for automated accessibility scanning:

### AUTOMATED WCAG SCANNING

For every page:
- [ ] Run axe accessibility scan on page load
- [ ] Zero critical accessibility violations
- [ ] Zero serious accessibility violations
- [ ] Document and track any moderate violations

```typescript
import { checkA11y } from 'axe-playwright'

test('homepage has no accessibility violations', async ({ page }) => {
  await page.goto('/')
  await checkA11y(page, undefined, {
    detailedReport: true,
    detailedReportOptions: { html: true }
  })
})
```

### KEYBOARD NAVIGATION

- [ ] All interactive elements reachable by Tab key
- [ ] Tab order is logical and follows visual layout
- [ ] Focus indicator is visible on all interactive elements
- [ ] Modal/dialog traps focus correctly
- [ ] Escape key closes modals and dropdowns
- [ ] Forms submittable by pressing Enter
- [ ] Dropdown menus navigable by arrow keys

### SCREEN READER COMPATIBILITY

- [ ] All images have descriptive alt text
- [ ] Form fields have associated labels
- [ ] Error messages are associated with their fields
- [ ] Page has a logical heading hierarchy (h1, h2, h3)
- [ ] Buttons have descriptive accessible names
  (not just "Click here" or "Submit")
- [ ] Links have descriptive accessible names
- [ ] Dynamic content updates announced to screen readers
  (via aria-live regions where appropriate)

### COLOR & CONTRAST

- [ ] Text meets WCAG AA contrast ratio (4.5:1 for normal,
  3:1 for large text)
- [ ] Interactive elements meet contrast requirements
- [ ] Information not conveyed by color alone
  (error states use icon or text, not just red color)

════════════════════════════════════════════════════════════════════════
MODULE 7 — RESPONSIVE & CROSS-BROWSER TESTS
════════════════════════════════════════════════════════════════════════

### RESPONSIVE LAYOUT TESTS

Test every major page at these viewports:
- Desktop: 1440px × 900px
- Laptop: 1280px × 800px
- Tablet: 768px × 1024px
- Mobile: 375px × 812px (iPhone)
- Mobile: 360px × 800px (Android)

For each viewport verify:
- [ ] Navigation is appropriate for screen size
  (hamburger menu on mobile, full nav on desktop)
- [ ] No horizontal scrollbar on mobile
- [ ] Text is readable without zooming
- [ ] Buttons and links are large enough to tap on mobile
  (minimum 44px × 44px touch target)
- [ ] Forms are usable on mobile
- [ ] Tables reflow or scroll correctly on small screens
- [ ] Images scale correctly
- [ ] No overlapping elements

### CROSS-BROWSER TESTS

Run the critical user journeys on all configured browsers:
- [ ] Chromium: all critical journeys pass
- [ ] Firefox: all critical journeys pass
- [ ] WebKit: all critical journeys pass

Browser-specific checks:
- [ ] Fonts render correctly in all browsers
- [ ] CSS animations work in all browsers
- [ ] Form behaviors consistent across browsers
- [ ] Date pickers work in all browsers
- [ ] File uploads work in all browsers

### MOBILE-SPECIFIC TESTS

- [ ] Touch gestures work (tap, swipe if applicable)
- [ ] Keyboard does not obscure focused input fields
- [ ] Pinch to zoom not broken for content
- [ ] Mobile navigation menu opens and closes correctly
- [ ] Links and buttons respond to touch correctly

════════════════════════════════════════════════════════════════════════
MODULE 8 — ERROR STATE TESTS
════════════════════════════════════════════════════════════════════════

### NETWORK ERROR STATES

- [ ] Application handles API failure gracefully
  (shows error message, not blank screen or crash)
- [ ] Application handles slow network gracefully
  (loading states visible during slow responses)
- [ ] Application handles timeout gracefully
- [ ] Retry mechanisms work when shown to user

### 404 & ERROR PAGES

- [ ] Navigating to a non-existent URL shows 404 page
- [ ] 404 page has navigation back to valid pages
- [ ] 404 page does not expose internal information
- [ ] 403 page shown when accessing restricted content
- [ ] Error pages are styled correctly (not raw HTML)

### FORM ERROR STATES

- [ ] Server-side validation errors display correctly
- [ ] Network error during form submission shows message
- [ ] Form data is preserved when submission fails
  (user does not lose entered data on error)

### EMPTY STATES

- [ ] Lists with no data show appropriate empty state
- [ ] Search with no results shows appropriate message
- [ ] Dashboard with no activity shows appropriate state
- [ ] Empty states have clear calls to action

════════════════════════════════════════════════════════════════════════
MODULE 9 — PERFORMANCE TESTS
════════════════════════════════════════════════════════════════════════

### CORE WEB VITALS

Use Playwright's built-in performance APIs to measure:

- [ ] Largest Contentful Paint (LCP) < 2.5 seconds
- [ ] First Input Delay (FID) preparation: measure
      time to interactive
- [ ] Cumulative Layout Shift (CLS) < 0.1
      (no unexpected layout jumps during load)
- [ ] First Contentful Paint (FCP) < 1.8 seconds
- [ ] Time to Interactive (TTI) < 3.8 seconds

```typescript
test('homepage meets performance thresholds', async ({ page }) => {
  await page.goto('/')
  
  const metrics = await page.evaluate(() => ({
    lcp: performance.getEntriesByType('largest-contentful-paint')
         .pop()?.startTime,
    fcp: performance.getEntriesByType('paint')
         .find(e => e.name === 'first-contentful-paint')?.startTime,
    cls: /* calculate from layout-shift entries */
  }))
  
  expect(metrics.lcp).toBeLessThan(2500)
  expect(metrics.fcp).toBeLessThan(1800)
})
```

### PAGE LOAD TESTS

For every major page:
- [ ] Page loads within acceptable time threshold
- [ ] No render-blocking resources
- [ ] Images load without layout shift
- [ ] Fonts load without invisible text flash

════════════════════════════════════════════════════════════════════════
MODULE 10 — CI/CD INTEGRATION
════════════════════════════════════════════════════════════════════════

Generate a complete CI/CD configuration for Playwright:

### PIPELINE CONFIGURATION

```yaml
e2e-tests:
  runs-on: ubuntu-latest
  steps:
    - name: Checkout
    
    - name: Install dependencies
    
    - name: Install Playwright browsers
      run: npx playwright install --with-deps
      
    - name: Start application
      # Start the application against test database
      # Wait for it to be ready before running tests
      
    - name: Run Playwright tests
      run: npx playwright test
      env:
        # Test environment variables
        
    - name: Upload test results
      if: always()
      uses: actions/upload-artifact@v3
      with:
        name: playwright-report
        path: playwright-report/
        retention-days: 30
        
    - name: Upload test videos on failure
      if: failure()
      uses: actions/upload-artifact@v3
      with:
        name: test-videos
        path: test-results/
```

### PIPELINE RULES:

- ANY failing E2E test blocks deployment to production
- Screenshots of failures are always archived
- Video recordings of failures are always archived
- Traces of failures are always archived
- Full HTML report is always generated and archived
- Flaky tests are retried twice before marking as failed
- Test results are compared to previous run for regression
- Parallel execution across multiple machines in CI

### SCHEDULED E2E RUNS:

DAILY (against staging environment):
- Full authentication test suite
- Full critical journey suite
- Full security E2E suite
- Full accessibility suite

WEEKLY:
- Full cross-browser suite
- Full responsive suite
- Full performance suite
- Full error state suite

════════════════════════════════════════════════════════════════════════
TEST QUALITY STANDARDS
════════════════════════════════════════════════════════════════════════

Every test in this suite must meet ALL of these standards:

RELIABILITY:
- [ ] Test passes consistently — no flakiness
      (if a test fails intermittently it is fixed
      before being committed, not retried away)
- [ ] Test does not depend on another test's state
- [ ] Test does not depend on execution order
- [ ] Test cleans up created data after itself
- [ ] Test uses explicit waits, never arbitrary timeouts
      (await page.waitForSelector not await page.waitForTimeout)
- [ ] Test waits for network idle or specific elements,
      never for a fixed number of milliseconds

SELECTORS:
- [ ] Uses accessible selectors (role, label, text)
- [ ] Does not use auto-generated class names
- [ ] Does not use XPath
- [ ] Resilient to minor UI changes that don't affect function

READABILITY:
- [ ] Test name clearly describes what user behavior
      is being verified
- [ ] Uses Should/When pattern:
      "should redirect to login when accessing protected
       page without authentication"
- [ ] Steps are commented to explain user intent
- [ ] Uses Page Object Model — no raw selectors in tests
- [ ] Test is readable by someone unfamiliar with the codebase

SPEED:
- [ ] Each test completes in under 30 seconds
- [ ] Authentication reuses saved state (no repeated logins)
- [ ] Tests run in parallel where possible
- [ ] No unnecessary page navigations or waits

════════════════════════════════════════════════════════════════════════
OUTPUT FORMAT
════════════════════════════════════════════════════════════════════════

Produce the following in this exact order:

### 1. APPLICATION MAP SUMMARY
- Every page and route discovered
- Every user role identified
- Every critical journey mapped
- Any gaps or ambiguities flagged

### 2. PLAYWRIGHT CONFIGURATION
- Complete playwright.config.ts
- Complete global setup file
- Complete global teardown file
- All fixture files
- .gitignore additions for test artifacts

### 3. PAGE OBJECT MODEL FILES
Complete POM class for every page discovered,
with all selectors and action methods.

### 4. ALL TEST FILES
Complete, runnable test files for every module:
- auth/login.spec.ts
- auth/logout.spec.ts
- auth/registration.spec.ts (if applicable)
- auth/password-reset.spec.ts
- navigation/public-routes.spec.ts
- navigation/protected-routes.spec.ts
- navigation/role-restricted-routes.spec.ts
- flows/[journey-name].spec.ts (one per journey)
- forms/[form-name].spec.ts (one per major form)
- security/auth-enforcement.spec.ts
- security/role-enforcement.spec.ts
- security/sensitive-data.spec.ts
- accessibility/wcag.spec.ts
- responsive/mobile-layouts.spec.ts
- error-states/error-handling.spec.ts

Every test must use real routes, real selectors,
and real user flows from this application.
No placeholders. Runnable immediately.

### 5. HELPER FILES
- Complete test-data.ts
- Complete api-helpers.ts
- Complete assertions.ts

### 6. CI/CD CONFIGURATION
Complete pipeline configuration file for the CI/CD
system used or recommended for this project.

### 7. README.md
Complete documentation including:
- How to install Playwright
- How to run the full suite
- How to run a single test file
- How to run tests in headed mode (to watch)
- How to run tests in UI mode (Playwright UI)
- How to view the HTML report
- How to debug a failing test
- How to add new tests
- How to update Page Objects when UI changes
- CI/CD setup instructions

### 8. INITIAL RUN REPORT
After generating all files, report:
- Total tests generated: [N]
- Tests by module
- Pages covered: [N]/[total pages]
- User journeys covered: [N]/[total journeys]
- Any pages or journeys not covered and why
- Recommended order to fix any gaps
```

---

## When to Use This File

**Setting up E2E testing on an existing project:**
```
Read the entire codebase and follow the
playwrightblueprint.md standards. Map the application
and generate the complete E2E test suite.
```

**Adding E2E tests for a new feature:**
```
A new [feature] has been added. Follow the
playwrightblueprint.md standards and add E2E tests
for every new page, form, and user journey it introduces.
```

**After a major UI refactor:**
```
The UI has been refactored. Update all Page Object
Models and verify all E2E tests still pass.
```

---

## How This Fits With the Other Blueprints

```
apibuildingblueprint.md     → API is built correctly
        ↓
cybersecurityaudit.md       → API is secured and API-level
                              security tests generated
        ↓
fileintegrityblueprint.md   → Codebase is clean and correct
        ↓
playwrightblueprint.md      → Full browser E2E tests verify
                              everything works together as
                              a real user experiences it
```

---

*Covers: Playwright setup and configuration, Page Object Model,
authentication fixtures, login/logout/registration/password reset
testing, route protection verification, critical user journey testing,
form testing, browser-level security testing, WCAG accessibility
scanning, keyboard navigation, cross-browser testing, responsive
layout testing, mobile testing, error state testing, Core Web Vitals
performance testing, CI/CD integration, and scheduled test runs —
for any stack, any framework, any project.*
