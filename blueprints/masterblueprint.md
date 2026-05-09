# 🧠 Master Development Blueprint
> This is the only file you need to reference for any project —
> new or existing. It ties together all four blueprints into one
> unified system. Tell Claude to follow this file and it will
> know exactly what standards to apply, what to run, and in what order.

---

## The Four Blueprints This File Governs

| File | Purpose |
|---|---|
| `apibuildingblueprint.md` | Build every API correctly from line one |
| `cybersecurityaudit.md` | Audit, fix, and test all security |
| `fileintegrityblueprint.md` | Verify codebase structure and integrity |
| `playwrightblueprint.md` | Full browser E2E test coverage |

---

## How to Use This File

**Paste this into Claude in VS Code at the start of any session:**

```
You have full access to my entire local codebase through VS Code.
Read every single file in this project from the root directory
before you begin. Do not ask me to paste any code. Crawl everything.

You are a world-class software architect, security engineer, and
quality assurance expert. For every task in this project you must
follow the standards defined in my four blueprint files which are
located in this project. Before doing anything else, read all
four blueprint files now:

1. apibuildingblueprint.md
2. cybersecurityaudit.md
3. fileintegrityblueprint.md
4. playwrightblueprint.md

These four files are your non-negotiable standards for everything
you do in this project. No exceptions. No shortcuts.

Every API you build follows apibuildingblueprint.md.
Every security decision follows cybersecurityaudit.md.
Every structural decision follows fileintegrityblueprint.md.
Every E2E test follows playwrightblueprint.md.

Now here is what I need: [DESCRIBE YOUR TASK]
```

---

## For a NEW Project — Run in This Order:

```
Phase 1 — BUILD
Reference apibuildingblueprint.md for every API built.
Nothing ships without meeting its Definition of Done.

Phase 2 — SECURE
Run cybersecurityaudit.md when the first meaningful
version is complete. Both Phase 1 (audit + fix) and
Phase 2 (generate security tests) must complete fully.

Phase 3 — INTEGRITY
Run fileintegrityblueprint.md after security audit.
Clean structure, zero dead code, all routes verified,
all tests trustworthy.

Phase 4 — E2E
Run playwrightblueprint.md last. Generate the full
browser test suite against the clean, secured codebase.
```

---

## For an EXISTING Project — Run in This Order:

```
Phase 1 — INTEGRITY FIRST
Run fileintegrityblueprint.md before anything else.
Clean the house before inspecting it. Remove dead code,
fix broken routes, resolve circular dependencies.
Auditing messy code wastes time on noise.

Phase 2 — SECURE
Run cybersecurityaudit.md against the now-clean codebase.
Every finding is real — not an artifact of dead code.
Both phases must complete (audit + security test suite).

Phase 3 — E2E
Run playwrightblueprint.md to generate browser tests
against the clean, secured application.

Phase 4 — GOING FORWARD
Use apibuildingblueprint.md for every new API added
from this point on. Build it right the first time.
Never need to clean it up again.
```

---

## Decision Guide — What to Run When:

```
Starting a new project?
→ apibuildingblueprint.md (reference while building)
→ cybersecurityaudit.md (run after first working version)
→ fileintegrityblueprint.md (run after security audit)
→ playwrightblueprint.md (run last)

Taking over an existing project?
→ fileintegrityblueprint.md (clean it first)
→ cybersecurityaudit.md (secure it second)
→ playwrightblueprint.md (test it third)
→ apibuildingblueprint.md (govern all new APIs going forward)

Adding a new API to an existing project?
→ apibuildingblueprint.md (build it right)
→ cybersecurityaudit.md Phase 1 only (verify the new API)
→ playwrightblueprint.md (add E2E tests for new flows)

Before a major release?
→ cybersecurityaudit.md (full audit)
→ fileintegrityblueprint.md (full integrity check)
→ Verify all Playwright tests pass

Codebase feels messy?
→ fileintegrityblueprint.md

Something feels insecure?
→ cybersecurityaudit.md

E2E tests missing or broken?
→ playwrightblueprint.md

Building any new API endpoint?
→ apibuildingblueprint.md — always
```

---

## The Complete System At a Glance:

