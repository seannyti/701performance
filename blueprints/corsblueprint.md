# 🔒 CORS Blueprint — Universal CORS Standard
> Drop this file in your `.blueprints/` folder. Reference it every time you
> configure CORS on any API, server, or framework regardless of stack.
> Claude will follow these rules as the law for every project it touches.

---

## How to Use This File

**Before configuring CORS on any project:**
```
Read all files in my .blueprints folder before doing anything.
They are your standards for this project. Then configure CORS
according to corsblueprint.md for [describe your stack/endpoints].
```

**To audit existing CORS on a project:**
```
Read all files in my .blueprints folder. Then audit the existing
CORS configuration in this project against corsblueprint.md and
give me a gap report with everything that is missing or misconfigured.
```

---

## What CORS Actually Is (Claude Must Understand This Fully)

CORS (Cross-Origin Resource Sharing) is a browser security mechanism that
controls which origins (protocol + domain + port) are allowed to make
cross-origin requests to your API. It does NOT protect server-to-server
communication — only browser-initiated requests. A misconfigured CORS
policy is not just a browser error — it is a security vulnerability that
can expose your API to unauthorized data access, credential theft, and
cross-site attacks.

**An origin is the exact combination of:**
- Protocol: `https://` vs `http://`
- Domain: `example.com` vs `api.example.com`
- Port: `:3000` vs `:443` vs (implicit)

`https://app.example.com` and `https://api.example.com` are DIFFERENT origins.
`http://example.com` and `https://example.com` are DIFFERENT origins.
`https://example.com` and `https://example.com:8080` are DIFFERENT origins.

---

## The CORS Request Flow Claude Must Always Implement Correctly

### Simple Requests
Triggered automatically by the browser for GET, HEAD, POST with safe content
types. No preflight. Server must still return correct `Access-Control-Allow-Origin`.

### Preflighted Requests
Any request with:
- Methods: PUT, DELETE, PATCH, or OPTIONS
- Custom headers (Authorization, Content-Type: application/json, X-*)
- Credentials (cookies, HTTP auth)

The browser sends an OPTIONS preflight FIRST. Your server MUST handle
OPTIONS on every route and respond with the correct CORS headers before
the actual request is sent. If the preflight fails, the browser never
sends the real request.

### Credentialed Requests
Any request that includes cookies, HTTP authentication, or TLS certificates.
Requires both:
- `Access-Control-Allow-Credentials: true` on the server
- `credentials: 'include'` on the fetch call
- `Access-Control-Allow-Origin` MUST be an explicit origin — NEVER a wildcard

---

## CORS Headers — Complete Reference

### Response Headers (Server → Browser)

| Header | Purpose | Rules |
|---|---|---|
| `Access-Control-Allow-Origin` | Which origin(s) can access | Single origin, `*`, or null. Never `*` with credentials |
| `Access-Control-Allow-Methods` | Allowed HTTP methods | Explicit list. Never blindly echo request |
| `Access-Control-Allow-Headers` | Allowed request headers | Explicit list. Never blindly echo request |
| `Access-Control-Allow-Credentials` | Allow cookies/auth | `true` only — omit entirely if false |
| `Access-Control-Expose-Headers` | Headers browser JS can read | Only expose what the client genuinely needs |
| `Access-Control-Max-Age` | Preflight cache duration | Set this. Reduces preflight requests significantly |

### Request Headers (Browser → Server, Read-Only for Reference)

| Header | What It Contains |
|---|---|
| `Origin` | The requesting origin — always validate this |
| `Access-Control-Request-Method` | The method the browser wants to use |
| `Access-Control-Request-Headers` | The headers the browser wants to send |

---

## Zero-Tolerance Security Rules

Claude must enforce ALL of the following without exception.

### Rule 1 — Never Use Wildcard With Credentials
```
// ❌ CRITICAL VULNERABILITY — never do this
Access-Control-Allow-Origin: *
Access-Control-Allow-Credentials: true

// Browsers block this but some misconfigured servers still set it.
// It completely defeats authentication security.
```

