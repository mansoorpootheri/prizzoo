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

[PrizzooServer/DOCS/PRIZZOO_MIGRATION_NOTES.md](PrizzooServer/DOCS/PRIZZOO_MIGRATION_NOTES.md)
(duplicated at `PrizzooServer/src/EnterpriseBase.Web.Host/PRIZZOO_MIGRATION_NOTES.md`)
is a point-in-time record of the original template→domain rewrite and is
still useful for "why does this look inconsistent" questions about leftover
template debris, but its "Known gaps" list is now stale in places — OTP
login, the `Pages_PriceModeration` permission, and an initial EF migration
all now exist (see below), where that doc says they don't. Trust the code
over that doc when the two disagree.

## Product context (PrizzooServer/DOCS/)

The rest of `DOCS/` (business plan, flow diagrams, UI mockups) isn't code but
explains *why* the domain model looks the way it does and how far the current
backend is from the full product vision:

- **`Prizzoo_Business_Plan_India.docx`** — the product roadmap this codebase
  is an early slice of. Key points that explain current code decisions: MVP
  scope is **one Tier-2 city, one category** (mobiles/electronics or
  pharmacy — not metro grocery, to avoid competing with quick-commerce apps
  like Blinkit/Zepto); shoppers must never be charged (comparison stays
  free); identity is phone-number/OTP based, not email/password — this part
  of the roadmap **is now built** (see Auth model below), unlike most of the
  rest of the plan. Retailer price updates were meant to come in via
  **WhatsApp Business**, not a web dashboard, because most kirana owners
  won't log into a portal; that intake channel still isn't built, and what
  exists instead is a password-authenticated **shop-owner web dashboard**
  (`app/shop-owner/`) where a host admin-provisioned owner submits prices
  directly — a stopgap, not the WhatsApp flow the plan describes. `Price`'s
  `PriceSource.RetailerReported`/`Crowdsourced` enum values predate this and
  still reflect the original WhatsApp-vs-shopper-submitted distinction. ONDC
  integration and PostGIS-based geo queries are explicitly deferred to later
  phases, not MVP. The roadmap (Phase 1–4) is far ahead of what's
  implemented — most of it (price-history charts, barcode scan, alerts,
  retailer analytics dashboard, monetisation/ads, ONDC feeds) has **no
  backend code yet**; product ratings (see below) are a partial exception, a
  single 1–5 star rating per shopper/product exists. Don't assume a feature
  exists just because it's in the business plan.
- **`prizzoo_dfd_shopper_side.png`** / **`prizzoo_dfd_shop_owner_side.png`**
  — the two core data flows the app is built around: shopper searches → app
  compares nearby prices → shopper picks the cheapest/closest store → walks
  in and buys (maps to `PriceCompareAppService`); shop owner sends a price
  update → gets checked → goes live for shoppers (maps to `ShopOwnerAppService`
  and/or `PriceSubmissionAppService` + moderation — the diagram predates the
  current split between host-provisioned shop owners and anonymous
  crowdsourced submitters, see Auth model below).
- **`HOME-Black-Mode.pdf`** / **`LOGIN.pdf`** — mobile UI mockups (brand name
  styled "PriZzoO.com", tagline "Compare. Locate. Save"). `LOGIN.pdf` shows
  an OTP phone-login screen, which the current `/phone-entry` → `/otp-verify`
  flow implements (see Frontend below), though not pixel-for-pixel.
  `HOME-Black-Mode.pdf` shows a much richer home feed than the backend fully
  supports — dark theme, top nav tabs (Home/Videos/Offers/Retailers/
  Categories/Events), a retailer logo strip, a spotlight banner carousel,
  category icon tiles, and promo banners. The current `/home` page
  (`HomeFeed` component + `GetHomeFeedAsync`) implements the "Top Picks for
  You" style sectioned product feed in a light theme, but has no dark mode,
  no Videos/Offers/Events tabs, and no retailer strip — those have no
  backend support (no Video/Offer/Event entities exist). Treat the mockup as
  target UI, not a spec of what the API currently returns.

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

`Migrations/` now has a real history, starting from `initialcommit` — don't
recreate an initial migration from scratch; add incremental ones on top the
normal EF Core way.

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
  used by hosting layers. `Controllers/JwtIssuingControllerBase.cs` factors
  out JWT creation (`CreateAccessToken`/`CreateJwtClaims`) shared by both
  `TokenAuthController` (password login) and `OtpAuthController` (OTP
  login) — tokens from either path are interchangeable everywhere
  `[AbpAuthorize]` is used.
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