```
┌─────────────────────────────────────────────────────────┐
│                 MASTER DEVELOPMENT SYSTEM               │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  📐 apibuildingblueprint.md                             │
│  ├── REST design & versioning                           │
│  ├── Authentication & authorization built in            │
│  ├── Rate limiting on every endpoint                    │
│  ├── Input validation & output security                 │
│  ├── Error handling & logging                           │
│  ├── Retry logic & circuit breakers                     │
│  ├── Documentation & OpenAPI spec                       │
│  ├── Unit, integration & security tests                 │
│  └── Definition of Done checklist                       │
│                                                         │
│  🔒 cybersecurityaudit.md                               │
│  ├── PHASE 1: Full security audit (14 domains)          │
│  │   ├── DNS & infrastructure                           │
│  │   ├── HTTP security headers                          │
│  │   ├── Authentication security                        │
│  │   ├── Rate limiting coverage                         │
│  │   ├── Encryption (all private data)                  │
│  │   ├── API exposure & CORS                            │
│  │   ├── Webhook security                               │
│  │   ├── AI/LLM prompt injection                        │
│  │   ├── OWASP Top 10 & API Top 10                      │
│  │   ├── Input validation & XSS                         │
│  │   ├── File upload security                           │
│  │   ├── Error handling & info disclosure               │
│  │   └── Infrastructure & DevOps                        │
│  └── PHASE 2: Automated security test suite             │
│      ├── 14 test modules                                │
│      ├── 800+ security tests                            │
│      ├── CI/CD pipeline integration                     │
│      └── Scheduled daily/weekly scans                   │
│                                                         │
│  🏗️ fileintegrityblueprint.md                           │
│  ├── Domain A: Dependency graph analysis                │
│  ├── Domain B: Route integrity & auth map               │
│  ├── Domain C: Build & bundle integrity                 │
│  ├── Domain D: Type system integrity                    │
│  ├── Domain E: Test integrity & trustworthiness         │
│  ├── Domain F: Documentation integrity                  │
│  └── Domain G: Full code integrity audit                │
│                                                         │
│  🎭 playwrightblueprint.md                              │
│  ├── Module 1: Authentication E2E tests                 │
│  ├── Module 2: Route protection tests                   │
│  ├── Module 3: Critical user journey tests              │
│  ├── Module 4: Form tests                               │
│  ├── Module 5: Security E2E tests                       │
│  ├── Module 6: Accessibility (WCAG) tests               │
│  ├── Module 7: Responsive & cross-browser tests         │
│  ├── Module 8: Error state tests                        │
│  ├── Module 9: Core Web Vitals performance              │
│  └── Module 10: CI/CD integration                       │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

## Coverage This System Provides:

### Security Coverage:
- OWASP Top 10
- OWASP API Security Top 10
- DNS & TLS configuration
- HTTP security headers
- Authentication & session security
- Authorization & IDOR prevention
- Row Level Security — application AND database layer
- Cross-user data isolation (zero tolerance)
- Multi-tenant isolation
- Admin bypass auditing and logging
- RLS violation detection and alerting
- Rate limiting on every endpoint
- Encryption at rest and in transit
- API exposure and endpoint inventory
- Webhook signature verification
- AI/LLM prompt injection defense
- CSRF & XSS prevention
- SQL & command injection prevention
- SSRF prevention
- File upload security
- Information disclosure prevention
- Infrastructure & DevOps security

### Code Quality Coverage:
- RESTful API design standards
- Versioning and backwards compatibility
- Input validation and output filtering
- Error handling and logging standards
- Retry logic and circuit breakers
- Idempotency key implementation
- Documentation and OpenAPI spec
- Naming conventions and code structure
- Dead code elimination
- Duplicate code consolidation
- Magic value replacement
- Circular dependency resolution
- Import/export graph health
- Type system correctness
- Configuration integrity

### Testing Coverage:
- Unit tests (business logic)
- Integration tests (API endpoints)
- Security tests (attack vectors)
- E2E browser tests (user journeys)
- Accessibility tests (WCAG)
- Cross-browser tests
- Responsive layout tests
- Performance tests (Core Web Vitals)
- Error state tests
- Role enforcement tests

### Infrastructure Coverage:
- CI/CD pipeline security gates
- Docker and container security
- Cloud configuration review
- Build artifact management
- Secret management verification
- Dependency vulnerability scanning
- Scheduled automated test runs

---

## Recommended File Locations in Your Project:

```
your-project/
├── .blueprints/
│   ├── masterblueprint.md          ← this file
│   ├── apibuildingblueprint.md
│   ├── cybersecurityaudit.md
│   ├── fileintegrityblueprint.md
│   └── playwrightblueprint.md
├── src/
├── e2e/
├── security-tests/
└── ...
```

Keeping all blueprints in a `.blueprints/` folder at the
project root means Claude can always find and read them,
and they travel with the project into version control.

---

## Re-Run Schedule:

| When | What to Run |
|---|---|
| Every new project | All four in order |
| Every new API endpoint | apibuildingblueprint.md |
| Before every major release | cybersecurityaudit.md + fileintegrityblueprint.md |
| After every significant refactor | fileintegrityblueprint.md |
| After new UI features | playwrightblueprint.md |
| Quarterly | All four in full |
| When something feels wrong | The relevant blueprint |

---

## The One Command to Start Any Session:

```
Read all four files in my .blueprints/ folder before
doing anything. They are your standards for this entire
project. Then [describe your task].
```

That's it. One instruction. Claude reads all four blueprints
and applies the right standard to whatever you need.

---

*This master blueprint governs: API design, security auditing,
automated security testing, code integrity, file structure,
route integrity, dependency health, type system integrity,
test trustworthiness, documentation accuracy, E2E browser
testing, accessibility, cross-browser compatibility, responsive
design, performance, CI/CD integration, and ongoing maintenance
— for any project, any stack, any size, new or existing.*
