# 🏗️ Universal File Structure & Integrity Blueprint
> The most thorough file structure, code integrity, route integrity, dependency integrity,
> type integrity, build integrity, test integrity, and documentation integrity audit
> ever run on a codebase. Stack-agnostic. Works on any project, any language, any size.
> Paste the prompt below into Claude in VS Code with your project open at the root.

---

```
You have full access to my entire local codebase through VS Code.
Read every single file in this project from the root directory before
you begin. Do not ask me to paste any code. Crawl everything.

You are a world-class software architect, code quality engineer, and
codebase health specialist. Your job is to perform the most exhaustive
file structure and integrity audit ever run on this codebase.

You are not scanning file by file. You are analyzing the entire codebase
as one interconnected system simultaneously. Every file, every function,
every route, every import, every export, every type, every test, every
config, every dependency, every doc — traced across the full project
graph at once.

You do not flag anything without proof. Before marking anything as
unused, stale, incorrect, or dead — confirm it by tracing every
reference across the entire codebase. Dead code that references other
dead code does not count as usage. A false positive that causes deletion
of needed code is worse than leaving stale code in place.

Do not begin writing findings until you have read and fully mapped the
entire codebase. Build the complete dependency graph, route map, import
graph, and type graph in your understanding before making a single
finding.

Adapt every finding and recommendation to the exact stack, language,
framework, and conventions detected in this codebase. No pseudocode.
No generic advice. Everything must be specific, actionable, and
production-ready.

════════════════════════════════════════════════════════════════════════
PHASE 1 — FULL CODEBASE MAPPING
════════════════════════════════════════════════════════════════════════

Before any analysis, build these maps in full:

### MAP 1 — COMPLETE FILE INVENTORY
List every file in the project with:
- Full path
- File type and purpose
- File size (lines of code)
- Last known role in the application
- Direct dependencies (files it imports)
- Dependents (files that import it)

### MAP 2 — COMPLETE DEPENDENCY GRAPH
For every file trace:
- Every import/require/use statement
- What it imports (specific exports or entire module)
- Whether each imported item is actually used
- Whether the imported file exists
- Whether the import creates a circular chain
- The full circular chain if it exists

### MAP 3 — COMPLETE ROUTE MAP
For every route in the application trace:
- HTTP method and full path
- Where it is registered (file, line)
- What middleware is attached (in order)
- What handler it points to (file, function)
- Whether the handler exists
- Whether the handler is reachable
- Whether the route is called by any client code
- Whether auth/guard middleware is present

### MAP 4 — COMPLETE EXPORT/IMPORT GRAPH
For every export in the codebase trace:
- What is exported (function, class, variable, type)
- From which file
- Which other files import it
- Whether each import actually uses what it imported
- Whether any export has zero consumers

### MAP 5 — COMPLETE TYPE GRAPH
(For typed languages: TypeScript, C#, Java, Go, Rust, etc.)
For every type, interface, class, enum trace:
- Where it is defined
- Where it is used
- Whether it has any consumers
- Whether it is correctly implemented

### MAP 6 — COMPLETE TEST MAP
For every test file trace:
- What module or feature it tests
- Whether that module or feature still exists
- Every test case — does it have assertions?
- Every test case — is it skipped?
- Every test case — does it actually test the real code
  or just the mock?

### MAP 7 — COMPLETE DEPENDENCY MANIFEST MAP
For every package in every manifest file trace:
- Package name and version
- Where it is imported in the codebase
- Whether it is used in production code,
  test code only, or nowhere
- Whether it belongs in dependencies vs devDependencies
- Whether a duplicate package exists

Output all 7 maps as structured summaries before
proceeding to analysis.

════════════════════════════════════════════════════════════════════════
DOMAIN A — DEPENDENCY GRAPH ANALYSIS
════════════════════════════════════════════════════════════════════════

### A.1 CIRCULAR DEPENDENCIES

Detect every circular dependency chain in the codebase:
- Not just pairs (A → B → A) but full chains
  (A → B → C → D → A)
- Map every file involved in every circular chain
- Identify which circular dependencies cause actual runtime
  problems vs which are benign
- Identify the cleanest break point in each chain
- Propose the exact refactor to eliminate each circle
  without breaking functionality

Flag as Critical: any circular dependency that affects
application startup, module loading order, or causes
runtime errors

Flag as High: any circular dependency between core
business logic modules

Flag as Medium: circular dependencies in utility or
helper modules

### A.2 IMPORT PATH CONSISTENCY

Scan every import statement across the entire codebase:
- Are relative imports (../../utils/helper) mixed with
  absolute imports (@/utils/helper) inconsistently?
- Are path aliases defined and used consistently?
- Are any path aliases defined but never used?
- Are any path aliases used but not defined in config?
- Are any imports going more than 3 directories up?
  (../../../../ is a structural problem)
- Are there imports that should use a barrel/index file
  but don't?
- Are there barrel files that should be imported directly
  but aren't (causing over-importing)?

### A.3 BARREL FILE HEALTH

For every index file / barrel file in the codebase:
- Does it re-export everything in its directory?
  (if so, is that intentional or accidental over-exposure?)
- Does it re-export only selected items?
  (are the selected items actually used by consumers?)
- Does it re-export items that are never imported by
  any consumer?
- Is it creating circular dependencies?
- Is it preventing effective tree-shaking?
- Are consumers importing from the barrel when they should
  import directly (and vice versa)?

### A.4 IMPORT USAGE ACCURACY

For every import statement across the codebase:
- Is every imported item actually used in the file?
- Are wildcard imports (import * as X) used fully
  or partially?
- Are there named imports where only one item is used
  but the entire module is effectively loaded?
- Are there side-effect imports that are no longer needed?
- Are there type-only imports that should use
  import type syntax?

### A.5 TREE-SHAKING COMPATIBILITY

Scan the module structure for tree-shaking issues:
- Are there exports that bundle everything into one object
  preventing dead code elimination?
- Are there side effects in module initialization that
  prevent tree-shaking?
- Are large libraries imported in ways that pull in the
  entire library when only one function is needed?
  (e.g., import _ from 'lodash' vs import {get} from 'lodash')
- Are there re-export patterns that prevent bundlers from
  eliminating unused code?

════════════════════════════════════════════════════════════════════════
DOMAIN B — ROUTE INTEGRITY
════════════════════════════════════════════════════════════════════════

### B.1 ROUTE REGISTRATION AUDIT

For every route registered in the application:

HANDLER VERIFICATION:
- Does the handler function exist?
- Is the handler function in the expected file?
- Does the handler function's parameter signature match
  what the route framework passes to it?
- Are route parameters in the URL pattern consistent
  with how the handler accesses them?
  (e.g., :userId in route vs req.params.user_id in handler)
- Does the handler return a response in every code path?
  (missing return on error path leaves requests hanging)

MIDDLEWARE VERIFICATION:
- Is every middleware function on this route defined?
- Is every middleware function actually doing what it
  claims to do?
- Is the middleware order correct?
  (auth before validation before business logic — always)
- Is authentication middleware present on every route
  that requires it?
- Is there any route that should have auth middleware
  but doesn't?
- Is rate limiting middleware present on every route?
- Is there any route that has middleware attached that
  it does not need?
- Are there middleware functions defined but never
  attached to any route?

### B.2 DUPLICATE & CONFLICTING ROUTES

Scan every route registration for:
- Exact duplicate routes (same method + same path
  registered twice — last one wins silently)
- Conflicting routes where a wildcard or param route
  shadows a specific route
  (e.g., GET /users/:id registered before GET /users/me
   means /users/me is unreachable)
- Routes with the same path but different casing
  (case sensitivity depends on framework — flag ambiguity)
- Routes that differ only by trailing slash behavior
  (GET /users vs GET /users/ — is this handled?)
- Multiple routers mounting at the same path prefix

### B.3 DEAD ROUTES

Identify every route that is registered but never called:
- Trace every route to any client-side code, test, or
  external caller that references it
- A route is dead if no frontend code, no test, no
  script, no external documented consumer ever calls it
- Flag separately: routes that exist in tests only
  (may be fine but should be confirmed intentional)
- Flag separately: routes that are documented in OpenAPI
  but have no corresponding implementation
- Flag separately: routes that have an implementation
  but are not in the OpenAPI spec

### B.4 ROUTE PARAMETER INTEGRITY

For every route with URL parameters:
- Is every URL parameter used in the handler?
- Is every URL parameter validated before use?
- Are URL parameters accessed by the correct name
  in the handler?
- Are there handlers that access parameters that
  don't exist in the route definition?
- Are optional parameters handled correctly when absent?

### B.5 ROUTE RESPONSE INTEGRITY

For every route handler:
- Does every code path in the handler return a response?
- Are there code paths that fall through without
  returning anything (hanging request)?
- Are there code paths that could return twice
  (headers already sent errors)?
- Are all possible exceptions caught or propagated
  to the global error handler?
- Is the response type consistent with what the
  OpenAPI spec documents for this route?

### B.6 ROUTE GUARD & AUTH MIDDLEWARE MAP

Produce a complete map of every route with its
auth/guard status:

FORMAT:
[METHOD] [PATH]
  Auth required: Yes/No
  Auth middleware: [name or MISSING]
  Role/permission required: [role or None]
  Role middleware: [name or MISSING]
  Rate limited: Yes/No
  Rate limit middleware: [name or MISSING]
  Security gaps: [list any missing controls]

Flag every route missing required security controls.

════════════════════════════════════════════════════════════════════════
DOMAIN C — BUILD & BUNDLE INTEGRITY
════════════════════════════════════════════════════════════════════════

### C.1 VERSION CONTROL CONTAMINATION

Scan for files that should never be in version control:
- Build output directories (dist/, build/, out/, .next/,
  .nuxt/, target/, bin/, obj/)
- Compiled files (.pyc, __pycache__, *.class, *.o, *.a)
- Dependency directories (node_modules/, vendor/, venv/,
  .venv/, packages/)
- Generated files (*.generated.ts, *.g.cs, migrations
  that are auto-generated)
- IDE and editor files (.idea/, .vscode/ settings,
  *.suo, *.user)
- OS files (.DS_Store, Thumbs.db, desktop.ini)
- Log files (*.log, logs/)
- Test coverage output (coverage/, .nyc_output/)
- Environment files (.env, .env.local, .env.production)
- Secret files (*.pem, *.key, *.pfx, *.p12, secrets.*)
- Temporary files (*.tmp, *.temp, *.bak, *.swp)

For each found: confirm it is in .gitignore.
Generate a complete, corrected .gitignore if gaps exist.

### C.2 BUNDLE SIZE ANALYSIS

For frontend or compiled projects:
- Are there imports that pull an entire large library
  for a single function?
  (moment.js, lodash, etc. — flag for tree-shaking or
  replacement with lighter alternatives)
- Are there assets (images, fonts, videos) that are
  not optimized or compressed?
- Are there large files being imported synchronously
  that should be lazy-loaded?
- Are there duplicate assets in different formats
  (same image as jpg and png)?
- Are there unused assets that add to bundle size?
- Are chunks configured for optimal splitting?

### C.3 SOURCE MAP INTEGRITY

- Are source maps committed to version control?
  (should not be — they reveal source code structure)
- Are source maps configured for production?
  (should not be — they expose source code to users)
- Are source maps available for development debugging?
  (should be — but only in dev)

### C.4 BUILD SCRIPT INTEGRITY

Scan every build script in package.json, Makefile,
Dockerfile, or equivalent:
- Are all referenced files and commands present?
- Are build scripts consistent across environments?
- Are there build steps that are never run?
- Are there build steps that duplicate each other?
- Are secrets being passed through build arguments
  (they end up in image layers — Critical)?
- Are build artifacts deterministic (same input = same output)?

════════════════════════════════════════════════════════════════════════
DOMAIN D — TYPE SYSTEM INTEGRITY
════════════════════════════════════════════════════════════════════════

(Apply to TypeScript, C#, Java, Go, Rust, Python with
type hints, or any other typed language in this project.
Skip sections not applicable to the detected stack.)

### D.1 UNUSED TYPES & INTERFACES

For every type, interface, class, and enum:
- Is it referenced anywhere in the codebase?
- Is it only referenced by other unused types?
  (dead type chain — the whole chain is dead)
- Is it exported but never imported by any consumer?
- Is it a duplicate of another type with a different name?

### D.2 TYPE CORRECTNESS

Scan for type integrity violations:
- Any type assertion that bypasses the type system:
  (as unknown as X, @ts-ignore, @ts-expect-error,
  any!, force-unwrap, unsafe cast)
  Flag each one and determine if it is justified or hiding a bug
- Any use of 'any' or 'object' or dynamic that should
  be a specific type
- Any implicit 'any' where a type should be specified
- Any type that claims a value is non-null/non-undefined
  when it actually can be
- Any function that returns a type inconsistent with
  its declared return type

### D.3 INTERFACE & CONTRACT INTEGRITY

For every interface or abstract type:
- Does at least one concrete implementation exist?
- Do all implementations correctly implement the full
  interface contract?
- Are there interface methods that no implementation
  ever calls?
- Are there implementations that add methods not in
  the interface that are used externally
  (bypassing the abstraction)?

### D.4 ENUM INTEGRITY

For every enum:
- Is every enum value used somewhere in the codebase?
- Are switch/match statements on enums exhaustive?
  (do they handle every possible value?)
- Are there enums with only one value?
  (should be a constant)
- Are there enums that duplicate the values of
  another enum?

### D.5 GENERIC TYPE INTEGRITY

For every generic type or function:
- Are the type parameters constrained appropriately?
- Are there unconstrained type parameters that should
  be constrained?
- Are there generic types that are always used with
  the same concrete type? (should it just be concrete?)

### D.6 TYPE CONFIGURATION INTEGRITY

For TypeScript specifically:
- Is strict mode enabled in tsconfig?
- Is noImplicitAny enabled?
- Is strictNullChecks enabled?
- Is noUnusedLocals enabled?
- Is noUnusedParameters enabled?
- Are there tsconfig paths that don't match actual
  directory structure?
- Are there multiple tsconfig files that conflict?

════════════════════════════════════════════════════════════════════════
DOMAIN E — TEST INTEGRITY
════════════════════════════════════════════════════════════════════════

### E.1 TEST COVERAGE MAP

For every module, service, controller, utility, and
significant function in the codebase:
- Does a test file exist for it?
- If no test file exists — flag as untested
- If a test file exists — does it actually import and
  test the real module (not a copy or mock of it)?

### E.2 DEAD TEST FILES

For every test file in the codebase:
- Does the module, feature, or function it tests
  still exist?
- If the tested module was deleted or renamed and the
  test was not — flag as dead test file
- If the test imports a path that no longer exists —
  flag as broken test (may still pass if the import
  is mocked)

### E.3 ASSERTION INTEGRITY

For every test case across the entire test suite:
- Does it have at least one assertion?
  (a test with no assertions always passes — this is
  the most dangerous kind of test, it gives false confidence)
- Are assertions actually checking the right thing?
  (asserting on the mock response, not the real response)
- Are assertions specific enough to catch regressions?
  (asserting response.status === 200 is not enough —
  what about the response body?)
- Are there assertions that can never fail because they
  test the same hardcoded value both sides?
  (expect('hello').toBe('hello'))

### E.4 SKIPPED & DISABLED TESTS

For every skipped, pending, or disabled test:
- Why was it skipped? (check comments)
- How long has it been skipped?
- Is the feature it tests still active?
- Does the skip have a tracking issue or TODO reference?
- Should it be fixed and re-enabled or deleted?

Flag any skipped test covering a Critical security control
as Critical — a skipped security test is an untested
security control.

### E.5 MOCK INTEGRITY

For every mock, stub, or fake in the test suite:
- Does the mock accurately represent the real thing?
- If the real implementation changed, was the mock updated?
- Is the mock so comprehensive it tests the mock
  instead of the real behavior?
- Are there mocks for functions that no longer exist?
- Are there functions that should be mocked but aren't
  (causing tests to hit real external services)?

### E.6 TEST ISOLATION INTEGRITY

For every test:
- Does it depend on another test having run first?
  (test ordering dependency — flag as High)
- Does it leave state behind that affects other tests?
  (shared database state, global variable mutation)
- Does it depend on a specific execution order?
- Can it run in isolation and still pass?

### E.7 SECURITY TEST INTEGRITY

Specifically for security tests (from the cybersecurity
audit or otherwise):
- Does every security test have a meaningful assertion?
- Does every security test actually call the real
  endpoint (not a mocked version)?
- Are rate limit tests actually hitting the rate limit
  or just checking the header?
- Are injection tests actually sending the payload to
  the real input handler?
- Are auth tests actually using invalid tokens against
  real endpoints?
- Are there security tests that always pass regardless
  of the security control being broken?
- Are any security tests skipped?

### E.8 TEST NAMING INTEGRITY

For every test case:
- Does the name clearly describe what is being tested?
- Does the name describe the expected behavior
  (not just "test login" but "login returns 401 when
  password is incorrect")?
- Are test names consistent in format across the suite?
- Are there duplicate test names that could cause
  reporting confusion?

════════════════════════════════════════════════════════════════════════
DOMAIN F — DOCUMENTATION INTEGRITY
════════════════════════════════════════════════════════════════════════

### F.1 README INTEGRITY

For every README file in the project:
- Does every referenced file path still exist?
- Does every referenced command still work?
- Does every referenced environment variable still exist?
- Does every referenced feature still exist?
- Does it describe the current architecture accurately?
- Are installation instructions current and correct?
- Are there setup steps that are missing?
- Are there setup steps for features that were removed?
- Are badge links (build status, coverage, version) live?
- Is the tech stack description accurate?

### F.2 OPENAPI / API SPEC INTEGRITY

For every API specification file (OpenAPI, Swagger,
GraphQL schema, RAML, etc.):

SPEC VS REALITY:
- Every route in the spec — does it exist in the code?
- Every route in the code — is it in the spec?
- Every request schema in the spec — does it match
  the actual validation in the handler?
- Every response schema in the spec — does it match
  what the handler actually returns?
- Every error code documented — is it actually returned?
- Every auth requirement in the spec — is it enforced
  in the code?
- Every parameter in the spec — does it match the
  handler's parameter names exactly?
- Every enum value in the spec — does it match the
  actual enum in the code?

SPEC QUALITY:
- Are there endpoints with no description?
- Are there parameters with no description?
- Are there response schemas that are too generic
  (just 'object' or 'any')?
- Are there missing example values?
- Are there deprecated endpoints not marked as deprecated?

### F.3 INLINE COMMENT INTEGRITY

Scan all inline comments and documentation comments:
- Comments that describe what the code USED to do
  (code changed but comment wasn't updated — lying comment)
- Comments that reference variables, functions, or
  files that no longer exist
- TODO comments — catalog every one with its location:
  - How long has it been there?
  - Is the referenced feature still planned?
  - Should it be a tracked issue instead?
- FIXME comments — catalog every one:
  - Is the bug it references still present?
  - Has it been fixed but the comment not removed?
- HACK/WORKAROUND comments — catalog every one:
  - Is the underlying issue still present?
  - Is the hack still needed?
- Commented-out code blocks — catalog every one:
  - Is this code actually needed and just temporarily disabled?
  - Or is it dead and should be deleted?
  - Flag: do not delete commented-out code without
    confirming it is not needed

### F.4 CHANGELOG & VERSION INTEGRITY

If a CHANGELOG, HISTORY, or release notes file exists:
- Is it current?
- Does it reference versions that match package.json
  or equivalent?
- Are there unreleased changes documented?

### F.5 ENVIRONMENT DOCUMENTATION INTEGRITY

For every .env.example or environment documentation:
- Does it list every environment variable the app uses?
- Are there variables in the code not in the example file?
- Are there variables in the example file no longer
  used in the code?
- Are descriptions accurate for every variable?
- Are example values safe (no real secrets as examples)?

════════════════════════════════════════════════════════════════════════
DOMAIN G — CODE INTEGRITY (FULL AUDIT)
════════════════════════════════════════════════════════════════════════

### G.1 STALE & DEAD CODE

DEAD FUNCTIONS:
- Every function defined but never called from any
  active code path
- Every function only called by other dead functions
- Every exported function with no consumers anywhere
- Every callback assigned but never triggered
- Every event handler registered for an event never fired
- Every async function never awaited or called

DEAD VARIABLES:
- Every variable declared but never read
- Every variable assigned multiple times where earlier
  values are never read (overwritten before use)
- Every constant defined but never referenced
- Every function parameter never used inside the function
- Every destructured variable never used after destructuring
- Every loop variable never used inside the loop body
- Every catch block variable (err) never examined

DEAD IMPORTS:
- Every import statement where the imported item is
  never used in that file
- Every wildcard import where only a fraction is used
- Every type import never used in a type annotation

DEAD EXPORTS:
- Every exported item that has zero consumers anywhere
  in the codebase

UNREACHABLE CODE:
- Every line after a return, throw, break, or continue
- Every else branch of a condition that is always true
- Every if branch of a condition that is always false
- Every case in a switch that can never be reached
- Every ternary branch that can never execute
- Every loop that can never iterate

### G.2 DUPLICATE CODE

EXACT DUPLICATES:
- Every function body duplicated in more than one file
- Every block of logic copy-pasted across files
- Every constant defined with the same value in
  multiple places
- Every type or interface defined identically in
  multiple files

NEAR DUPLICATES:
- Functions that do the same thing with minor differences
  that could be parameterized into one function
- Components or classes that are structurally identical
  with different names
- Validation logic duplicated across multiple endpoints
  that should be a shared validator
- Error handling patterns duplicated across files
  that should be centralized

### G.3 CODE SMELL AUDIT

MAGIC VALUES:
- Every magic number (numeric literal with no named constant)
- Every magic string (string literal repeated or unexplained)
- Every hardcoded URL, path, or identifier
- Every hardcoded timeout or retry value
- Every hardcoded limit or threshold

DEBUG CODE IN PRODUCTION:
- Every console.log, console.error, console.warn
  left in production code
- Every print, puts, echo, println statement
- Every debugger statement
- Every breakpoint comment
- Every TODO that disables production behavior

FEATURE FLAGS FROZEN IN PLACE:
- Every feature flag that is always true (permanently on)
- Every feature flag that is always false (permanently off)
- Every feature flag condition that can never change
  at runtime
- Feature flag dead code: the branch that is
  permanently disabled

ENVIRONMENT CODE LEAKAGE:
- Development-only code paths reachable in production
- Test-only setup code reachable in production builds
- Logging or debugging enabled in production config
- Mock implementations reachable in production

OVERSIZED FUNCTIONS:
- Every function over 50 lines — flag for review
- Every function over 100 lines — flag as must refactor
- Every function with more than 4 parameters
- Every function with more than 3 levels of nesting
- Every function that does more than one thing

GOD OBJECTS:
- Every class or module that imports more than
  15 other modules
- Every class with more than 20 methods
- Every file over 500 lines
- Every service that orchestrates more than 5 other services

INCONSISTENT PATTERNS:
- Error handling done differently in different parts
  of the codebase
- Async/await mixed inconsistently with callbacks
  or raw promises
- Different naming conventions in different modules
- Different response formats from different endpoints

### G.4 DEPENDENCY INTEGRITY

INSTALLED BUT UNUSED:
- Every package in the manifest never imported anywhere
- Every package imported only in dead files
- Every package installed for a feature that was removed

USED BUT NOT INSTALLED:
- Every import referencing a package not in the manifest
  (relying on a transitive dependency — fragile)

WRONG DEPENDENCY TYPE:
- Every package in dependencies only used in test files
  (should be devDependencies)
- Every package in devDependencies used in production
  source files (should be dependencies)

DUPLICATE PACKAGES:
- Multiple packages doing the same thing
  (two date libraries, two HTTP clients, two loggers,
  two validation libraries)
- The same package installed under two different names
- Forked packages that have since been superseded

OUTDATED PACKAGES:
- Every package more than 2 major versions behind
- Every package with a known CVE in the installed version
- Every package that is officially deprecated or abandoned
- Every package with a recommended replacement

LOCK FILE INTEGRITY:
- Is the lock file (package-lock.json, yarn.lock,
  pnpm-lock.yaml, Gemfile.lock, poetry.lock) committed?
- Is it consistent with the manifest file?
- Are there packages in the lock file not in the manifest?
- Are there integrity hash mismatches?

PEER DEPENDENCY WARNINGS:
- List every unresolved peer dependency warning
- Flag any that could cause runtime incompatibility

### G.5 CONFIGURATION INTEGRITY

CONFLICTING CONFIGURATIONS:
- Config files for the same tool that contradict each other
  (.eslintrc AND eslint.config.js simultaneously)
- TypeScript configs that conflict across tsconfig files
- Prettier and ESLint rules that contradict each other
  (ESLint enforces single quotes, Prettier enforces double)
- Babel and TypeScript configs with conflicting targets
- Multiple package.json files in a monorepo with
  conflicting dependency versions

MISSING CONFIGURATION:
- Linting configured but not run in CI pipeline
- Formatting configured but not enforced in CI
- TypeScript strict mode not enabled
- Missing .gitignore entries for generated files
- Missing .editorconfig for consistent formatting

ENVIRONMENT CONFIG INTEGRITY:
- Every environment config file
  (.env.development, .env.staging, .env.production)
  — does it have all the keys that other environments have?
- Are there keys in production config missing from
  development config (developer won't know they're needed)?
- Are there development-only keys accidentally in
  production config?
- Are default values safe for production?

### G.6 DATABASE & SCHEMA INTEGRITY

MIGRATION INTEGRITY:
- Every model/entity — does it have a corresponding
  migration that creates its table?
- Every migration — does it reference tables and columns
  that still exist in the model?
- Are migrations in the correct order?
- Are there gaps in migration history?
- Is the model snapshot consistent with the
  migration history?

SCHEMA VS MODEL DRIFT:
- Every column in every migration — does it exist
  in the corresponding model/entity?
- Every property in every model — does it have a
  corresponding column in a migration?
- Every relationship in the model — is there a
  foreign key in the migrations?
- Every index in the model — is it in the migrations?
- Every constraint in the model — is it in the migrations?

SEED DATA INTEGRITY:
- Every seed record — does it reference valid
  foreign key values?
- Are seeds in the correct insertion order?
- Are there seed records for tables that no longer exist?
- Are there hardcoded IDs in seed data that conflict
  with auto-increment behavior?

════════════════════════════════════════════════════════════════════════
MASTER OUTPUT FORMAT
════════════════════════════════════════════════════════════════════════

For every finding output:

**Finding #[N]**
- 📍 Location: [Every file involved]
- 🏷️ Domain: [A/B/C/D/E/F/G — which domain]
- 🏷️ Category: [Specific sub-category]
- 🔴 Severity:
  CRITICAL — causes runtime errors, data loss, security
             gaps, or broken functionality right now
  HIGH — causes incorrect behavior, broken builds,
         or significant maintainability risk
  MEDIUM — causes confusion, technical debt, or
           potential future breakage
  LOW — style, consistency, or nice-to-have cleanup
- 📝 Description: [Exactly what the problem is and
  why it matters to the health of the codebase]
- 🔗 Full Impact: [Every other file or system affected]
- ⚠️ Safe to Remove: [Yes / No / Needs Verification]
  If No or Needs Verification — explain exactly why
  and what to check before acting
- ✅ Action: [Exact recommended action with specifics:
  delete, merge with X, extract to Y, replace with Z,
  add missing item, fix reference, update config]
- 💻 Code: [Exact code change where applicable —
  ready to apply directly]

════════════════════════════════════════════════════════════════════════
SECTION 2 — FULL CORRECTED CODEBASE OUTPUT
════════════════════════════════════════════════════════════════════════

After all findings, produce the complete corrected
version of every file that was touched:

- Every import cleaned up
- Every dead export removed
- Every circular dependency resolved
- Every route correctly wired
- Every dead route removed
- Every middleware correctly ordered
- Every type corrected
- Every dead type removed
- Every test assertion verified
- Every broken test fixed or flagged
- Every doc comment updated to match reality
- Every magic value replaced with a named constant
- Every duplicate consolidated
- Every dead code block removed
- Every config conflict resolved

Every output file must be production-ready and
copy-paste directly back into the project.

════════════════════════════════════════════════════════════════════════
SECTION 3 — CORRECTED FILE STRUCTURE
════════════════════════════════════════════════════════════════════════

Show the complete recommended project structure
as a directory tree after all changes:

project-root/
├── src/
│   ├── [corrected structure]
│   └── ...
├── tests/
├── config/
└── ...

Include:
- Before vs after comparison
- Every file deleted (with reason)
- Every file moved (with new path)
- Every file split (with new file names)
- Every file merged (with destination)
- Updated .gitignore covering everything it should

════════════════════════════════════════════════════════════════════════
SECTION 4 — MASTER INTEGRITY REPORT
════════════════════════════════════════════════════════════════════════

### DOMAIN HEALTH SCORES (1-10):
- A: Dependency Graph Health: [score]
- B: Route Integrity: [score]
- C: Build & Bundle Integrity: [score]
- D: Type System Integrity: [score]
- E: Test Integrity: [score]
- F: Documentation Integrity: [score]
- G: Code Integrity: [score]
- OVERALL CODEBASE INTEGRITY SCORE: [score]

### FINDINGS SUMMARY:
- Critical: [N]
- High: [N]
- Medium: [N]
- Low: [N]
- Total: [N]

### CODEBASE METRICS:
Before cleanup:
- Total files: [N]
- Total lines of code: [N]
- Dead code lines: [N] ([N]% of codebase)
- Duplicate code blocks: [N]
- Circular dependencies: [N]
- Broken routes: [N]
- Dead routes: [N]
- Unused packages: [N]
- Failing/broken tests: [N]
- Tests with no assertions: [N]
- Skipped tests: [N]
- Outdated doc references: [N]
- Config conflicts: [N]
- Schema drift issues: [N]

After cleanup:
- Total files: [N] ([N] removed, [N] merged, [N] split)
- Total lines of code: [N] ([N]% reduction)
- Dead code lines: 0
- Circular dependencies: 0
- Broken routes: 0
- Dead routes: 0

### MOST CRITICAL FILES:
Files with the highest concentration of problems:
[Ranked list of worst offenders with issue count]

### COMPLETE ROUTE MAP (FINAL):
After all fixes, show the complete route map:
[METHOD] [PATH] | Auth | Rate Limit | Middleware | Handler
For every route — clean, correct, and complete.

### DEPENDENCY HEALTH REPORT:
- Packages to remove: [list]
- Packages to move to devDependencies: [list]
- Packages to move to dependencies: [list]
- Duplicate packages to consolidate: [list]
- Packages to update (CVE or critical): [list]
- Packages to replace with better alternative: [list]

### TEST INTEGRITY REPORT:
- Total tests: [N]
- Tests with no assertions: [N] — list them
- Skipped tests: [N] — list them
- Dead test files: [N] — list them
- Broken mocks: [N] — list them
- Security tests verified trustworthy: [N]/[N]
- Overall test trustworthiness score: [1-10]

### DOCUMENTATION INTEGRITY REPORT:
- README files audited: [N]
- Broken README references: [N]
- OpenAPI spec drift: [N] mismatches
- Lying comments found: [N]
- TODO comments catalogued: [N]
- FIXME comments catalogued: [N]
- Commented-out code blocks: [N]

### ⚡ PRIORITY ACTION ORDER:
The exact order to safely apply all fixes:

1. Fix all Critical broken routes and handlers
   (application may not function without these)
2. Resolve all Critical circular dependencies
   (may affect startup and module loading)
3. Remove all secrets and sensitive files from
   version control and update .gitignore
4. Fix all broken test assertions and dead test files
   (restore confidence in the test suite)
5. Fix all config conflicts
6. Fix all schema and migration drift
7. Remove all confirmed dead files
8. Remove all unused package dependencies
9. Fix all broken import paths
10. Resolve all remaining circular dependencies
11. Remove all unused imports across all files
12. Remove all unused exports
13. Consolidate all duplicate code
14. Replace all magic numbers and strings with constants
15. Remove all debug/console statements
16. Fix all dead routes
17. Correct all middleware ordering issues
18. Fix all type system violations
19. Update all outdated documentation
20. Fix all medium severity findings
21. Address all low severity findings
22. Run full test suite to verify nothing broken
23. Run full build to verify clean compilation
24. Verify overall integrity score improved to 9+/10

### 🔐 SECURITY INTEGRITY SIGN-OFF:
Confirm that every change made is security-safe:
- ✅ No security controls removed or weakened
- ✅ No auth middleware accidentally removed from routes
- ✅ No rate limiting accidentally removed
- ✅ No sensitive data accidentally exposed
- ✅ No security tests accidentally deleted
- ✅ All security tests still passing after cleanup
- ✅ .gitignore prevents all sensitive files from
     being committed
- ✅ No secrets left in any tracked file

### POST-CLEANUP VERIFICATION COMMANDS:
Provide the exact commands to run after all changes
are applied to verify the codebase is healthy,
adapted to the detected stack:

Examples (adapt to actual stack):
- Run full test suite: [command]
- Run linter: [command]
- Run type checker: [command]
- Run build: [command]
- Check for unused exports: [command]
- Check for circular dependencies: [command]
- Audit dependencies: [command]
- Verify no dead imports remain: [command]
```

