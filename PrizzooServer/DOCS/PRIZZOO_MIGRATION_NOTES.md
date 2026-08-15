# Prizzoo migration notes

This solution is a stripped-and-rebuilt version of the original `Server.zip`
(FieldSale/YathraSoft/EnterpriseBase). It was restructured by Claude (chat, not
Claude Code) in a sandbox with **no .NET SDK and no network access** — meaning
none of this was compiled or built. Treat it as a strong starting point, not a
verified-working solution. The very first thing to do with it is
`dotnet build` and fix whatever the compiler finds.

## Why this exists

The original solution is a multi-tenant B2B field-sales/distribution ERP:
one tenant = one company managing its own customers, vans, routes, and sales.
Prizzoo is the opposite shape: a single public price index that many
independent stores are listed in, visible to every shopper regardless of who
manages the app. Those two shapes don't merge cleanly, so this pass keeps the
useful infrastructure and removes the incompatible domain logic. Full
reasoning is in the chat conversation this was generated from; the short
version is in `Store.cs` and `Product.cs`'s doc comments.

## What was removed

Entire domain folders deleted from both `EnterpriseBase.Core` and
`EnterpriseBase.Application`:

Sales, Purchases, Inventory, VanManagement, RouteManagement, Vouchers,
Accounting, CreditNotes, Tours, FinancialYears, Invoicing, and the old
`Parties` module (replaced by `Stores`).

Also removed: `Reports/` (accounting reports), `Misc/MiscInvoiceAppService.cs`,
`test/EnterpriseBase.Tests/Tours/`, two orphaned seed creator files
(`DefaultAccountHeadsCreator.cs`, `DefaultFinancialYearCreator.cs`), and the
entire old `Migrations/` folder (the schema changed too much for those to
still apply — see "Migrations" below).

