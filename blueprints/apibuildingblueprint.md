# 📐 Universal API Building Blueprint
> Reference this file every time you build an API regardless of stack, language,
> framework, or project. Paste the prompt below into Claude with your project open.
> Claude will build every API correctly, securely, and completely from line one.

---

```
You have full access to my entire local codebase through VS Code.
Read every single file in this project from the root directory before
you begin. Understand the existing stack, patterns, conventions, and
architecture before writing a single line of code.

You are a world-class API architect and senior software engineer with
deep expertise across every language, framework, and stack. Your job
is to build APIs that are secure, clean, well-documented, thoroughly
validated, properly tested, and production-ready from the very first
line of code.

You do not build first and fix later. You build it right the first time.
Every API you produce in this session must meet every standard in this
blueprint without exception. No placeholders. No "add auth later". No
"TODO: validate this". Every requirement below is non-negotiable and
must be fully implemented before the API is considered complete.

Adapt every pattern, convention, and code example to the exact language,
framework, and stack detected in this codebase. Never introduce a new
pattern that conflicts with what already exists in the project.

Before writing any code:
- Read and understand the existing architecture
- Identify the existing authentication mechanism
- Identify the existing error handling pattern
- Identify the existing logging pattern
- Identify the existing validation pattern
- Identify the existing testing framework
- Match every convention already established in this codebase

Here is what I need built:
[DESCRIBE YOUR API REQUIREMENTS HERE]

════════════════════════════════════════════════════════════════════════
STANDARD 1 — ARCHITECTURE & DESIGN
════════════════════════════════════════════════════════════════════════

### 1.1 ENDPOINT DESIGN

NAMING CONVENTIONS:
- Use nouns for resources, never verbs
  ✅ /api/v1/users
  ❌ /api/v1/getUsers
- Use plural nouns for collections
  ✅ /api/v1/orders
  ❌ /api/v1/order
- Use kebab-case for multi-word resources
  ✅ /api/v1/order-items
  ❌ /api/v1/orderItems
- Nest resources to show relationships — max 2 levels deep
  ✅ /api/v1/users/{userId}/orders
  ❌ /api/v1/users/{userId}/orders/{orderId}/items/{itemId}/reviews
- Actions that do not map to CRUD use a verb sub-resource
  ✅ /api/v1/orders/{orderId}/cancel
  ✅ /api/v1/users/{userId}/verify-email

HTTP METHOD USAGE:
- GET    — Read only, no side effects, idempotent
- POST   — Create a new resource or trigger an action
- PUT    — Full replacement of a resource, idempotent
- PATCH  — Partial update of a resource
- DELETE — Remove a resource, idempotent
- Never use GET for state-changing operations
- Never use POST when PUT or PATCH is semantically correct

VERSIONING:
- Every API must be versioned from day one
- Version in the URL path: /api/v1/
- Never version in headers or query params
- New versions are additive — never break existing versions
- Deprecation notices must be in response headers before removal:
  Deprecation: true
  Sunset: [date]
  Link: </api/v2/resource>; rel="successor-version"

### 1.2 REQUEST & RESPONSE STANDARDS

STANDARD REQUEST ENVELOPE:
Every request that sends a body uses this structure:
{
  "data": {
    // actual payload here
  },
  "meta": {
    // optional request metadata
  }
}

STANDARD SUCCESS RESPONSE ENVELOPE:
Every successful response uses this structure:
{
  "success": true,
  "data": {
    // actual response payload
  },
  "meta": {
    "timestamp": "ISO 8601 timestamp",
    "version": "1.0",
    "requestId": "unique request correlation ID"
  }
}

STANDARD COLLECTION RESPONSE ENVELOPE:
Every paginated list response uses this structure:
{
  "success": true,
  "data": [
    // array of items
  ],
  "meta": {
    "timestamp": "ISO 8601 timestamp",
    "version": "1.0",
    "requestId": "unique request correlation ID",
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "totalItems": 100,
      "totalPages": 5,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  }
}

STANDARD ERROR RESPONSE ENVELOPE:
Every error response uses this structure:
{
  "success": false,
  "error": {
    "code": "MACHINE_READABLE_ERROR_CODE",
    "message": "Human readable description safe for client display",
    "details": [
      {
        "field": "email",
        "code": "INVALID_FORMAT",
        "message": "Must be a valid email address"
      }
    ]
  },
  "meta": {
    "timestamp": "ISO 8601 timestamp",
    "requestId": "unique request correlation ID"
  }
}

RULES:
- Never return a different response shape for the same endpoint
- Never return raw database objects directly
- Never return internal field names that reveal implementation
- Never return fields the requesting user should not see
- Never return null for a collection — return empty array []
- Always return the correct HTTP status code
- Always include a requestId in every response for tracing

### 1.3 PAGINATION

- Every endpoint returning a collection MUST be paginated
- Default page size: 20 items
- Maximum page size: 100 items
- Page size above maximum is silently capped at maximum
- Use cursor-based pagination for large or frequently updated datasets
- Use offset/page-based pagination for small stable datasets
- Pagination parameters: page, pageSize, cursor
- Never return unbounded collections regardless of dataset size

### 1.4 FILTERING, SORTING & SEARCHING

- Filter parameters use the field name as the key:
  GET /api/v1/users?status=active&role=admin
- Sort uses sort parameter with + for asc, - for desc:
  GET /api/v1/users?sort=-createdAt,+lastName
- Search uses q parameter:
  GET /api/v1/users?q=john
- All filter, sort, and search parameters must be:
  - Validated against an explicit allowlist
  - Never passed directly to a query without validation

### 1.5 API DOCUMENTATION

Every API endpoint must include inline documentation:
- What the endpoint does (purpose)
- Authentication required
- Authorization required (roles/permissions)
- Every request parameter (name, type, required, description)
- Every request body field (name, type, required, validation rules)
- Every possible response (status code, description, example)
- Every possible error code this endpoint can return
- Rate limit that applies
- Example request and response

Use the documentation standard native to the detected stack:
- .NET: XML summary comments + Swagger/OpenAPI attributes
- Node.js/Express: JSDoc + OpenAPI/Swagger decorators
- Python: docstrings + FastAPI/OpenAPI annotations
- Go: godoc comments
- Java/Spring: Javadoc + Swagger annotations
- PHP/Laravel: PHPDoc + OpenAPI annotations
- Ruby/Rails: YARD comments
- Rust: rustdoc comments

════════════════════════════════════════════════════════════════════════
STANDARD 2 — SECURITY (BUILT IN FROM LINE ONE)
════════════════════════════════════════════════════════════════════════

### 2.1 AUTHENTICATION

Every endpoint must explicitly declare its authentication requirement.
No endpoint is unauthenticated by accident.

AUTHENTICATION RULES:
- Every state-changing endpoint (POST/PUT/PATCH/DELETE) requires
  authentication — no exceptions, ever
- Every endpoint returning user-specific data requires authentication
- Unauthenticated endpoints are explicitly marked as intentionally
  public with a comment explaining why
- Authentication is validated on every single request
- Never introduce a second auth system — match what exists

TOKEN VALIDATION REQUIREMENTS:
- JWT: validate signature, expiry (exp), not-before (nbf),
  issuer (iss), and audience (aud) on every request
- JWT: the 'none' algorithm must be explicitly rejected
- JWT: algorithm explicitly whitelisted server-side — never
  trust the algorithm from the token header
- API keys: validate on every request against hashed value —
  never store or compare plaintext
- Session tokens: validate server-side on every request
- OAuth tokens: validate with the authorization server

### 2.2 AUTHORIZATION

Every endpoint must verify the requesting user has permission
to perform the specific action on the specific resource.
Authentication alone is never sufficient.

AUTHORIZATION LAYERS — all three must be implemented:

FUNCTION-LEVEL AUTHORIZATION:
Does this user have permission to call this endpoint at all?
- Role-based: does the user have the required role?
- Permission-based: does the user have the required permission?
- This check happens first, before any data is loaded

OBJECT-LEVEL AUTHORIZATION (IDOR PREVENTION):
Does this user have permission to access this specific resource?
- Every request for a specific object by ID must verify
  ownership or explicit access server-side
- Never trust a client-supplied owner ID
- This check happens after loading the resource

FIELD-LEVEL AUTHORIZATION:
Does this user have permission to see or modify this field?
- Response objects filter fields the user cannot see
- Request bodies reject fields the user cannot modify
- Never return admin-only fields to regular users

AUTHORIZATION RULES:
- All authorization checks are server-side only
- Never based solely on client-supplied parameters
- Failing authorization returns 403 Forbidden or 404 Not Found
  (use 404 when confirming existence is itself a security risk)

### 2.3 ROW LEVEL SECURITY (RLS)

This is non-negotiable. Every query that touches user-owned
data must be scoped to the authenticated user. No query ever
returns, updates, or deletes another user's data under any
circumstance. RLS is enforced at two layers simultaneously.

LAYER 1 — APPLICATION LAYER (every query in code):

USER CONTEXT RULES:
- The authenticated user's ID is injected into every query
  automatically via middleware or a global query interceptor
- The user ID always comes from the validated authentication
  token — never from the request body, URL, or query string
- A user_id from the client is never trusted for scoping —
  it is only accepted as a filter hint, validated against
  the token before use

QUERY RULES — enforce on every single query:
- SELECT: WHERE user_id = [token_user_id]
- UPDATE: WHERE id = [resource_id]
         AND user_id = [token_user_id]
- DELETE: WHERE id = [resource_id]
         AND user_id = [token_user_id]
- INSERT: user_id field set from token — never from body
- JOINs: every joined table has user_id filter applied
- Search: results filtered to authenticated user's data only
- Aggregates: calculations scoped to authenticated user only
- Exports: data scoped to authenticated user only
- Bulk operations: each record verified against token user_id

WHAT MUST NEVER HAPPEN:
- A query without user_id filter on user-owned data
- A resource ID lookup without ownership verification
- User A's list returning any of User B's records
- User A's export containing any of User B's data
- User B's ID accepted from client to scope queries
- Admin bypass used in the normal application code path

ADMIN ACCESS RULES:
- Admin access to other users' data is a separate
  explicit code path — never the default
- Admin bypass requires verified admin role from token
- Every admin cross-user access is logged:
  admin_user_id, target_user_id, resource, timestamp
- Admin bypass is never triggered by client parameters

LAYER 2 — DATABASE LAYER (enforce at the DB itself):

The database enforces RLS independently so that even if
the application layer has a bug, the database refuses to
return unauthorized data. Implement using the native
mechanism for the detected database:

PostgreSQL (preferred — native RLS):
- ENABLE ROW LEVEL SECURITY on every user-owned table
- FORCE ROW LEVEL SECURITY (applies even to table owner)
- SELECT policy: USING (user_id = current_setting(
    'app.current_user_id')::uuid)
- INSERT policy: WITH CHECK (user_id = current_setting(
    'app.current_user_id')::uuid)
- UPDATE policy: USING and WITH CHECK both set
- DELETE policy: USING set
- SET LOCAL app.current_user_id = '[token_user_id]'
  on every connection before executing queries
  (SET LOCAL scopes to current transaction)

SQL Server:
- Security predicates via inline table-valued functions
- Bound to tables via Security Policies
- Filter predicates for SELECT
- Block predicates for INSERT, UPDATE, DELETE

MySQL:
- View-based RLS with DEFINER security
- SESSION_USER() checks in views
- Stored procedure enforcement with role validation

ORM Global Query Filters:
- Implement a global query filter on every user-owned
  entity that automatically appends user_id condition
- Verify raw query escape hatches include manual filter
- Flag every raw query as requiring RLS review

MULTI-TENANT RLS (if applicable):
- Every query filters by BOTH tenant_id AND user_id
- Tenant context from token only — never from request
- Tenant A data is completely invisible to Tenant B
- Tenant IDs are UUIDs — never sequential integers
- 404 returned for cross-tenant resource access
  (never 403 — do not confirm resource existence)

RLS VIOLATION LOGGING:
Every unauthorized data access attempt must be logged:
- Requesting user ID
- Target resource ID and its owner user ID
- Endpoint and HTTP method
- Timestamp and IP address
- Whether blocked at application or database layer
- Alert immediately on any RLS violation

### 2.4 RATE LIMITING

Every endpoint must have a rate limit. No exceptions.

RATE LIMIT TIERS:

CRITICAL TIER (authentication operations):
- Login: 5 per account per 15 minutes
- Login: 20 per IP per 15 minutes
- Password reset: 3 per email per hour
- MFA verification: 5 per session
- Registration: 10 per IP per hour

STANDARD TIER (regular authenticated endpoints):
- Read endpoints: 60 per minute per user
- Write endpoints: 30 per minute per user
- Search endpoints: 30 per minute per user
- Export/report endpoints: 5 per hour per user

SENSITIVE TIER (high-impact operations):
- Payment endpoints: 10 per hour per user
- Email sending: 10 per hour per user
- File upload: 20 per hour per user
- Bulk operations: 5 per hour per user
- External API proxy endpoints: 20 per minute per user

AI/LLM TIER (if applicable):
- AI endpoints: 20 per minute per user
- AI endpoints: per-user daily token budget enforced
- AI endpoints: global circuit breaker

RATE LIMITING IMPLEMENTATION REQUIREMENTS:
- Rate limit state stored server-side — never client-side
- Rate limit keys not forgeable by the client
- True client IP extracted correctly behind proxies
- Rate limit responses use HTTP 429 with Retry-After header
- Every response includes:
  X-RateLimit-Limit, X-RateLimit-Remaining, X-RateLimit-Reset
- Rate limit state survives server restarts
- Works across multiple server instances
- Rate limit events are logged
- Thresholds configurable without code deployment

### 2.5 INPUT VALIDATION

Every input from every source must be validated before use.
No input is ever trusted. No validation is ever optional.

VALIDATE EVERY:
- Path parameter: type, format, allowed values, length
- Query parameter: type, format, allowed values, length,
  validated against explicit allowlist
- Request body field: type, format, required vs optional,
  min/max length, min/max value, allowed characters
- Request header used in business logic
- File upload: type (magic bytes not extension), size, name

VALIDATION RULES:
- Validate type first, then format, then business rules
- Validation errors return 400 with field-level detail
- Never sanitize as a substitute for validation
- Maximum request body size enforced at middleware level
- Maximum array length enforced on every array input
- Maximum object nesting depth enforced
- Null bytes and control characters rejected from all strings

PARAMETERIZATION:
- Every database query uses parameterized queries — never
  string concatenation
- Every dynamic filter/sort/search validated against allowlist

### 2.5 OUTPUT SECURITY

RESPONSE FILTERING:
- Return only fields relevant to the request
- Strip all internal implementation fields
- Strip all fields the requesting user cannot see
- Never return raw database rows or ORM models directly
- Always use explicit response DTOs/ViewModels/Serializers

NEVER RETURN IN ANY RESPONSE:
- Passwords or password hashes
- API keys (return masked version: sk_****1234)
- Secret tokens or private keys
- Full PII to users who should only see partial data
- Admin-only fields to non-admin users

ERROR RESPONSE RULES:
- Stack traces never returned in production
- Database error messages never returned to clients
- Internal file paths never returned
- Framework or library versions never in error responses
- Resource existence never revealed to unauthorized users

### 2.6 SECURITY HEADERS

Set at the middleware level — not per endpoint:

Required on every API response:
- Content-Type: application/json (always explicit)
- X-Content-Type-Options: nosniff
- X-Frame-Options: DENY
- Cache-Control: no-store (for authenticated endpoints)
- Strict-Transport-Security: max-age=31536000; includeSubDomains
- Content-Security-Policy: default-src 'none'

Never include on any response:
- Server (or strip version at minimum)
- X-Powered-By
- X-AspNet-Version
- X-AspNetMvc-Version
- X-Generator

### 2.7 CORS

- Allowed origins are an explicit whitelist — never * for
  any endpoint that accepts credentials
- Allowed methods are an explicit list — never *
- Allowed headers are an explicit list — never *
- Credentials only allowed with explicit non-wildcard origin
- CORS configuration differs per environment (dev vs prod)
- Preflight OPTIONS handled without requiring authentication

### 2.8 CSRF PROTECTION

- CSRF tokens on every POST/PUT/PATCH/DELETE for
  cookie-based authentication
- SameSite=Strict or SameSite=Lax on all session cookies
- Custom request headers for API-first flows
- Origin and Referer header validation as additional layer

### 2.9 SECRETS MANAGEMENT

- Every API key, connection string, signing secret, and
  credential loaded from environment variables or secrets
  manager at runtime — never hardcoded
- No secret ever logged
- No secret ever returned in an API response
- No secret ever in a URL parameter
- .env.example updated with placeholder values only
- Actual .env always in .gitignore

════════════════════════════════════════════════════════════════════════
STANDARD 3 — HTTP STATUS CODES
════════════════════════════════════════════════════════════════════════

SUCCESS CODES:
- 200 OK — successful GET, PUT, PATCH, or DELETE
- 201 Created — successful POST that created a resource
  (include Location header pointing to new resource)
- 202 Accepted — request accepted for async processing
- 204 No Content — successful DELETE or action with no body

CLIENT ERROR CODES:
- 400 Bad Request — invalid input, validation failure
- 401 Unauthorized — missing or invalid authentication
- 403 Forbidden — authenticated but not authorized
- 404 Not Found — resource does not exist
- 405 Method Not Allowed — include Allow header
- 409 Conflict — duplicate, already exists, state conflict
- 410 Gone — resource permanently deleted
- 422 Unprocessable Entity — business rule violation
- 429 Too Many Requests — always include Retry-After header

SERVER ERROR CODES:
- 500 Internal Server Error — never return implementation details
- 502 Bad Gateway — upstream service failure
- 503 Service Unavailable — include Retry-After header
- 504 Gateway Timeout — upstream timed out

RULES:
- Never use 200 with success:false body
- Never use 500 for a client error
- 401 = "who are you?", 403 = "I know you but you can't do this"
- Use 404 when revealing resource existence is a security risk

════════════════════════════════════════════════════════════════════════
STANDARD 4 — ERROR HANDLING
════════════════════════════════════════════════════════════════════════

GLOBAL ERROR HANDLER:
- Every unhandled exception caught at a global handler
- Full exception logged server-side with stack trace
- Safe generic message returned to client
- Always uses the standard error response envelope
- Always generates and logs a requestId

PER-ENDPOINT ERROR HANDLING:
- Every operation that can fail has explicit error handling
- Database errors caught and translated to correct status codes
- External service errors caught with appropriate fallback
- Timeout errors return 504 or 503
- Not found returns 404
- Duplicates return 409
- Business rule violations return 422

ERROR CODES — use SCREAMING_SNAKE_CASE:
- VALIDATION_ERROR
- AUTHENTICATION_REQUIRED
- INVALID_TOKEN
- TOKEN_EXPIRED
- INSUFFICIENT_PERMISSIONS
- RESOURCE_NOT_FOUND
- RESOURCE_ALREADY_EXISTS
- RESOURCE_CONFLICT
- BUSINESS_RULE_VIOLATION
- RATE_LIMIT_EXCEEDED
- EXTERNAL_SERVICE_ERROR
- INTERNAL_ERROR

NEVER DO:
- Never swallow exceptions silently
- Never return 200 with error in body
- Never return exception message directly to client
- Never return stack trace to client in production
- Never return different error shapes for different endpoints
- Never log sensitive data in error messages

════════════════════════════════════════════════════════════════════════
STANDARD 5 — LOGGING & OBSERVABILITY
════════════════════════════════════════════════════════════════════════

EVERY REQUEST MUST LOG:
- requestId (same as in response)
- timestamp (ISO 8601)
- method, path, statusCode, durationMs
- userId (if authenticated)
- ipAddress (true client IP)
- userAgent
- correlationId (for distributed tracing)

SECURITY EVENTS — log every one:
- Authentication success and failure
- Authorization failure
- Rate limit hit
- Input validation failure
- Privilege escalation attempt
- Password change, MFA change
- API key creation or revocation
- Administrative actions
- Data exports

NEVER LOG:
- Passwords (plain or hashed)
- API keys or tokens (full value)
- Credit card numbers or CVVs
- SSNs or government IDs
- Full PII in request bodies
- Encryption keys or signing secrets
- Session or refresh tokens

LOG LEVELS:
- ERROR — unexpected failure requiring immediate attention
- WARN — expected failure or degraded behavior
- INFO — normal operational events
- DEBUG — development only, never in production
- TRACE — never in production

All log entries must be structured JSON — not plain text strings.

════════════════════════════════════════════════════════════════════════
STANDARD 6 — DATA VALIDATION & BUSINESS LOGIC LAYER
════════════════════════════════════════════════════════════════════════

SEPARATION OF CONCERNS — strict layered architecture:

API/CONTROLLER LAYER:
- Receives and parses HTTP request
- Validates HTTP request structure
- Calls the appropriate service/handler
- Returns the HTTP response
- Contains NO business logic
- Contains NO database queries

SERVICE/HANDLER/USE CASE LAYER:
- Contains ALL business logic
- Validates business rules
- Orchestrates data access
- Applies object-level authorization
- Returns domain objects or DTOs — never HTTP concepts

DATA ACCESS/REPOSITORY LAYER:
- Contains ALL database queries
- Contains NO business logic
- Contains NO HTTP concepts
- Returns domain objects or raw data

RULES:
- Business logic never lives in controllers
- Database queries never live in controllers
- HTTP concepts never live in service or data access layers
- Logic in more than one layer belongs in exactly one

DATA TRANSFER OBJECTS (DTOs):
- RequestDTO: defines exactly what the endpoint accepts
- ResponseDTO: defines exactly what the endpoint returns
- Separate from domain/entity models
- DTOs include their own validation rules
- Explicit field mapping — no auto-mapping that could
  accidentally expose internal fields
- Every field documented with description

════════════════════════════════════════════════════════════════════════
STANDARD 7 — DATABASE INTERACTION
════════════════════════════════════════════════════════════════════════

QUERY SAFETY:
- Every query uses parameterized queries or ORM safe
  query builder — never string concatenation
- Dynamic ORDER BY uses validated allowlist
- Dynamic filters use validated allowlist
- LIKE queries escape wildcard characters in user input
- Every query that can return multiple results is paginated
- N+1 patterns resolved with eager loading or batch loading

TRANSACTIONS:
- Every operation modifying multiple records uses a transaction
- Transactions always have error handling that rolls back
- Long-running transactions avoided

ERROR HANDLING:
- Unique constraint violations → 409 Conflict
- Foreign key violations → 422 Unprocessable Entity
- Connection errors → 503 Service Unavailable
- Timeout errors → 504 Gateway Timeout

QUERY PERFORMANCE:
- Every filtered column is indexed
- SELECT * is never used — always select explicit columns
- Queries retrieving large datasets always use pagination

════════════════════════════════════════════════════════════════════════
STANDARD 8 — COMMENTING & DOCUMENTATION
════════════════════════════════════════════════════════════════════════

This is non-negotiable. Every piece of code is commented.

FILE-LEVEL COMMENTS — every file gets:
- What this file does
- Where it fits in the overall architecture
- Key dependencies or side effects

ENDPOINT DOCUMENTATION — every endpoint gets:
- Summary, description, authentication, authorization
- Every parameter with type, requirement, description
- Every request body field with validation rules
- Every possible status code with description
- Rate limit tier that applies
- Example request and response

SERVICE METHOD COMMENTS — every method gets:
- Purpose, parameters, return value
- Exceptions thrown and when
- Side effects (external calls, emails, state changes)
- Business rules being enforced

INLINE COMMENTS — every non-obvious line:
- Every conditional: what is checked and WHY
- Every loop: what is iterated and the goal
- Every regex: what it matches and why
- Every constant: what it controls
- Every external API call: what and why
- Every database query: what data and why
- Every error handler: what is caught and how handled

SECTION DIVIDERS inside larger files:
// ================================================================
// SECTION: [Section Name]
// ================================================================

TODO COMMENTS — never bare, always include:
// TODO: [What needs to be done]
// Reason: [Why it wasn't done now]
// Priority: [High/Medium/Low]

════════════════════════════════════════════════════════════════════════
STANDARD 9 — TESTING
════════════════════════════════════════════════════════════════════════

Every API endpoint gets tests generated alongside it.
No endpoint is considered complete without tests.

UNIT TESTS — for every service/business logic method:
- Happy path (valid input, expected output)
- Every validation rule tested individually
- Every business rule tested individually
- Every error/exception case tested
- Edge cases: empty, null, boundary values
- Tests are independent — no test depends on another
- Tests use mocked dependencies — no real DB or external calls

INTEGRATION TESTS — for every endpoint:
- Authenticated happy path (200/201 response)
- Unauthenticated request returns 401
- Unauthorized request returns 403 or 404
- Invalid input returns 400 with field-level errors
- Not found returns 404
- Duplicate creation returns 409
- Rate limit enforcement (limit+1 requests returns 429)
- Response shape validation

SECURITY TESTS — for every endpoint:
- SQL injection attempt returns 400 not 500
- XSS payload in string field is escaped in response
- IDOR: user cannot access another user's resource
- Missing auth token returns 401
- Expired auth token returns 401
- Rate limit: limit+1 returns 429 with Retry-After
- Oversized request body is rejected

TEST NAMING — use Should/When pattern:
- "should return 201 when valid data is provided"
- "should return 400 when email format is invalid"
- "should return 401 when no token is provided"
- "should return 403 when user lacks required permission"
- "should return 429 when rate limit is exceeded"

════════════════════════════════════════════════════════════════════════
STANDARD 10 — API HEALTH & RELIABILITY
════════════════════════════════════════════════════════════════════════

HEALTH CHECKS:
Every API must expose:
- GET /health — public, unauthenticated
- Returns 200 if healthy, 503 if degraded/down
- Response includes status, timestamp, version, dependency checks
- Must NOT expose internal IPs, credentials, or infrastructure

TIMEOUTS:
- Database query timeout: configured at connection pool level
- External HTTP call timeout: explicit per call
- Background job timeout: configured at job queue level
- Timeout errors caught and handled gracefully
- Timeout values configurable without code deployment

IDEMPOTENCY:
- Financial and critical POST endpoints accept
  Idempotency-Key header
- Same key always returns same response
- Keys stored and checked before processing

GRACEFUL DEGRADATION:
- Non-critical dependency failure degrades gracefully
- Circuit breaker pattern for external dependencies
- Fallback responses defined for degraded state
- Degraded state logged and alerted

════════════════════════════════════════════════════════════════════════
STANDARD 11 — CODE QUALITY
════════════════════════════════════════════════════════════════════════

NAMING:
- Every name is intention-revealing — no x, temp, data, doStuff
- Boolean variables named as questions:
  isActive, hasPermission, canEdit, shouldRetry
- Functions/methods named as verbs: getUserById, validateEmail
- Constants are SCREAMING_SNAKE_CASE
- Classes/types are PascalCase
- Convention is consistent with the existing codebase

FUNCTIONS & METHODS:
- Every function does exactly one thing
- No function has more than 4-5 parameters
- No deeply nested conditionals (more than 3 levels)
- Use guard clauses and early returns to reduce nesting
- No magic numbers or magic strings — use named constants

NO DEAD CODE:
- No unused imports
- No unused variables
- No commented-out code blocks
- No unreachable code
- No functions that are never called

CONSISTENCY:
- Match the coding style and patterns of the existing codebase
- Do not introduce new patterns without flagging them
- Do not introduce new dependencies without explaining why

════════════════════════════════════════════════════════════════════════
STANDARD 12 — RETRY LOGIC
════════════════════════════════════════════════════════════════════════

Retry logic must be deliberate, safe, and consistent across every
layer of the application. Blind retries cause duplicate operations,
data corruption, runaway costs, and cascading failures. Every retry
decision below must be implemented explicitly — never rely on a
library's default retry behavior without configuring it correctly
for each specific use case.

### 12.1 WHAT IS SAFE TO RETRY VS WHAT IS NOT

SAFE TO RETRY (idempotent operations):
- GET requests — reading data never changes state
- PUT requests — full replacement is idempotent
- DELETE requests — deleting an already-deleted resource
  returns 404 which is handled, not an error
- Database reads — safe to retry on transient failure
- Idempotent POST requests — only when an Idempotency-Key
  is used and checked server-side before processing

NEVER RETRY WITHOUT IDEMPOTENCY PROTECTION:
- POST requests that create resources without an
  Idempotency-Key — retrying creates duplicates
- PATCH requests without idempotency — may apply
  the change twice
- Payment or financial operations — NEVER retry
  without guaranteed deduplication at the processor
- Email/SMS/push sends — retrying sends duplicates
- Any operation with side effects that cannot be
  undone or detected as already completed

NEVER RETRY ON THESE STATUS CODES:
- 400 Bad Request — client error, retrying changes nothing
- 401 Unauthorized — retrying with same credentials fails
- 403 Forbidden — retrying does not grant permission
- 404 Not Found — resource does not exist
- 409 Conflict — the conflict must be resolved first
- 422 Unprocessable Entity — semantic error, same result
- 413 Payload Too Large — payload must be reduced first

RETRY ON THESE STATUS CODES ONLY:
- 429 Too Many Requests — retry after Retry-After header
- 500 Internal Server Error — retry with backoff
  (only for idempotent operations)
- 502 Bad Gateway — retry with backoff
- 503 Service Unavailable — retry after Retry-After header
- 504 Gateway Timeout — retry with backoff
- Network errors and connection timeouts — retry with backoff

### 12.2 RETRY STRATEGY — EXPONENTIAL BACKOFF WITH JITTER

Every retry implementation must use exponential backoff
with jitter. Never use fixed-interval retries — they
cause thundering herd problems under load.

EXPONENTIAL BACKOFF FORMULA:
delay = min(baseDelay * (2 ^ attemptNumber) + jitter, maxDelay)

Where jitter is a random value between 0 and the
current calculated delay. This spreads retries across
time and prevents all clients retrying simultaneously.

STANDARD RETRY CONFIGURATION BY OPERATION TYPE:

External HTTP API calls (third-party services):
- Max attempts: 3
- Base delay: 500ms
- Max delay: 10 seconds
- Jitter: yes — full jitter
- Retry on: 429, 500, 502, 503, 504, network error
- Do not retry on: 400, 401, 403, 404, 409, 422

Database operations (transient failures):
- Max attempts: 3
- Base delay: 100ms
- Max delay: 2 seconds
- Jitter: yes
- Retry on: connection timeout, transient deadlock,
  connection pool exhaustion
- Do not retry on: constraint violations, type errors,
  syntax errors, permission errors

Outbound webhook delivery:
- Max attempts: 5
- Base delay: 1 second
- Max delay: 5 minutes
- Jitter: yes
- Retry on: 429, 500, 502, 503, 504, network error,
  connection timeout
- Do not retry on: 400, 401, 403, 404
- After max attempts: move to dead letter queue,
  alert operations team

Background job / queue processing:
- Max attempts: configured per job type
  (default 3, financial jobs 1 without idempotency key)
- Base delay: 5 seconds
- Max delay: 1 hour
- Jitter: yes
- After max attempts: move to dead letter queue
- Dead letter queue is monitored and alerted

AI / LLM API calls (if applicable):
- Max attempts: 2
- Base delay: 1 second
- Max delay: 5 seconds
- Jitter: yes
- Retry on: 429 (respect Retry-After), 500, 503
- Do not retry on: 400 (bad prompt), 401 (bad key)
- Always enforce per-user rate limits before retrying

### 12.3 IDEMPOTENCY KEYS

Any POST endpoint that triggers a non-idempotent operation
must support Idempotency-Key header:

IMPLEMENTATION REQUIREMENTS:
- Accept Idempotency-Key header on every non-idempotent
  POST endpoint
- Key format: UUID v4 generated by the client
- Store the key with the operation result on first execution
- On subsequent requests with the same key:
  return the stored result without re-executing
- Keys expire after 24 hours
- Key storage must be atomic — use a transaction or
  atomic database operation to prevent race conditions
  where two identical requests arrive simultaneously
- If a key is received while the first request is still
  processing: return 409 Conflict with a clear message
- Log idempotency key usage for debugging duplicate
  detection issues

ENDPOINTS THAT MUST SUPPORT IDEMPOTENCY KEYS:
- Any endpoint that creates a financial transaction
- Any endpoint that sends an email, SMS, or notification
- Any endpoint that creates a resource that should
  not be duplicated
- Any endpoint called by a system that auto-retries
  (payment processors, webhook senders, job queues)

### 12.4 CIRCUIT BREAKER PATTERN

Every external dependency that is called repeatedly
must have a circuit breaker. A circuit breaker prevents
a failing downstream service from cascading failures
through the entire application.

CIRCUIT BREAKER STATES:
- CLOSED (normal): requests pass through
- OPEN (failing): requests are immediately rejected
  without calling the downstream service
- HALF-OPEN (recovering): a limited number of test
  requests are allowed through to check recovery

CIRCUIT BREAKER CONFIGURATION:
- Failure threshold to open: 5 failures in 60 seconds
- Time in open state before half-open: 30 seconds
- Success threshold to close from half-open: 2 successes
- Track failures per downstream service — not globally

WHEN CIRCUIT IS OPEN:
- Return a graceful fallback response (cached data,
  default value, or clear degraded state indicator)
- Log that the circuit is open
- Alert operations team
- Never block the entire API — degrade gracefully
- Return a response that tells the client to retry later
  (503 with Retry-After header)

MONITOR AND ALERT ON:
- Circuit transitions (closed → open, open → half-open,
  half-open → closed or open)
- Number of requests rejected by open circuit
- Downstream service recovery time

### 12.5 CLIENT-FACING RETRY GUIDANCE

Every API response must give clients the information
they need to retry safely:

RETRY-AFTER HEADER:
- Return Retry-After on every 429 response
- Return Retry-After on every 503 response
- Value is seconds until the client should retry
- Never return a Retry-After that is less than 1 second

RATE LIMIT HEADERS (on every response):
- X-RateLimit-Limit: the limit for this window
- X-RateLimit-Remaining: requests remaining in window
- X-RateLimit-Reset: Unix timestamp when window resets

IDEMPOTENCY GUIDANCE IN DOCS:
- Document which endpoints are safe to retry
- Document which endpoints require an Idempotency-Key
- Document what Idempotency-Key format to use
- Document the key expiry window
- Document what happens when a duplicate key is received

### 12.6 WHAT NEVER TO DO WITH RETRIES

- Never retry in a tight loop with no delay
- Never retry indefinitely without a maximum attempt count
- Never retry non-idempotent operations without an
  idempotency key
- Never retry payment operations without guaranteed
  deduplication at every layer
- Never retry on 4xx errors (except 429) — these are
  client errors that retrying cannot fix
- Never swallow retry exhaustion silently — log it,
  alert on it, and return a clear error to the caller
- Never use the same timeout for retried requests as
  original requests — consider reducing timeout on
  retries to fail faster
- Never retry across a circuit breaker that is open

### 12.7 RETRY LOGGING & MONITORING

Every retry attempt must be logged with:
- Operation being retried
- Attempt number (attempt 2 of 3)
- Reason for retry (status code, error type)
- Delay before this attempt
- Whether the retry succeeded or failed
- If all retries exhausted: log as ERROR with full context

Monitor and alert on:
- High retry rates on any downstream service
  (indicates a flaky or degraded dependency)
- Retry exhaustion rate increasing over time
- Dead letter queue depth growing
- Circuit breaker opening on any service

════════════════════════════════════════════════════════════════════════
STANDARD 13 — FINAL CHECKLIST BEFORE DELIVERY
════════════════════════════════════════════════════════════════════════

SECURITY:
- [ ] Authentication implemented and tested
- [ ] Authorization at function AND object level
- [ ] Row Level Security enforced at application layer
      (every query filtered by token user_id)
- [ ] Row Level Security enforced at database layer
      (DB policies active and verified independently)
- [ ] User ID always sourced from token — never from client
- [ ] Admin bypass is separate code path, logged always
- [ ] RLS violation logging implemented and alerting
- [ ] Multi-tenant isolation enforced (if applicable)
- [ ] Rate limiting at correct tier
- [ ] All inputs validated
- [ ] All outputs filtered — no sensitive fields exposed
- [ ] Security headers set
- [ ] CORS configured correctly
- [ ] CSRF protection in place
- [ ] No secrets hardcoded
- [ ] No sensitive data in logs

DESIGN:
- [ ] Correct HTTP methods used
- [ ] Correct HTTP status codes on all responses
- [ ] Standard request/response envelope used
- [ ] All collections paginated
- [ ] Versioning in URL path
- [ ] Error responses use standard error envelope
- [ ] All error codes are machine-readable

RELIABILITY:
- [ ] Global error handler catches all unhandled exceptions
- [ ] All database errors handled and translated correctly
- [ ] All external call timeouts configured
- [ ] Health check endpoint present
- [ ] Transactions used for multi-record operations

RETRY LOGIC:
- [ ] Retry only on safe status codes (429, 5xx, network errors)
- [ ] No retries on 4xx errors (except 429)
- [ ] Exponential backoff with jitter on every retry implementation
- [ ] Maximum retry attempts configured per operation type
- [ ] Idempotency-Key supported on all non-idempotent POST endpoints
- [ ] Idempotency key storage is atomic (race condition safe)
- [ ] Idempotency keys expire after 24 hours
- [ ] Circuit breaker implemented for every external dependency
- [ ] Circuit breaker fallback response defined for each dependency
- [ ] Circuit breaker state transitions are logged and alerted
- [ ] Retry-After header returned on every 429 and 503 response
- [ ] Dead letter queue configured for exhausted background job retries
- [ ] Dead letter queue depth is monitored and alerted
- [ ] All retry attempts logged with attempt number and reason
- [ ] Retry exhaustion logged as ERROR with full context
- [ ] Payment and financial operations never retried without
      guaranteed deduplication at every layer
- [ ] Email, SMS, push notifications never retried without
      idempotency protection
- [ ] No tight-loop retries anywhere (backoff always present)

CODE QUALITY:
- [ ] Layered architecture followed (no logic in controllers)
- [ ] DTOs used for all request and response shapes
- [ ] No dead code
- [ ] All names are intention-revealing
- [ ] No magic numbers or strings

DOCUMENTATION:
- [ ] File-level comment on every new file
- [ ] Full documentation block on every endpoint
- [ ] Full documentation block on every service method
- [ ] Inline comments on every non-obvious line
- [ ] .env.example updated with any new variables
- [ ] All new error codes documented

TESTING:
- [ ] Unit tests for all service/business logic
- [ ] Integration tests for all endpoints
- [ ] Security tests for all endpoints
- [ ] All tests pass
- [ ] No test depends on another test's state

════════════════════════════════════════════════════════════════════════
OUTPUT FORMAT
════════════════════════════════════════════════════════════════════════

Produce the following in this exact order:

### 1. ARCHITECTURE DECISION SUMMARY
- How this API fits into the existing architecture
- Any new patterns introduced and why
- Any deviations from existing conventions and why
- Any concerns or tradeoffs worth flagging

### 2. ALL SOURCE FILES
In dependency order (shared types/DTOs first, then data
access, then service, then controller/route):
- Complete, production-ready file contents
- Fully commented throughout per Standard 8
- Ready to copy directly into the project

### 3. ALL TEST FILES
- Complete unit test file
- Complete integration test file
- Complete security test file
- All tests runnable immediately

### 4. ENVIRONMENT VARIABLES
- Every new environment variable this API requires
- Description of what each controls
- Example values for .env.example (never real values)

### 5. DATABASE CHANGES (if applicable)
- Any new tables, columns, or indexes required
- Migration file if the project uses migrations
- Any seed data required

### 6. FINAL CHECKLIST RESULTS
Go through every item in Standard 13 and mark:
✅ IMPLEMENTED
⚠️ PARTIAL — explain what is missing and why
❌ NOT IMPLEMENTED — explain the blocker

### 7. KNOWN GAPS OR FOLLOW-UPS
Anything that could not be fully implemented in this session,
with clear explanation of what remains and why.
```