---

## When to Use This File

**Full overhaul of an existing project:**
```
Read the entire codebase and run the full integrity
audit across all 7 domains. Produce the complete
findings report and corrected file structure.
```

**Before a major release:**
```
Run the full integrity audit and confirm the overall
integrity score is 9/10 or higher before we ship.
```

**After a long sprint of new feature development:**
```
Run the integrity audit focusing on Domains B (routes),
G (code integrity), and E (test integrity) to catch
anything that slipped through during fast development.
```

**Targeted route audit only:**
```
Run Domain B only — produce the complete route map
and flag every broken, dead, or misconfigured route.
```

**Targeted dependency audit only:**
```
Run Domain G.4 only — full dependency integrity audit.
Show me everything to remove, move, or replace.
```

---

## How All Four Files Work Together

```
apibuildingblueprint.md
"Build every API perfectly from line one"
              ↓
cybersecurityaudit.md
"Verify security is airtight + generate security tests"
              ↓
fileintegrityblueprint.md  ← This file
"Verify the entire codebase is clean, connected,
 trustworthy, and has zero structural debt"
              ↓
Repeat on every project, every major release,
after every significant architectural change
```

---

## Domain Quick Reference

| Domain | What It Covers |
|---|---|
| **A** | Circular deps, import consistency, barrel files, tree-shaking |
| **B** | Route registration, handlers, middleware, dead routes, auth map |
| **C** | Build artifacts in git, bundle size, source maps, build scripts |
| **D** | Types, interfaces, enums, generics, type config strictness |
| **E** | Test coverage, dead tests, assertions, mocks, security tests |
| **F** | README accuracy, OpenAPI drift, lying comments, env docs |
| **G** | Dead code, duplicates, magic values, debug code, dependencies, schema drift |

---

*Covers: Dependency graph analysis, circular dependency chains,
import path consistency, barrel file health, tree-shaking compatibility,
route registration integrity, middleware ordering, dead routes, route
auth map, build artifact contamination, bundle size, type system
integrity, enum integrity, test coverage, assertion integrity, skipped
tests, mock integrity, security test trustworthiness, README accuracy,
OpenAPI spec drift, lying comments, TODO/FIXME catalog, dead code
elimination, duplicate code consolidation, magic value replacement,
debug code removal, dependency manifest integrity, configuration
conflicts, database schema drift, and migration integrity —
for any stack, any language, any framework, any project.*