### Auth model: OTP for shoppers, passwords for admin/shop-owner

Two separate, coexisting login paths, both issuing interchangeable JWTs via
`JwtIssuingControllerBase`:

- **Shoppers** log in with phone + OTP, never a password.
  `OtpAuthController.RequestOtp` (→ `Application/Otp/OtpChallengeService`)
  creates an `OtpChallenge` row (`Core/Authorization/Otp/OtpChallenge.cs`,
  hashed code, 5-min expiry, throttled) and sends it via `ISmsSender`; the
  only implementation is `NoopSmsSender`, which just logs the code — **there
  is no real SMS integration**, and `GenerateCode()` currently hardcodes
  `"123456"` instead of a random code (explicit TODO in the file). Anyone
  can log in as any phone number right now — don't treat this as a real
  auth boundary until that's fixed. `VerifyOtp` looks up/creates a `User`
  under the default tenant on first success (phone becomes `UserName`,
  synthesized placeholder email, random unusable password) and grants role
  `Shopper`. Shopper JWTs are also deliberately longer-lived than
  admin/shop-owner ones — 30 days (`AppConsts.ShopperAccessTokenExpiration`)
  vs. 1 day (`AppConsts.AccessTokenExpiration`) — since there's no
  refresh-token flow for shoppers and a shopper shouldn't have to re-verify
  their phone every day.
- **Admin and shop-owner** accounts are password-based via the unchanged
  `TokenAuthController` (`/api/TokenAuth/Authenticate`). There is **no
  self-registration** — a host admin creates a store *and* its owner's login
  together in one call (`StoreAppService.CreateAsync`), returning a
  one-time password (`StoreDto.OwnerTemporaryPassword`) for the admin to
  relay manually. Newly created stores are `IsVerified = true` immediately
  (host admin creating it *is* the verification).

Roles (`Core/Authorization/Roles/StaticRoleNames.cs`): `Host.Admin`,
`Tenant.Admin`, `Tenant.ShopOwner`, `Tenant.Shopper`. **The old "Retailer"
role/module is gone entirely** (`RetailerAppService`/`RetailerDtos.cs`
deleted, `Pages_Retailer` permission removed) — everywhere the domain used
to say "retailer" it now says "shop owner", except the untouched
`PriceSource.RetailerReported` enum value. There's no separate moderator
role: `Pages_PriceModeration` and `Pages_Products_Edit` are granted to the
same tenant `Admin` role that does tenant administration, so one
`admin@Default.com`-style login both administers and moderates.

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
  re-reading the reasoning in `Store.cs`'s doc comment first. `Store` also
  has a nullable `OwnerUserId` linking it to its shop-owner `User` and a
  nullable `LocationId` FK (see MasterData below) as the source of truth for
  where it is, with `Store.City` kept as a denormalized display string.
- `Price` is **append-only** (one row per product/store/observation, never
  updated in place) so that price-history charts work later without a
  separate audit table. Only `Status == Approved` rows should ever be
  surfaced to shoppers. `Price.OriginalAmount` (nullable) holds the
  pre-discount MRP; `PriceCompareAppService` derives a `DiscountPercent`
  from it when set.
- `PriceCompareAppService` (`Application/Pricing/PriceCompareAppService.cs`)
  is the shopper-facing search/compare/home-feed service: bounding-box
  pre-filter in SQL + exact Haversine distance in C#. It is now gated
  `[AbpAuthorize(PermissionNames.Pages_Shopper)]` — its own doc comment
  calls this out as a **deliberate reversal** from an earlier
  `[AllowAnonymous]` version, i.e. browsing now requires an OTP-verified
  shopper login. This is explicitly an MVP shortcut that won't scale past
  one city — see the doc comment for the Postgres `earthdistance`/PostGIS
  upgrade path before touching its filtering logic.
- `PriceSubmissionAppService` is the crowdsourced-submission/moderation
  service: any authenticated user can `SubmitAsync` a price
  (`Source=Crowdsourced`); `GetPendingAsync`/`ModerateAsync` require
  `Pages_PriceModeration`. Rejecting a price triggers
  `INotifyShopOwnerService.NotifyPriceRejectedAsync` when the store has an
  owner (only impl is `NoopNotifyShopOwnerService`, which just logs — no
  real WhatsApp/push integration yet).