### Rule 2 — Never Reflect Origin Without Validation
```javascript
// ❌ DANGEROUS — reflects any origin blindly
const origin = req.headers.origin;
res.setHeader('Access-Control-Allow-Origin', origin); // DO NOT DO THIS

// ✅ CORRECT — validate against allowlist first
const allowedOrigins = ['https://app.example.com', 'https://admin.example.com'];
const origin = req.headers.origin;
if (allowedOrigins.includes(origin)) {
  res.setHeader('Access-Control-Allow-Origin', origin);
  res.setHeader('Vary', 'Origin'); // Required when dynamically setting origin
}
```

### Rule 3 — Never Use Regex With Wildcards on Origin
```javascript
// ❌ VULNERABLE — attacker registers "evilexample.com" and bypasses this
if (/example\.com/.test(req.headers.origin)) { ... }

// ❌ VULNERABLE — "https://notexample.com" matches this
if (/.*example\.com/.test(req.headers.origin)) { ... }

// ✅ CORRECT — exact string match against an explicit allowlist only
const allowedOrigins = new Set([
  'https://app.example.com',
  'https://www.example.com'
]);
if (allowedOrigins.has(req.headers.origin)) { ... }
```

### Rule 4 — Always Set Vary: Origin
When dynamically setting `Access-Control-Allow-Origin` based on the
request origin, always include `Vary: Origin` in the response. Without
this, CDNs and proxies cache the first origin's response and serve it
to all other origins, causing both broken CORS and security leakage.

```
Vary: Origin
```

### Rule 5 — Never Allow `null` Origin in Production
The `null` origin is sent by:
- Sandboxed iframes
- Local file:// requests
- Certain redirects

Allowing `null` in production gives attackers a trivial bypass vector.
```javascript
// ❌ NEVER whitelist null in production
if (origin === 'null' || origin === null) {
  res.setHeader('Access-Control-Allow-Origin', 'null'); // DO NOT DO THIS
}
```

### Rule 6 — Allowlist Must Be Explicit and Maintained
No wildcards in subdomains. No regex. No dynamic construction from
untrusted input. The allowlist is a hardcoded, reviewed, version-controlled
array of exact origin strings.

```javascript
// ✅ The only acceptable pattern
const ALLOWED_ORIGINS = [
  'https://app.yourdomain.com',
  'https://www.yourdomain.com',
  'https://admin.yourdomain.com',
  // Add origins here explicitly and deliberately
];
```

### Rule 7 — Handle OPTIONS Preflight on Every Route
Every route that accepts non-simple methods or custom headers MUST
respond to OPTIONS with 204 and the correct CORS headers. A missing
OPTIONS handler causes all credentialed and non-simple requests to fail
silently in production.

### Rule 8 — Never Expose Unnecessary Headers
`Access-Control-Expose-Headers` makes response headers readable by
browser JavaScript. Only expose what the client actively needs.
Never use a wildcard here in credentialed contexts.

```
// ❌ Exposing everything
Access-Control-Expose-Headers: *

// ✅ Expose only what the client needs
Access-Control-Expose-Headers: X-Request-ID, X-Rate-Limit-Remaining
```