**Correction made mid-pass:** `DashboardCustomization` was deleted first, then
restored — it turned out to be generic ABP.Zero dashboard widget
infrastructure (used by the admin theme's home screen), not FieldSale-specific
domain logic. `AppSettingProvider.cs` depends on it in 40+ places. If anything
else in the solution throws a missing-type error referencing
`EnterpriseBaseDashboardCustomizationConsts`, it means this restore was
incomplete — check that folder was actually copied back in full.

## What was added / rebuilt

- **`EnterpriseBase.Core/Stores/Store.cs`** (new) — replaces `Party`. Public,
  not tenant-scoped. Has `Latitude`/`Longitude` (carried over from the old
  `Party` fields), `OwnerUserId` (nullable, for future retailer self-service
  login), `IsVerified`.
- **`EnterpriseBase.Core/Pricing/Price.cs`** (new) — the actual core of the
  app. One row per product-per-store-per-observation, with `Status`
  (Pending/Approved/Flagged/Rejected), `Source`, `ObservedAt`. Deliberately
  append-only (don't overwrite rows) so price-history charts (Phase 2 of the
  business plan) work without a separate audit table later.
- **`EnterpriseBase.Core/MasterData/Product.cs`** (rewritten) — removed
  `TenantId`, `SKU`, `CostPrice`, `SellingPrice`, `MinimumStock`,
  `MaximumStock`, `TaxId`. A product is catalog metadata only now; its many
  prices live in `Price`, keyed by `Store`.
- **`Category.cs` / `Unit.cs`** — removed `IMustHaveTenant`, since these are
  shared public catalog data now, same reasoning as `Product`.
- **`EnterpriseBaseDbContext.cs`** — rewritten `DbSet` list to match the
  above; added indexes on `Price(ProductId, StoreId, Status)` and
  `Store(Latitude, Longitude)`.
- **`Application/Stores/`** (new) — `StoreAppService` (admin CRUD).
- **`Application/Pricing/`** (new) — two separate services on purpose:
  - `PriceCompareAppService` — **`[AllowAnonymous]`**. This is the one the
    shopper mobile app actually calls for search/compare. No login required.
    Does a rough lat/lng bounding-box pre-filter then exact Haversine
    distance in C#. **This will not scale past one city** — see the
    performance note in that file's doc comment for the Postgres
    `earthdistance`/PostGIS upgrade path.
  - `PriceSubmissionAppService` — authenticated. Shopper submits a
    crowdsourced price (`SubmitAsync`), admin/moderator approves or rejects
    it (`ModerateAsync`, `GetPendingAsync`). Currently gated with a bare
    `[AbpAuthorize]` (any logged-in user) as a placeholder — see "Known gaps"
    below.
- **`Application/Dashboard/`** (rewritten) — the old dashboard summed invoice
  revenue and cash balance. Replaced with `GetOpsDashboardAsync()`: store
  count, verified-store count, product count, pending-moderation count,
  prices approved in the last 7 days, recent pending submissions. The
  host-level tenant/user dashboard (`GetHostDashboardAsync`) was left as-is,
  it's generic and unrelated to the domain change.
- **`Application/CustomDtoMapper.cs`** — removed AutoMapper configs for
  Invoice/Voucher/Party/FinancialYear/Tour. Left a comment block where
  Product/Store/Price mappings should go once those Dto shapes stabilise —
  none were added yet because the three new AppServices above map manually
  (`MapToDto` methods) rather than via AutoMapper, which is fine for now but
  worth reconsidering once the Dto surface grows.

## Known gaps — things Claude Code should do next

1. **Regenerate migrations.** The old `Migrations/` folder is gone. Run
   `dotnet ef migrations add InitialPrizzooSchema` from
   `EnterpriseBase.EntityFrameworkCore` once the project builds cleanly.
   Don't try to hand-write a migration against the deleted history.
2. **`dotnet build` and fix compiler errors.** This was restructured without
   a compiler in the loop. High-probability trouble spots, roughly in order
   of likelihood:
   - `Web.Host/Startup/EnterpriseBaseNavigationProvider.cs` — almost
     certainly still has menu items pointing at deleted permission names or
     deleted controller routes (Sales, Vouchers, etc.). Not checked in this
     pass.
   - `EnterpriseBaseAuthorizationProvider.cs` — likely still *defines*
     permissions for deleted modules. These are just `CreatePermission(...)`
     calls with string constants, so they're not guaranteed to break the
     build, but they're dead weight worth pruning.
   - `PermissionNames.cs` / `PageNames.cs` — full of stale constants for
     deleted modules (Invoices, Vouchers, Tours, Party, etc.). These are
     plain `const string` fields, so they won't cause compile errors on
     their own, but clean them up once the rest of the build is green.
   - Any EF Core entity-configuration classes (if this project uses
     `IEntityTypeConfiguration<T>` files instead of only the inline
     `OnModelCreating` this pass edited) for the deleted entities — not
     found in this pass's search, but worth double-checking.
   - `Web.Host` controllers, if any exist outside the ABP dynamic API layer,
     for the deleted domains.
3. **Add a real moderator permission.** `PriceSubmissionAppService.GetPendingAsync`
   and `ModerateAsync` are gated with a bare `[AbpAuthorize]` — any logged-in
   user, not just moderators. Add `Pages_PriceModeration` (or similar) to
   `PermissionNames.cs`, register it in `EnterpriseBaseAuthorizationProvider.cs`,
   and swap the placeholder attribute.
4. **OTP phone login.** Not touched in this pass — the shopper identity flow
   discussed in the business plan (OTP instead of ABP's default
   username/password) still needs to be built on top of the existing
   `Authorization.Users` module.
5. **`earthdistance`/PostGIS.** `PriceCompareAppService` currently filters
   in C#, not SQL. Fine for one city's worth of stores at MVP scale; becomes
   a real bottleneck past that. See the file's doc comment.
6. **Wire up `Category`/`Unit` seed data** for the launch category (the
   business plan recommends mobiles/electronics or pharmacy) — no seed data
   was added in this pass.
7. **Leftover dead code, safe to ignore for now:** `Misc/MiscInvoice.cs`,
   `IMiscInvoiceAppService.cs`, and the print-settings DTOs
   (`InvoicePrintColumnsDto` etc. in `Configuration/Tenants/Dto/`) are
   self-contained and compile fine, but they're billing/invoicing features
   irrelevant to Prizzoo. Delete when convenient; not urgent.

## What was deliberately left alone

`Authorization`, `Identity`, `MultiTenancy` (present but unused by the public
catalog — see `Store.cs`/`Product.cs` comments for why), `Localization`,
`Storage`, `Editions`, `Features`, `Geography`, `Configuration`, `Branches`,
`Employees`, `Timing`, `Validation`, `Security`, `DataFilters`,
`ExtraProperties`, `ReleaseNotes`, `Taxes` (entity kept, just unused by
`Product` now), `DashboardCustomization` (restored, see above), Docker setup,
CI workflow files, `NuGet.Config`, PostgreSQL connection config. All of this
is either generic ABP infrastructure or genuinely reusable regardless of
domain.
