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
  of the roadmap **is now built** (see Auth model below), and in fact now
  covers admin login too, not just shoppers. Retailer price updates were
  meant to come in via **WhatsApp Business**, not a web dashboard, because
  most kirana owners won't log into a portal; that intake channel still
  isn't built. A password-authenticated self-service shop-owner web
  dashboard was built as a stopgap and has since been **torn out entirely**
  (`ShopOwnerAppService`, `app/shop-owner/`, store-owner provisioning on
  `Store` — all deleted, see Auth model and Modules deleted below); what
  exists today instead is admin-direct price entry
  (`PriceSubmissionAppService.CreateApprovedAsync`, `app/admin/prices/`).
  `Price`'s `PriceSource.RetailerReported`/`Crowdsourced` enum values predate
  this churn and no longer map cleanly to "who submitted it" — see the
  domain model notes below. ONDC
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
  update → gets checked → goes live for shoppers. The shop-owner-side diagram
  is now stale on *who* performs that update: the dedicated shop-owner
  actor/login it depicts has been removed, and the update instead comes from
  either an anonymous shopper (`PriceSubmissionAppService.SubmitAsync` +
  moderation) or an admin entering it directly
  (`PriceSubmissionAppService.CreateApprovedAsync`, no moderation step).
- **`HOME-Black-Mode.pdf`** / **`LOGIN.pdf`** — mobile UI mockups (brand name
  styled "PriZzoO.com", tagline "Compare. Locate. Save"). `LOGIN.pdf` shows
  an OTP phone-login screen, which the current `/phone-entry` → `/otp-verify`
  flow implements (see Frontend below), though not pixel-for-pixel.
  `HOME-Black-Mode.pdf` shows a much richer home feed than the backend fully
  supports — dark theme, top nav tabs (Home/Videos/Offers/Retailers/
  Categories/Events), a retailer logo strip, a spotlight banner carousel,
  category icon tiles, and promo banners. The current `/home` page
  (`HomeFeed` component + `GetHomeFeedAsync`) implements the "Top Picks for
  You" style sectioned product feed and now matches the mockup's dark theme
  (`app/globals.css` is dark-only, `color-scheme: dark`, no light variant)
  plus a real `RetailerStrip` (deduped from actual nearby `Store` data, not
  a fake merchant list — tapping a tile browses that store's catalog via
  `PriceCompareAppService`'s `StoreId` filter) and a `StoreFlyerBanner`
  peek-carousel matching the mockup's "In the spotlight" banner (see Flyer
  feature below). The Videos/Offers/Events tabs still have no backend
  support (no Video/Offer/Event entities exist) and render as a "coming
  soon" placeholder. Treat the mockup as target UI, not a spec of what the
  API currently returns.

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
  login), so tokens from either would be interchangeable everywhere
  `[AbpAuthorize]` is used — but see Auth model below: `TokenAuthController`
  is unreferenced by the frontend today, since OTP is now the only login
  path for every role.
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
  Only `Users/` and `Sessions/` have test classes today — none of the
  Prizzoo-specific app services (`PriceCompareAppService`,
  `PriceSubmissionAppService`, `FlyerAppService`, `AdminAppService`, etc.)
  have any test coverage yet; there's no existing pattern to follow for
  testing them, only the generic ABP test-base scaffolding above.
- **test/EnterpriseBase.Web.Tests** — thin web/controller-level tests.

### Auth model: OTP for everyone, admin included

This has changed twice: originally shoppers used OTP while admin/shop-owner
used passwords; then a self-service shop-owner role was added (also
password-based); **both password paths are now gone**. Every login — admin
or shopper — goes through the same phone+OTP flow, and `TokenAuthController`
(`/api/TokenAuth/Authenticate`, password-based, still present in
`Web.Core/Controllers/`) is unreferenced by the frontend and effectively
dead — don't build new features against it, and don't be surprised it still
compiles.