- `ShopOwnerAppService` (`Application/ShopOwner/`) is the separate,
  pre-verified path for a store's own owner: resolves "my store" via
  `Store.OwnerUserId == AbpSession.UserId`, so an owner can never act on
  someone else's store. `CreateMyProductAsync` goes live immediately with no
  moderation (the owner is already verified), but
  `SubmitPriceForMyStoreAsync` still creates a `Pending` price
  (`Source=RetailerReported`) that goes through the same moderation queue as
  crowdsourced submissions.
- `Application/MasterData/Locations/` (`Location` entity,
  `Core/MasterData/Location.cs`) is a locality *within* a `District` (e.g.
  "Feroke" within "Kozhikode"), with optional lat/lng — **distinct from**
  the pre-existing, unrelated `Core/Geography`/`Application/Geography`
  Country→State→District CRUD module; don't conflate the two.
  `DefaultGeographyCreator.cs` seeds India/Kerala/all 14 Kerala districts as
  fixed MVP constants (no admin UI for country/state); `District` plays the
  "city" role in store-creation forms, and `Location` narrows within it.
  Host-only CRUD via `LocationAppService`, gated `Pages_Locations`.
- `Application/Ratings/ProductRatingAppService.cs` (`Core/Pricing/
  ProductRating.cs`) is a simple product-scoped (not store/price-scoped)
  1–5 star rating, one per shopper per product (unique index on
  `ProductId`+`ShopperUserId`, rating again overwrites). `Application/
  Account/MyAccountAppService.ChangeMyPasswordAsync` is a deliberately
  separate self-service password-change endpoint from
  `UserAppService.ChangePassword`, since the latter requires `Pages_Users`
  and would 403 for ShopOwner/Shopper roles.
- Multi-tenancy (`Authorization`, `Identity`, `MultiTenancy` modules) is kept
  and functional for host/tenant admin accounts, but is orthogonal to the
  public catalog — don't assume `AbpSession.TenantId` scoping applies to
  `Store`/`Product`/`Price` queries the way it would in the original
  ERP template.

### Modules deleted from the original template

Sales, Purchases, Inventory, VanManagement, RouteManagement, Vouchers,
Accounting, CreditNotes, Tours, FinancialYears, Invoicing, the old `Parties`
module, and (more recently) `Retailer` (replaced by `ShopOwner`, see Auth
model above). `EnterpriseBaseNavigationProvider.cs`,
`EnterpriseBaseAuthorizationProvider.cs`, and `PermissionNames.cs` have since
been cleaned of references to the deleted batch, but
`Web.Host/Startup/PageNames.cs` still has stale `const string` entries for
`Accounting`/`Parties`/`FinancialYears`/`Vouchers`/`Invoicing` — dead weight,
not a compile risk, safe to ignore or prune. Treat the migration notes doc's
"Known gaps" #2 file list as stale on this point; trust a grep over the doc.

## Frontend (prizzoo-web/)

A Next.js (App Router) + TypeScript web client that talks to the ABP host
above. **Before writing any frontend code, read `prizzoo-web/AGENTS.md`**
(also aliased as `prizzoo-web/CLAUDE.md`) — this project pins a Next.js
version (16.3.1) newer than most training data, with breaking API/convention
changes, and instructs reading `prizzoo-web/node_modules/next/dist/docs/`
first. Don't assume familiar Next.js patterns still apply without checking.

```bash
cd prizzoo-web
npm run dev     # Next.js dev server, http://localhost:3000
npm run build
npm run lint     # eslint (flat config, eslint-config-next core-web-vitals + typescript)
```

Env vars (see `.env.example`): `NEXT_PUBLIC_API_BASE_URL` (defaults to
`http://localhost:5001`, the ABP host above), `NEXT_PUBLIC_DEFAULT_LATITUDE`/
`NEXT_PUBLIC_DEFAULT_LONGITUDE` (fallback coords when geolocation is
denied/unavailable — currently `0,0` placeholders, TODO in the repo to set to
the MVP launch city). `NEXT_PUBLIC_DEFAULT_TENANT_ID` (`lib/api/config.ts`'s
`DEFAULT_TENANT_ID`) is now dead code — it existed for self-registration,
which has been removed in favor of OTP login; nothing in the codebase
references it anymore.

### Structure

Three separate flows, each with its own auth path:

- **Shopper** (OTP login, no registration step): `app/page.tsx` (splash,
  auto-redirects) → `app/phone-entry/` (`PhoneEntryForm`) → `app/otp-verify/`
  (`OtpVerifyForm`) → `app/home/` (search + `HomeFeed` sectioned product
  feed) → `app/product/[keyword]/` (`ProductHeader` incl. `StarRatingInput`
  + `ResultsList`).