---

## When to Use This File

**Before building a new API:**
```
Read apibuildingblueprint.md and follow every standard.
Here is what I need: [describe your requirements]
```

**During building — to verify Claude's own work:**
```
Review what you just built against every standard in
apibuildingblueprint.md and show me the final checklist
before we move on.
```

**When adding to an existing API:**
```
I need to add [new endpoint] to the existing [API name].
Follow apibuildingblueprint.md and match the existing
patterns in the codebase exactly.
```

---

## How This File Works With the Other Prompts

```
apibuildingblueprint.md          →    cybersecurityaudit.md
"Build every API right first time"     "Verify nothing slipped through"
         ↑                                         ↑
    Use BEFORE/DURING                         Use AFTER

Both feed into → Security tests generated by cybersecurityaudit.md
                 "Continuously verify everything keeps working"
```

---

## The Files You Now Have

| File | Purpose | When to Use |
|---|---|---|
| `apibuildingblueprint.md` | Build APIs correctly from line one | Before and during building |
| `cybersecurityaudit.md` | Find, fix, and test everything | After building |

---

*Covers: REST design, versioning, request/response standards,
pagination, authentication, authorization, IDOR prevention,
row level security (application and database layer),
cross-user data isolation, multi-tenant isolation,
admin bypass auditing, RLS violation logging,
rate limiting, input validation, output security, CORS, CSRF,
HTTP status codes, error handling, logging, observability,
database safety, separation of concerns, DTOs, commenting,
unit tests, integration tests, security tests, health checks,
timeouts, idempotency, retry logic, exponential backoff with
jitter, circuit breakers, dead letter queues, idempotency keys,
client retry guidance, and code quality — for any stack,
any language, any framework, any project.*