### Rule 9 — Separate Dev and Production Configs
Development CORS settings (localhost, http://) must NEVER reach
production. Use environment variables to switch configs. Fail closed —
if the environment is unknown, apply the most restrictive policy.

### Rule 10 — Log and Alert on Origin Violations
Every request with an origin not on the allowlist should be logged
with the origin, IP, and endpoint. Repeated violations from the same
origin should trigger alerts. This is your early warning system for
misconfigured clients and active probing attacks.

---

## Universal CORS Implementation Template

### Node.js / Express
```javascript
import cors from 'cors';

// Load from environment — never hardcode
const ALLOWED_ORIGINS = process.env.ALLOWED_ORIGINS
  ? process.env.ALLOWED_ORIGINS.split(',').map(o => o.trim())
  : [];

const corsOptions = {
  origin: (origin, callback) => {
    // Allow requests with no origin (server-to-server, curl, Postman)
    if (!origin) return callback(null, true);

    if (ALLOWED_ORIGINS.includes(origin)) {
      callback(null, true);
    } else {
      // Log the violation before rejecting
      console.warn(`[CORS] Blocked origin: ${origin}`);
      callback(new Error(`CORS policy violation: origin ${origin} not allowed`));
    }
  },
  methods: ['GET', 'POST', 'PUT', 'PATCH', 'DELETE', 'OPTIONS'],
  allowedHeaders: [
    'Content-Type',
    'Authorization',
    'X-Request-ID',
    'X-API-Key',
  ],
  exposedHeaders: [
    'X-Request-ID',
    'X-Rate-Limit-Limit',
    'X-Rate-Limit-Remaining',
    'X-Rate-Limit-Reset',
  ],
  credentials: true,           // Only if cookies/auth are needed
  maxAge: 86400,               // 24 hours preflight cache
  optionsSuccessStatus: 204,   // Some legacy browsers choke on 200 for OPTIONS
};

// Apply globally before all routes
app.use(cors(corsOptions));

// Handle preflight on all routes explicitly
app.options('*', cors(corsOptions));
```

### Environment Variables (Required in Every Project)
```env
# .env.development
ALLOWED_ORIGINS=http://localhost:3000,http://localhost:5173,http://localhost:4200

# .env.production
ALLOWED_ORIGINS=https://app.yourdomain.com,https://www.yourdomain.com

# Never put localhost in production ALLOWED_ORIGINS
# Never put * in ALLOWED_ORIGINS
```

### Fastify
```javascript
await fastify.register(import('@fastify/cors'), {
  origin: (origin, callback) => {
    if (!origin || ALLOWED_ORIGINS.includes(origin)) {
      callback(null, true);
    } else {
      console.warn(`[CORS] Blocked: ${origin}`);
      callback(new Error('Not allowed by CORS'));
    }
  },
  methods: ['GET', 'POST', 'PUT', 'PATCH', 'DELETE', 'OPTIONS'],
  allowedHeaders: ['Content-Type', 'Authorization', 'X-Request-ID'],
  exposedHeaders: ['X-Request-ID', 'X-Rate-Limit-Remaining'],
  credentials: true,
  maxAge: 86400,
  preflight: true,
  strictPreflight: true, // Reject preflights that don't match — enforce this
});
```

### Hono (Edge / Cloudflare Workers)
```javascript
import { cors } from 'hono/cors';

app.use('*', cors({
  origin: (origin) => {
    if (!origin || ALLOWED_ORIGINS.includes(origin)) return origin;
    console.warn(`[CORS] Blocked: ${origin}`);
    return null;
  },
  allowMethods: ['GET', 'POST', 'PUT', 'PATCH', 'DELETE', 'OPTIONS'],
  allowHeaders: ['Content-Type', 'Authorization', 'X-Request-ID'],
  exposeHeaders: ['X-Request-ID', 'X-Rate-Limit-Remaining'],
  credentials: true,
  maxAge: 86400,
}));
```

### Koa
```javascript
import cors from '@koa/cors';

app.use(cors({
  origin: (ctx) => {
    const origin = ctx.request.headers.origin;
    if (!origin || ALLOWED_ORIGINS.includes(origin)) return origin;
    console.warn(`[CORS] Blocked: ${origin}`);
    return false;
  },
  allowMethods: ['GET', 'POST', 'PUT', 'PATCH', 'DELETE', 'OPTIONS'],
  allowHeaders: ['Content-Type', 'Authorization', 'X-Request-ID'],
  exposeHeaders: ['X-Request-ID', 'X-Rate-Limit-Remaining'],
  credentials: true,
  maxAge: 86400,
}));
```

### Raw Node.js (No Framework)
```javascript
function setCORSHeaders(req, res) {
  const origin = req.headers.origin;

  if (origin && ALLOWED_ORIGINS.includes(origin)) {
    res.setHeader('Access-Control-Allow-Origin', origin);
    res.setHeader('Vary', 'Origin'); // Always required when dynamic
  }

  res.setHeader('Access-Control-Allow-Methods', 'GET, POST, PUT, PATCH, DELETE, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization, X-Request-ID');
  res.setHeader('Access-Control-Expose-Headers', 'X-Request-ID, X-Rate-Limit-Remaining');
  res.setHeader('Access-Control-Allow-Credentials', 'true');
  res.setHeader('Access-Control-Max-Age', '86400');
}

// In your request handler
if (req.method === 'OPTIONS') {
  setCORSHeaders(req, res);
  res.writeHead(204);
  res.end();
  return;
}

setCORSHeaders(req, res);
// Continue handling request...
```

### Python / FastAPI
```python
from fastapi.middleware.cors import CORSMiddleware
import os

ALLOWED_ORIGINS = [o.strip() for o in os.getenv("ALLOWED_ORIGINS", "").split(",") if o.strip()]

app.add_middleware(
    CORSMiddleware,
    allow_origins=ALLOWED_ORIGINS,         # Never use ["*"] with credentials
    allow_credentials=True,
    allow_methods=["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"],
    allow_headers=["Content-Type", "Authorization", "X-Request-ID"],
    expose_headers=["X-Request-ID", "X-Rate-Limit-Remaining"],
    max_age=86400,
)
```

### Python / Django
```python
# settings.py
CORS_ALLOWED_ORIGINS = [
    "https://app.yourdomain.com",
    "https://www.yourdomain.com",
]

CORS_ALLOW_CREDENTIALS = True
CORS_ALLOW_ALL_ORIGINS = False   # Never True in production

CORS_ALLOW_METHODS = [
    "DELETE", "GET", "OPTIONS", "PATCH", "POST", "PUT",
]

CORS_ALLOW_HEADERS = [
    "accept", "authorization", "content-type",
    "x-request-id", "x-api-key",
]

CORS_EXPOSE_HEADERS = [
    "x-request-id", "x-rate-limit-remaining",
]

CORS_PREFLIGHT_MAX_AGE = 86400
```

### Go / Gin
```go
import "github.com/gin-contrib/cors"

config := cors.Config{
    AllowOriginFunc: func(origin string) bool {
        for _, allowed := range allowedOrigins {
            if allowed == origin {
                return true
            }
        }
        log.Printf("[CORS] Blocked origin: %s", origin)
        return false
    },
    AllowMethods:     []string{"GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"},
    AllowHeaders:     []string{"Content-Type", "Authorization", "X-Request-ID"},
    ExposeHeaders:    []string{"X-Request-ID", "X-Rate-Limit-Remaining"},
    AllowCredentials: true,
    MaxAge:           86400 * time.Second,
}

router.Use(cors.New(config))
```

---

## CORS for Public APIs (No Auth / No Cookies)

If your API is genuinely public and does not use cookies or credentials:

```javascript
// Acceptable ONLY for fully public, unauthenticated APIs
const corsOptions = {
  origin: '*',
  methods: ['GET', 'OPTIONS'],
  allowedHeaders: ['Content-Type'],
  credentials: false,  // Must be false when origin is *
  maxAge: 86400,
};
```

Even then, scope it tightly — only the endpoints that need to be public.
Never apply wildcard CORS globally across a server that also has
authenticated endpoints.

---

## CORS for Multi-Tenant / Dynamic Subdomain Apps

If tenants get subdomains like `tenant1.app.com`, `tenant2.app.com`:

```javascript
const BASE_DOMAIN = process.env.BASE_DOMAIN; // 'app.com'

const corsOptions = {
  origin: (origin, callback) => {
    if (!origin) return callback(null, true);

    try {
      const url = new URL(origin);
      const hostname = url.hostname;

      // Must be HTTPS in production
      if (url.protocol !== 'https:' && process.env.NODE_ENV === 'production') {
        console.warn(`[CORS] Non-HTTPS origin blocked: ${origin}`);
        return callback(new Error('HTTPS required'));
      }

      // Must end with exact base domain — prevents evilapp.com bypass
      if (hostname === BASE_DOMAIN || hostname.endsWith(`.${BASE_DOMAIN}`)) {
        return callback(null, true);
      }

      console.warn(`[CORS] Subdomain not under base domain: ${origin}`);
      callback(new Error('Origin not allowed'));
    } catch {
      callback(new Error('Invalid origin'));
    }
  },
  credentials: true,
  maxAge: 86400,
};
```

---

## CORS + Authentication Header Matrix

| Scenario | Allow-Origin | Allow-Credentials | Notes |
|---|---|---|---|
| Public API, no auth | `*` | omit/false | Wildcard safe here only |
| JWT in Authorization header | Explicit origin | false | Auth header isn't a credential |
| JWT in HttpOnly cookie | Explicit origin | true | Cookie IS a credential |
| API key in header | Explicit origin | false | Header isn't a credential |
| Session cookie | Explicit origin | true | Cookie IS a credential |
| Mixed (JWT header + CSRF cookie) | Explicit origin | true | Cookie present = credentials |

---

## Preflight Response Requirements

Every preflight OPTIONS response MUST include ALL of these:

```
HTTP/1.1 204 No Content
Access-Control-Allow-Origin: https://app.yourdomain.com
Access-Control-Allow-Methods: GET, POST, PUT, PATCH, DELETE, OPTIONS
Access-Control-Allow-Headers: Content-Type, Authorization, X-Request-ID
Access-Control-Allow-Credentials: true
Access-Control-Max-Age: 86400
Vary: Origin
```

Missing ANY of these causes the preflight to fail and the browser
blocks the actual request entirely.

---

## CORS Middleware Order — Critical

CORS middleware MUST be registered before:
- Authentication middleware
- Rate limiting middleware
- Any route handlers
- Body parsers

If auth middleware runs first and rejects the preflight OPTIONS request
with a 401, the browser never gets the CORS headers and your API appears
completely broken to the frontend — even though it works server-to-server.

```javascript
// ✅ CORRECT ORDER
app.use(cors(corsOptions));       // 1. CORS first — always
app.use(helmet());                // 2. Security headers
app.use(rateLimiter);             // 3. Rate limiting
app.use(express.json());          // 4. Body parsing
app.use(authenticate);            // 5. Auth
app.use('/api', routes);          // 6. Routes last
```

---

## Nginx / Reverse Proxy CORS

If you handle CORS at the proxy level instead of the application:

```nginx
# Only set CORS headers once — at the app OR at the proxy, NEVER both
# Duplicate headers cause browsers to reject the response

map $http_origin $cors_origin {
    default                         "";
    "https://app.yourdomain.com"    "https://app.yourdomain.com";
    "https://www.yourdomain.com"    "https://www.yourdomain.com";
}

server {
    location /api/ {
        # Handle preflight
        if ($request_method = 'OPTIONS') {
            add_header 'Access-Control-Allow-Origin' $cors_origin always;
            add_header 'Access-Control-Allow-Methods' 'GET, POST, PUT, PATCH, DELETE, OPTIONS' always;
            add_header 'Access-Control-Allow-Headers' 'Content-Type, Authorization, X-Request-ID' always;
            add_header 'Access-Control-Allow-Credentials' 'true' always;
            add_header 'Access-Control-Max-Age' '86400' always;
            add_header 'Vary' 'Origin' always;
            return 204;
        }

        # Add to all other responses
        add_header 'Access-Control-Allow-Origin' $cors_origin always;
        add_header 'Access-Control-Allow-Credentials' 'true' always;
        add_header 'Vary' 'Origin' always;

        proxy_pass http://your_backend;
    }
}
```

---

## Docker / Local Dev Gotchas

When running frontend and backend in Docker with different ports:

```env
# docker-compose.env or .env.development
ALLOWED_ORIGINS=http://localhost:3000,http://localhost:5173,http://127.0.0.1:3000
```

Common mistakes in Docker dev setups:
- Frontend running on `localhost:3000` but API configured for `localhost:8080` only
- Using `host.docker.internal` in the browser but allowlist has `localhost`
- HTTP in dev but HTTPS in prod — both forms need separate allowlist entries for dev
- Vite proxy configured but CORS still set on the API — pick one, not both

---

## CORS Audit Checklist

Run this against every project before shipping:

### Configuration
- [ ] Allowlist is an explicit array of exact origin strings
- [ ] No wildcard `*` used on authenticated or credentialed endpoints
- [ ] No regex-based origin matching
- [ ] No blind origin reflection without validation
- [ ] `null` origin is NOT on the allowlist
- [ ] Dev/prod configs are separated via environment variables
- [ ] `Vary: Origin` is set on all dynamic origin responses

### Headers
- [ ] `Access-Control-Allow-Origin` — exact origin or controlled wildcard
- [ ] `Access-Control-Allow-Methods` — only methods actually used
- [ ] `Access-Control-Allow-Headers` — only headers actually sent
- [ ] `Access-Control-Allow-Credentials` — true only when cookies/auth used
- [ ] `Access-Control-Expose-Headers` — only what the client needs
- [ ] `Access-Control-Max-Age` — set to reduce preflight overhead
- [ ] No duplicate CORS headers from both app and proxy

### Preflight
- [ ] OPTIONS handler registered on all non-simple routes
- [ ] OPTIONS returns 204 with full CORS headers
- [ ] Auth middleware does NOT block OPTIONS requests
- [ ] CORS middleware registered before auth middleware

### Security
- [ ] No `*` + `credentials: true` combination anywhere
- [ ] No `null` origin allowed in production
- [ ] Origin violations are logged with origin, IP, and endpoint
- [ ] CORS config reviewed when new subdomains or apps are added
- [ ] No localhost or dev origins present in production config

### Testing
- [ ] Test from an allowed origin — request succeeds
- [ ] Test from a blocked origin — request fails with CORS error
- [ ] Test preflight OPTIONS — returns 204 with correct headers
- [ ] Test credentialed request — cookies sent and received correctly
- [ ] Test with no origin header (server-to-server) — request succeeds
- [ ] Test that removing a token still blocks access (CORS ≠ auth)

---

## Common CORS Errors and What They Actually Mean

| Browser Error | Actual Cause | Fix |
|---|---|---|
| "No 'Access-Control-Allow-Origin' header" | Origin not in allowlist, or OPTIONS blocked by auth | Check allowlist, move CORS before auth middleware |
| "The value of 'Access-Control-Allow-Origin' must not be '*' when credentials flag is true" | Wildcard + credentials | Use explicit origin |
| "Request header not allowed" | Custom header not in Allow-Headers | Add header to allowedHeaders list |
| "Method not allowed" | Method not in Allow-Methods | Add method to allowedMethods list |
| Preflight returns 401 | Auth middleware blocking OPTIONS | Exempt OPTIONS from auth or move CORS first |
| Works in dev, broken in prod | localhost in prod allowlist missing | Check environment-specific configs |
| Works in Postman, broken in browser | CORS only applies to browsers | CORS config is wrong — Postman bypasses it |
| Random breakage behind CDN | Missing Vary: Origin header | Add Vary: Origin to all CORS responses |

---

## CORS Is Not a Security Boundary for Your API

**Claude must understand and enforce this distinction:**

CORS only controls which browser origins can make requests. It does NOT:
- Prevent server-to-server requests (curl, Postman, other backends)
- Replace authentication or authorization
- Protect against server-side attacks
- Prevent a determined attacker from bypassing it

A CORS policy of `*` does not mean your API is open to the world for
server-side callers — it was already open to them. CORS only gates
browser-initiated requests.

Your authentication (JWT, API key, session) is the actual security layer.
CORS is a browser-level access control on top of that. Both are required.
Neither replaces the other.

---

## Definition of Done — CORS

Claude must not consider CORS configured until ALL of the following are true:

- [ ] Allowlist is explicit, environment-variable-driven, and reviewed
- [ ] No security anti-patterns present (wildcard+credentials, null origin, blind reflection)
- [ ] Preflight OPTIONS handled correctly on all non-simple routes
- [ ] CORS middleware registered before auth and rate limiting
- [ ] `Vary: Origin` present on all dynamic origin responses
- [ ] Dev and production configs are strictly separated
- [ ] Origin violations logged to the application log
- [ ] All checklist items above pass
- [ ] Manually tested from allowed and blocked origins
- [ ] No duplicate CORS headers from proxy + application

If any item above is not checked, CORS is not done.