- **Admin** (password login via `app/login/`, shared `LoginForm`):
  `app/admin/dashboard/`, `app/admin/stores/` (list/new/edit — creating a
  store also provisions its owner's login),
  `app/admin/moderation/prices/` (approve/flag/reject pending submissions),
  `app/admin/categories/`, `app/admin/locations/`,
  `app/admin/change-password/`.
- **Shop owner** (same password login as admin, role-routed):
  `app/shop-owner/dashboard/` (own store + own products/prices),
  `app/shop-owner/products/new/` (create product + submit its first price),
  `app/shop-owner/change-password/`.

`components/` has one subfolder per route area (`home/`, `product/`,
`admin/`, `shop-owner/`, `auth/`, `login/`, `splash/`) plus `common/` for
shared pieces (`PriceResultCard`, `ResultsList`, `ImageUploadField` +
`ImageCropModal` — a dependency-free canvas crop/zoom/pan step before
upload, `StarRatingInput`, `ChangePasswordForm` — shared by admin and
shop-owner, parameterized by `backHref`, `ErrorBanner`, `EmptyState`,
`LoadingSpinner`). Styling is CSS Modules (`*.module.css`) colocated with
each component, not a global CSS framework.

`lib/api/` has one file per backend area: `otpAuth.ts` (shopper OTP
request/verify), `auth.ts` (admin/shop-owner password login),
`priceCompare.ts` (search + `getHomeFeed`), `priceSubmission.ts`
(crowdsourced submit + moderation queue), `adminCatalog.ts` (Category +
Location CRUD), `adminStores.ts` (Store CRUD incl. owner provisioning),
`geography.ts` (read-only Country→State→District comboboxes),
`shopOwner.ts` (own-store/product/price endpoints), `ratings.ts`,
`myAccount.ts` (self-service password change), `image.ts` — all going
through `lib/api/client.ts`'s `fetchJson<T>()`.

`lib/auth/` — `AuthContext.tsx` (exposes both `login` for password and
`loginWithOtp` for OTP, plus `logout`/`isAuthenticated`/`isReady`) +
`token-storage.ts` (single localStorage slot shared by both auth paths —
logging into one role replaces the other's session in the same browser) +
`jwt.ts` (`decodeJwtRoles`, used only for UI routing after password login —
picking `/shop-owner/dashboard` vs `/admin/dashboard` — never for actual
authorization, which the backend enforces independently via
`[AbpAuthorize]`). `lib/geolocation/` — browser geolocation hook + fallback
coords. `lib/hooks/useHomeFeed.ts` — loads `getHomeFeed()` on mount once
location is ready, backing the `/home` page's default (pre-search) view.

### Talking to the ABP backend (`lib/api/client.ts`)

- Every ABP dynamic-API/controller response — success or failure — is
  wrapped in an `AbpAjaxResponse<T>` envelope (`{ result, success, error,
  ... }`); `fetchJson<T>()` unwraps it and throws `ApiError` on failure, so
  callers only ever see the unwrapped `T`. Plain MVC controllers (e.g. the
  image upload controller) return `BadRequest(string)` instead of a thrown
  exception — that message lands under `result` rather than `error.message`,
  and `fetchJson` handles both shapes.
- **ABP's dynamic API derives the HTTP method from the app-service method
  name prefix**, not from any explicit routing: `Get*` → GET, `Create*`/
  `Insert*` → POST, `Update*` → PUT, `Delete*` → DELETE. `client.ts` itself
  doesn't derive this automatically — callers pass `method` explicitly, so
  when adding a new `lib/api/*.ts` call, match it to the C# app-service
  method name yourself or calls fail with a 405, not an obvious error.
- Auth: `fetchJson` attaches `Authorization: Bearer <token>` automatically
  unless `skipAuth: true` is passed (used by `requestOtp`/`verifyOtp`, since
  those calls happen before any session exists).
- `fetchJson` does **not** send an `Abp-TenantId` (or any tenant) header —
  there's no multi-tenant registration flow left on the frontend to need
  one, so every authenticated call resolves tenancy from the JWT alone.
- `FormData` bodies (image uploads) must not get an explicit `Content-Type`
  header — the browser needs to set its own multipart boundary — so
  `fetchJson` skips that header when `body instanceof FormData`.