- **`OtpAuthController`** (`Web.Core/Controllers/OtpAuthController.cs`) is
  now the *only* login path, for both roles. `RequestOtp` (→
  `Application/Otp/OtpChallengeService`) creates an `OtpChallenge` row
  (`Core/Authorization/Otp/OtpChallenge.cs`, hashed code, 5-min expiry,
  throttled) and sends it via `ISmsSender`; the only implementation is
  `NoopSmsSender`, which just logs the code — **there is no real SMS
  integration**, and `GenerateCode()` currently hardcodes `"123456"` instead
  of a random code (explicit TODO in the file). Anyone can log in as any
  phone number right now — don't treat this as a real auth boundary until
  that's fixed. `VerifyOtp` looks up/creates a `User` under the default
  tenant on first success (phone becomes `UserName`, synthesized placeholder
  email, random unusable password) and grants role `Shopper` — **unless**
  that phone number already carries the `Admin` role (assigned by seeding or
  by `AdminAppService`), in which case it logs in as an admin instead.
  Which role a phone number gets is decided **server-side, not by anything
  the client sends** — see `AuthContext.tsx`'s `login()` on the frontend
  side. Admin sessions get the short 1-day expiration
  (`AppConsts.AccessTokenExpiration`) rather than the shopper one, since the
  OTP is still the stubbed `"123456"` code and a month-long admin session
  would be riskier; shoppers keep the 30-day `AppConsts.
  ShopperAccessTokenExpiration` (no refresh-token flow, so a shopper
  shouldn't have to re-verify their phone every visit).
- **`EnterpriseBaseConsts.InitialAdminPhoneNumber`** is the one admin phone
  number seeded directly into the database (`TenantRoleAndUserBuilder.cs`) —
  the bootstrap account with no chicken-and-egg problem. From there,
  `AdminAppService` (`Application/Admin/`, gated `Pages_Admins`,
  `app/admin/admins/`) lets a logged-in admin add further admin phone
  numbers; there's still no self-registration into the `Admin` role, only
  admin-invites-admin.
- **`RegisteredUserAppService`** (`Application/Users/`, gated
  `Pages_RegisteredUsers`, `app/admin/users/`) is a read-only admin view of
  every `Shopper`-role account created via OTP login — shoppers self-create
  on first `VerifyOtp`, so there's nothing to add/edit here, only to list.

Roles (`Core/Authorization/Roles/StaticRoleNames.cs`): just `Host.Admin`,
`Tenant.Admin`, `Tenant.Shopper` — **`Tenant.ShopOwner` is gone**, along with
the entire self-service shop-owner login model (`ShopOwnerAppService`,
`Store.OwnerUserId`, `StoreDto.OwnerTemporaryPassword`, `app/shop-owner/`;
see Modules deleted below). The older "Retailer" role/module was removed
even earlier (`RetailerAppService`/`RetailerDtos.cs` deleted, `Pages_Retailer`
permission removed) and never came back under either name — the untouched
`PriceSource.RetailerReported` enum value is the only trace either one left
behind. There's no separate moderator role: `Pages_PriceModeration` and
`Pages_Products_Edit` are granted to the same tenant `Admin` role that does
tenant administration and store/price CRUD, so one admin phone number does
everything back-office.

### Domain model: public catalog, not multi-tenant

This is the key architectural deviation from the stock ABP template, and the
source of most "why is this not tenant-scoped" questions:

- `Store`, `Product`, `Category`, `Unit`, `Price`, `StoreChain`, `Flyer`,
  `FlyerItem` are deliberately **not** `IMustHaveTenant`/`IMayHaveTenant`. Prizzoo is one
  shared public price index, not a per-tenant ERP — every shopper sees every
  store's data regardless of who administers it. `Store.TenantId` exists as
  a plain nullable int reserved for a future Phase 3 white-label mode; adding
  `IMayHaveTenant` to it now would make ABP's tenant filter silently hide
  host-created stores from tenant-scoped queries, so don't do that without
  re-reading the reasoning in `Store.cs`'s doc comment first. `Store` no
  longer has an `OwnerUserId` (removed with the shop-owner model, see Auth
  model above) — there is no per-store owner login, so nothing here is
  scoped by owner any more. `Store` does have a nullable `LocationId` FK
  (see MasterData below) as the source of truth for where it is, with
  `Store.City` kept as a denormalized display string.
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
- `PriceSubmissionAppService` covers both the shopper crowdsourced path and
  the admin direct-entry path, now that there's no separate shop-owner
  service: any authenticated user can `SubmitAsync` a price
  (`Source=Crowdsourced`, goes to `Pending`) — though currently **nothing in
  the frontend calls it**, `submitPrice()` exists in `lib/api/
  priceSubmission.ts` unused by any component, so crowdsourced submission is
  backend-only right now. `GetPendingAsync`/`ModerateAsync` (admin
  moderation queue, `app/admin/moderation/prices/`) and `CreateApprovedAsync`
  (admin direct entry, skips moderation entirely, `Source=RetailerReported`,
  `app/admin/prices/` via `AddStorePriceForm`) all require
  `Pages_PriceModeration`, as do `GetAllAsync`/`UpdateAsync`/`DeleteAsync` —
  the full admin price-list CRUD backing `PriceList`/`EditPriceForm`. The
  `INotifyShopOwnerService` shop-notification seam this service used to call
  on rejection has been deleted along with the rest of the shop-owner model
  (see Auth model above) — rejection is now silent, no notification path
  exists.
- `Flyer` (`Core/Pricing/Flyer.cs`, `Application/Flyers/FlyerAppService.cs`)
  is deliberately minimal: **admin-only**, **manual entry**, **no
  moderation step** — an earlier OCR/AI-extraction design (a `FlyerItem`
  entity with bounding boxes, a `NoopFlyerExtractionService` seam, a
  shop-owner self-upload path, a pending-review queue) was built and then
  torn out in favor of this simpler version; don't resurrect any of that
  from memory or old docs. `FlyerAppService.CreateFlyerForStoreAsync`
  (gated `Pages_PriceModeration`) takes a `StoreId`, an `ImageId`, and a
  hand-typed list of `{name/productId, price, categoryId?}` line items in
  one call; each line either picks an existing `Product` via `ProductId`,
  or is matched by exact case-insensitive name against the catalog, or
  creates a new `Product` (optionally under a category) if no match exists
  — a "pre-verified actor, goes live immediately" pattern also used by
  `PriceSubmissionAppService.CreateApprovedAsync` (see above).
  `AddItemsToFlyerAsync` (same gate, same per-item resolution logic,
  factored into a shared `InsertItemsAsync` private helper) lets an admin
  append more items to a flyer that's already live, e.g. from the
  `app/admin/flyers/` list's "Add product" action. `Flyer` itself carries no
  status field (creating one *is* publishing it).

  **`FlyerProduct`** (`Core/Pricing/FlyerProduct.cs`) is the master/detail
  link between a `Flyer` and the `Product`s it features — just `FlyerId` +
  `ProductId`, a unique index on the pair, nothing else. It deliberately
  carries **no price of its own**: `Price` is a fully separate entity again
  with no connection to `Flyer` at all (no `FlyerId`/`Flyer` nav —
  `PriceSource.Flyer` was removed too). This replaced an earlier design
  (migration `SimplifyFlyerToManualEntry`, then superseded by migration
  `AddFlyerProductAndRemovePriceFlyerId`) where every flyer item wrote its
  own new `Price` row — which meant a product already priced at a store got
  a **second**, duplicate `Price` row the moment it was also put on that
  store's flyer. Now `InsertItemsAsync` only ever inserts a `Price` row the
  *first* time a product has no existing Approved price at that store yet
  (a genuinely new price, tagged `Source = RetailerReported` like any other
  admin-direct entry); if one already exists, it links via `FlyerProduct`
  and leaves `Price` untouched. Anywhere a flyer's items need displaying —
  `BuildFlyerDetailDtosAsync` (backing `GetFlyersForStoreAsync`/
  `GetRecentFlyersAsync`) and `PriceCompareAppService.ComparePricesAsync`'s
  `FlyerId`-scoped branch — resolves each item's current price by looking
  it up live from `Price` (`ProductId` + the flyer's own `StoreId`,
  `Status == Approved`, most recent `ObservedAt` wins), never from
  `FlyerProduct`. This is why a flyer-listed product still shows up in
  ordinary keyword search/home-feed too: it's just its own regular `Price`
  row doing that, same as any other product — `FlyerProduct` only decides
  what's *featured on the flyer*, not what's searchable.

  `ComparePricesInputDto.FlyerId` is a `PriceCompareAppService` filter (like
  the existing `StoreId` one) that **deliberately skips the usual
  bounding-box/radius geo-filtering** — `FlyerAppService.
  GetRecentFlyersAsync` (backing the home screen's all-stores carousel)
  isn't geo-scoped either, so a flyer from outside the shopper's configured
  radius can legitimately be on screen, and silently returning zero results
  for a valid tap would be confusing. Shoppers browse flyers via
  `StoreFlyerBanner` (`components/home/StoreFlyerBanner.tsx`), a peek
  carousel shown directly on `/home` — both an all-stores one (above "Top
  Picks for You", populated from `GetRecentFlyersAsync`) and a per-store one
  (above that store's results, from `GetFlyersForStoreAsync`, once a
  `RetailerStrip` tile is tapped). Tapping a flyer slide calls
  `useComparePrices().searchByFlyer` and renders results inline via the
  same `ResultsList`/`PriceResultCard` used everywhere else — no page
  navigation, no separate flyer-item UI. The standalone `/store-flyer?
  storeId=` page (query param, not a dynamic route segment — see the
  static-export note under Frontend below) still exists and still works,
  but nothing currently links to it; it's an orphaned direct-URL view, not
  dead code to delete on sight.
- `Application/MasterData/Locations/` (`Location` entity,
  `Core/MasterData/Location.cs`) is a locality *within* a `District` (e.g.
  "Feroke" within "Kozhikode") — **distinct from** the pre-existing,
  unrelated `Core/Geography`/`Application/Geography` Country→State→District
  CRUD module; don't conflate the two. `DefaultGeographyCreator.cs` seeds
  India/Kerala/all 14 Kerala districts as fixed MVP constants (no admin UI
  for country/state); `District` plays the "city" role in store-creation
  forms, and `Location` narrows within it. `LocationAppService.
  CreateOrEditAsync`/`DeleteAsync` stay admin-only (`Pages_Locations_*`),
  but `GetAllAsync`/`GetForComboboxAsync` are gated `[AbpAuthorize
  (Pages_Locations, Pages_Shopper)]` — an OR, so any authenticated user can
  read the list, not just admins — since shoppers now need it too (see the
  `LocationPickerModal` note under Frontend below). A `Location`'s
  `Latitude`/`Longitude` are `[Required]` on every create/edit call and are
  captured **only** via the admin's device GPS (`LocationMaster.tsx`'s "use
  my current location" button) — there is no manual numeric-entry path any
  more, and `GetForComboboxAsync` only ever returns locations that already
  have both set. The entity columns stay nullable for legacy rows created
  before this was enforced. `Store.Latitude`/`Longitude` are no longer
  independently settable at all: `LocationId` is `[Required]` on
  `CreateStoreDto`/`UpdateStoreDto`, and `StoreAppService` derives a store's
  coordinates entirely from its `Location` server-side (rejecting the save
  if that `Location` has no coordinates yet) — a `Location` is now the sole
  source of geocoding anywhere in the app, both for stores and (see below)
  for a shopper's picked location. Existing stores were one-time backfilled
  from their `Location`'s coordinates by migration
  `SyncStoreCoordinatesFromLocation`.
- `Application/Ratings/ProductRatingAppService.cs` (`Core/Pricing/
  ProductRating.cs`) is a simple product-scoped (not store/price-scoped)
  1–5 star rating, one per shopper per product (unique index on
  `ProductId`+`ShopperUserId`, rating again overwrites). `Application/
  Account/MyAccountAppService.ChangeMyPasswordAsync` is a self-service
  password-change endpoint, kept distinct from `UserAppService.
  ChangePassword` (which requires `Pages_Users` and would 403 for a
  Shopper) — but now that every login is phone+OTP with no password to
  change, it's unreferenced by the frontend and effectively dead code, kept
  around as harmless rather than actively used.
- Multi-tenancy (`Authorization`, `Identity`, `MultiTenancy` modules) is kept
  and functional for host/tenant admin accounts, but is orthogonal to the
  public catalog — don't assume `AbpSession.TenantId` scoping applies to
  `Store`/`Product`/`Price` queries the way it would in the original
  ERP template.

### Modules deleted from the original template

Sales, Purchases, Inventory, VanManagement, RouteManagement, Vouchers,
Accounting, CreditNotes, Tours, FinancialYears, Invoicing, the old `Parties`
module, `Retailer` (briefly replaced by `ShopOwner`), and — most recently —
`Branches` and `Employees` (unrelated leftover ERP modules, migration
`RemoveBranchEmployeeModuleAndDemoSeeder`) and `ShopOwner` itself
(`ShopOwnerAppService`, its DTOs, `INotifyShopOwnerService`, `Store.
OwnerUserId`; migration `RemoveShopOwnerModel`). Nothing has replaced
`Retailer` or `ShopOwner` — see Auth model above for what price-submission
looks like now that neither exists. `EnterpriseBaseNavigationProvider.cs`,
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
`http://localhost:5001`, the ABP host above) is the only one left.
`NEXT_PUBLIC_DEFAULT_TENANT_ID` (`lib/api/config.ts`'s `DEFAULT_TENANT_ID`)
is dead code — it existed for self-registration, which has been removed in
favor of OTP login. `NEXT_PUBLIC_DEFAULT_LATITUDE`/`NEXT_PUBLIC_DEFAULT_LONGITUDE`
(a raw-geolocation fallback) were removed entirely once shoppers stopped
using device GPS at all — see the `LocationPickerModal` note below.

### Structure

There is now **one login flow for everyone** — admin included, no more
separate password login or shop-owner flow (both were removed; see Auth
model above). `app/page.tsx` (splash, auto-redirects based on
`AuthContext.isAuthenticated`) → `app/phone-entry/` (`PhoneEntryForm`) →
`app/otp-verify/` (`OtpVerifyForm`) → `app/home/`. Which role you land as is
decided by the backend per phone number, not by which page you started
from; an admin phone number reaches `/admin/dashboard` via the account menu
from `/home`, not a separate redirect.

- **Shopper-facing**: `app/home/` (search + `HomeFeed` sectioned product
  feed + `RetailerStrip` + `StoreFlyerBanner`) → `app/product/`
  (`?keyword=` query param, see static-export note below; `ProductHeader`
  incl. `StarRatingInput` + `ResultsList`). `app/store-flyer/` (`?storeId=`)
  still exists as a standalone deep link but isn't linked from anywhere in
  the UI anymore — flyers are browsed inline on `/home` instead (see Flyer
  feature above). Every search on `/home` and `/product` is scoped to a
  **shopper-picked `Location`**, not raw device GPS: `/home` shows a
  mandatory, non-dismissible `LocationPickerModal` (District→Location
  select, reusing the same combobox calls the admin store form uses) the
  first time a shopper has no location saved yet (see `lib/location/`
  below); `LocationBar` is tappable to reopen the same picker, dismissibly,
  to change it later. `/product` redirects to `/home` if it's ever reached
  with no location picked (e.g. a direct deep link), since that's where the
  mandatory picker runs.
- **Admin** (same OTP login, routed to `/admin/*` once the JWT's role claim
  says `Admin` — see `AdminDashboard.tsx`'s own client-side guard, on top of
  the backend's independent `[AbpAuthorize]`): `app/admin/dashboard/` links
  out to `app/admin/stores/` (list/new/edit — no owner provisioning any
  more, just store fields), `app/admin/products/` + `app/admin/categories/`
  + `app/admin/units/` (catalog master data — `CreateProductForm`/
  `EditProductForm`/`ProductList`, `CategoryMaster`, `UnitMaster`),
  `app/admin/prices/` (direct price CRUD against every price ever recorded
  — `PriceList`/`AddStorePriceForm`/`EditPriceForm`, backed by
  `PriceSubmissionAppService`'s `GetAllAsync`/`CreateApprovedAsync`/
  `UpdateAsync`/`DeleteAsync`) *and, separately*,
  `app/admin/moderation/prices/` (the pending-queue approve/flag/reject
  view, `AdminPriceModerationQueue` — a different flow from `/admin/prices`,
  not a duplicate of it), `app/admin/flyers/new/` (`UploadFlyerForStoreForm`
  — store picker, photo, and a dynamic add/remove list of item rows in one
  submit; deliberately breaks out of the app's usual 480px mobile shell into
  a two-column split at ≥860px, since it's a back-office form rather than
  shopper-facing UI — see its own `.module.css` comment before changing that
  layout) *and, separately*, `app/admin/flyers/` (`FlyerList` — a table of
  every uploaded flyer, newest first, for checking a flyer's photo and
  mapped items after upload; reuses `FlyerAppService.GetRecentFlyersAsync`
  — the same shopper-facing endpoint the home screen's carousel calls,
  since the tenant `Admin` role already carries every `Pages_Shopper`-gated
  permission — rather than a separate admin-only endpoint; each row's
  "View" jumps to `/store-flyer?storeId=`, the same standalone view
  `EditStoreForm`'s "View uploaded flyers for this store" link uses, and
  "Add product" navigates to `app/admin/flyers/add-item/` (`?flyerId=&
  storeId=`, same static-export query-param reasoning as `/product` and
  `/store-flyer` — a full page rather than a `Modal`, so the flyer's photo
  and its already-added items can sit on the left, `AddFlyerItemsForm` on
  the right, the same "photo left, items right" split
  `UploadFlyerForStoreForm` uses; calls `AddItemsToFlyerAsync`). Unlike the
  initial upload form, `AddFlyerItemsForm` deliberately has **no "+ New
  product" option** — every item added here must map to a product that
  already exists in Manage products; the initial upload form still allows
  creating one inline), `app/admin/locations/`,
  `app/admin/admins/`
  (`AdminAccountsPage` — add further admin phone numbers, see
  `AdminAppService` above), `app/admin/users/` (`RegisteredUsersList` —
  read-only shopper list, see `RegisteredUserAppService` above). There is no
  `app/login/`, no `app/admin/change-password/`, and no `app/shop-owner/`
  tree any more — all deleted along with password auth.

One route uses a `?query=param` instead of a `[dynamicSegment]`
(`/product?keyword=`; `/store-flyer?storeId=` likewise) — deliberate, since
`next.config.ts` sets `output: "export"` for static hosting (Azure Static
Web Apps), and a dynamic segment would need every possible value known at
build time (`generateStaticParams`), which is impossible for arbitrary
search terms/ids. Each such page wraps its `useSearchParams()` call in
`<Suspense>`, which that hook requires under static export.

`components/` has one subfolder per route area (`home/`, `product/`,
`admin/`, `store-flyer/`, `auth/`, `splash/`) plus `common/` for shared
pieces (`PriceResultCard`, `ResultsList`, `ImageUploadField` +
`ImageCropModal` — a dependency-free canvas crop/zoom/pan step before
upload, `StarRatingInput`, `Modal`, `ImageWithFallback`, `ErrorBanner`,
`EmptyState`, `LoadingSpinner`). There is no `components/login/` or
`components/shop-owner/` any more — both were deleted with password auth,
including the `ChangePasswordForm` they used to share, since there's no
password left to change from the UI (the backend endpoint survives as dead
code, see `MyAccountAppService` above). Styling is CSS Modules
(`*.module.css`) colocated with each component, not a global CSS framework.

`lib/api/` has one file per backend area: `otpAuth.ts` (OTP request/verify —
the only login call now, for every role), `priceCompare.ts` (search +
`getHomeFeed`, incl. flyer-scoped search), `priceSubmission.ts`
(crowdsourced submit, moderation queue, and admin direct-entry/CRUD — see
`PriceSubmissionAppService` above), `adminCatalog.ts` (Category + Location
CRUD, plus a read-only Product combobox reused by the flyer upload form's
"pick an existing product" option), `adminStores.ts` (Store CRUD, no more
owner provisioning), `admins.ts` (`AdminAppService` calls),
`registeredUsers.ts` (`RegisteredUserAppService` calls), `product.ts`,
`geography.ts` (read-only Country→State→District comboboxes), `flyer.ts`
(`createFlyerForStore`, `getFlyersForStore`, `getRecentFlyers` — see Flyer
feature above), `ratings.ts`, `image.ts` — all going through `lib/api/
client.ts`'s `fetchJson<T>()`. There is no more `auth.ts`, `shopOwner.ts`,
or `myAccount.ts` — all deleted with password auth.

`lib/auth/` — `AuthContext.tsx` (`login(phoneNumber, code)` — OTP only, for
every role — plus `logout`/`isAuthenticated`/`isAdmin`/`isReady`, where
`isAdmin` is derived from the JWT's role claim purely for UI
navigation/visibility) + `token-storage.ts` (single localStorage slot,
shared by whatever role most recently logged in) + `jwt.ts`
(`decodeJwtRoles`, used only to compute `isAdmin` — never for actual
authorization, which the backend enforces independently via
`[AbpAuthorize]`). `logout()` also clears the shopper's picked location
(below) so it doesn't silently carry over to a different shopper logging in
next on the same browser. `lib/geolocation/coordinates.ts` is now just the
plain `Coordinates` type (`{ latitude, longitude }`) — the `useGeolocation`
hook and its `DEFAULT_LOCATION` fallback that used to live in this folder
were deleted outright, since nothing in the app reads raw device GPS
anymore. `lib/location/` is the shopper equivalent of `lib/auth/`:
`shopperLocation.ts` persists a shopper's picked `Location` (id, name,
district, lat/lng) to a single localStorage slot, and
`useShopperLocation.ts` is the read/write/clear hook both `/home` and
`/product` use in place of the old `useGeolocation()` call.
`components/home/LocationPickerModal.tsx` is the District→Location picker
UI itself, built on the shared `Modal` component's new `dismissible` prop
(`false` for the mandatory first-pick case). `lib/hooks/useHomeFeed.ts` —
loads `getHomeFeed()` once a location is picked, backing the `/home` page's
default (pre-search) view.

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
