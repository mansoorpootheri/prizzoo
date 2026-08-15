# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Prizzoo is a public, crowdsourced price-comparison platform: shoppers search a
product and see it ranked by price across nearby physical stores. It is built
on top of **EnterpriseBase**, an ASP.NET Boilerplate (ABP v10, framework
`Abp.*` NuGet packages, not the newer ABP Framework/`Volo.Abp.*`) multi-tenant
ERP template that was stripped down and repurposed. All namespaces, assembly
names, and the solution file (`EnterpriseBase.sln`) still say
`EnterpriseBase` — that is not a bug, it's the underlying template name.

The single most important document in this repo is
[PrizzooServer/DOCS/PRIZZOO_MIGRATION_NOTES.md](PrizzooServer/DOCS/PRIZZOO_MIGRATION_NOTES.md)
(duplicated at `PrizzooServer/src/EnterpriseBase.Web.Host/PRIZZOO_MIGRATION_NOTES.md`).
It explains exactly what was deleted from the original template, what was
rebuilt for Prizzoo's domain, and a prioritized list of known gaps/likely
compile errors. **Read it before making structural changes** — a lot of
"why does this look inconsistent" questions are answered there. That file was
written by a Claude session that restructured the template **without a
compiler in the loop** (no .NET SDK / no network access at the time), so the
first thing to do in a fresh session is `dotnet build` and fix whatever the
compiler surfaces, per the notes.

## Product context (PrizzooServer/DOCS/)

The rest of `DOCS/` (business plan, flow diagrams, UI mockups) isn't code but
explains *why* the domain model looks the way it does and how far the current
backend is from the full product vision:

- **`Prizzoo_Business_Plan_India.docx`** — the product roadmap this codebase
  is an early slice of. Key points that explain current code decisions: MVP
  scope is **one Tier-2 city, one category** (mobiles/electronics or
  pharmacy — not metro grocery, to avoid competing with quick-commerce apps
  like Blinkit/Zepto); shoppers must never be charged (comparison stays
  free); identity is phone-number/OTP based, not email/password (see "Known
  gaps" #4 in the migration notes — not built yet); retailer price updates
  come in via **WhatsApp Business, not a web dashboard** (most kirana owners
  won't log into a portal — this is why `PriceSource.RetailerReported` /
  `Crowdsourced` exist as separate enum values on `Price`); ONDC integration
  and PostGIS-based geo queries are explicitly deferred to later phases, not
  MVP. The roadmap (Phase 1–4) is far ahead of what's implemented — most of
  it (ratings/reviews, price-history charts, barcode scan, alerts, retailer
  analytics dashboard, monetisation/ads, ONDC feeds) has **no backend code
  yet**. Don't assume a feature exists just because it's in the business
  plan.
- **`prizzoo_dfd_shopper_side.png`** / **`prizzoo_dfd_shop_owner_side.png`**
  — the two core data flows the app is built around: shopper searches → app
  compares nearby prices → shopper picks the cheapest/closest store → walks
  in and buys (maps to `PriceCompareAppService`); shop owner sends a
  WhatsApp price update → gets checked → goes live for shoppers (maps to
  `PriceSubmissionAppService` + moderation, though the WhatsApp intake step
  itself isn't built — see business plan note above).
- **`HOME-Black-Mode.pdf`** / **`LOGIN.pdf`** — mobile UI mockups (brand name
  styled "PriZzoO.com", tagline "Compare. Locate. Save"). `LOGIN.pdf` is the
  OTP phone-login screen described above. `HOME-Black-Mode.pdf` shows a much
  richer home feed than the backend currently supports — city selector,
  retailer logo strip, category tiles, videos, offers/events tabs, "top
  picks" carousels. Treat these as target UI, not a spec of what the API
  currently returns; the `Application` layer today only has
  Pricing/Stores/Dashboard services, with nothing yet for videos, offers, or
  events.

## Commands

All commands run from `PrizzooServer/` (the solution root — `EnterpriseBase.sln`).

```bash
# Restore + build the whole solution
dotnet restore
dotnet build

# Run the API host (Kestrel, http://0.0.0.0:5001 per appsettings.json)
dotnet run --project src/EnterpriseBase.Web.Host

# Run all tests
dotnet test

# Run a single test project
dotnet test test/EnterpriseBase.Tests
dotnet test test/EnterpriseBase.Web.Tests

# Run a single test by name (xunit)
dotnet test test/EnterpriseBase.Tests --filter "FullyQualifiedName~PriceCompareAppService"

# EF Core migrations (run from the EntityFrameworkCore project)
cd src/EnterpriseBase.EntityFrameworkCore
dotnet ef migrations add <Name> -s ../EnterpriseBase.Web.Host
dotnet ef database update -s ../EnterpriseBase.Web.Host
```

There is currently no `Migrations/` folder — the old one was deleted as part
of the domain rewrite and hasn't been regenerated yet (see migration notes,
"Known gaps" #1). Don't hand-write a migration against the deleted history;
generate a fresh initial migration once the solution builds cleanly.

Docker: `PrizzooServer/src/EnterpriseBase.Web.Host/Dockerfile` builds/publishes
the host image directly (`dotnet publish` against `EnterpriseBase.Web.Host`).
`docker/mvc/docker-compose.yml` and `build/build-*.sh|ps1` reference an older
`EnterpriseBase.Web.Mvc` project that no longer exists in this solution —
treat those as stale/template leftovers, not working scripts.

## Architecture

Layered ABP solution, dependencies flow top-to-bottom:

- **EnterpriseBase.Core** — domain entities, enums, domain services,
  cross-cutting concerns (Authorization, MultiTenancy, Localization,
  Features, Settings). No EF Core or web dependencies.
- **EnterpriseBase.Application** — application services (`*AppService`
  classes), DTOs, `EnterpriseBaseApplicationModule.cs`. This is where
  request/response shaping and authorization attributes live.
- **EnterpriseBase.EntityFrameworkCore** — `EnterpriseBaseDbContext`,
  `EnterpriseBaseDbContextConfigurer` (Npgsql/PostgreSQL — **not** SQL
  Server, despite the SqlServer package still being referenced alongside
  Npgsql in this project's own `.csproj`), repositories, seed data.
- **EnterpriseBase.Web.Core** — shared web infra (auth, controllers, models)
  used by hosting layers.
- **EnterpriseBase.Web.Host** — the actual runnable ASP.NET Core app:
  `Startup/Program.cs`, `Startup/Startup.cs`,
  `Startup/EnterpriseBaseWebHostModule.cs`, navigation/menu/permission
  wiring (`EnterpriseBaseNavigationProvider.cs`, `PageNames.cs`). ABP
  auto-generates the dynamic Web API from `Application` services — there's
  no need to hand-write controllers for CRUD app services.
- **EnterpriseBase.Migrator** — standalone console app for running DB
  migrations outside the web host (multi-tenant migration runner).
- **test/EnterpriseBase.Tests** — xunit + `Abp.TestBase`, runs against EF
  Core InMemory provider (see `EnterpriseBaseTestBase.cs`), seeds a host +
  default tenant + tenant admin user in the constructor of every test class.
- **test/EnterpriseBase.Web.Tests** — thin web/controller-level tests.

### Domain model: public catalog, not multi-tenant

This is the key architectural deviation from the stock ABP template, and the
source of most "why is this not tenant-scoped" questions:

- `Store`, `Product`, `Category`, `Unit`, `Price`, `StoreChain` are
  deliberately **not** `IMustHaveTenant`/`IMayHaveTenant`. Prizzoo is one
  shared public price index, not a per-tenant ERP — every shopper sees every
  store's data regardless of who administers it. `Store.TenantId` exists as
  a plain nullable int reserved for a future Phase 3 white-label mode; adding
  `IMayHaveTenant` to it now would make ABP's tenant filter silently hide
  host-created stores from tenant-scoped queries, so don't do that without
  re-reading the reasoning in `Store.cs`'s doc comment first.
- `Price` is **append-only** (one row per product/store/observation, never
  updated in place) so that price-history charts work later without a
  separate audit table. Only `Status == Approved` rows should ever be
  surfaced to shoppers.
- `PriceCompareAppService` (`Application/Pricing/PriceCompareAppService.cs`)
  is the actual shopper-facing search/compare endpoint: `[AllowAnonymous]`,
  bounding-box pre-filter in SQL + exact Haversine distance in C#. This is
  explicitly an MVP shortcut that won't scale past one city — see the
  doc comment for the Postgres `earthdistance`/PostGIS upgrade path before
  touching its filtering logic.
- `PriceSubmissionAppService` is the authenticated counterpart (crowdsourced
  submission + moderation). It's currently gated with a bare `[AbpAuthorize]`
  (any logged-in user can moderate) as a known placeholder — see migration
  notes "Known gaps" #3 before assuming moderation is properly locked down.
- Multi-tenancy (`Authorization`, `Identity`, `MultiTenancy` modules) is kept
  and functional for host/tenant admin accounts, but is orthogonal to the
  public catalog — don't assume `AbpSession.TenantId` scoping applies to
  `Store`/`Product`/`Price` queries the way it would in the original
  ERP template.

### Modules deleted from the original template

Sales, Purchases, Inventory, VanManagement, RouteManagement, Vouchers,
Accounting, CreditNotes, Tours, FinancialYears, Invoicing, and the old
`Parties` module. If you find a dangling reference to any of these (menu
items, permission constants, stale DTOs), it's expected leftover debris —
see migration notes "Known gaps" #2 for the specific files likely to still
reference them (`EnterpriseBaseNavigationProvider.cs`,
`EnterpriseBaseAuthorizationProvider.cs`, `PermissionNames.cs`/`PageNames.cs`).
