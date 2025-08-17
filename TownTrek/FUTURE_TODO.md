# Future TODOs

Centralized list of non-blocking tasks and production hardening items to tackle later.

## Subscription & Access Control
- Add app setting to disable legacy subscription flag fallback in production (gate in `SubscriptionAuthService`).
- Normalize and clean stale roles on login: map `CurrentSubscriptionTier` to `AppRoles` and remove mismatched `Client-*` or `Client-Trial` when not applicable.
- ✅ Removed `TestAnalyticsController` diagnostics.

## Analytics & Charts
- Implement real per-day view tracking:
  - Create `BusinessViewLog` (or daily aggregate) and increment in `RecordBusinessViewAsync`.
  - Update `AnalyticsService.GetViewsOverTimeAsync` to query real counts instead of simulated data.
- Optional interim: distribute `Business.ViewCount` over N days with noise for better demo visuals.

## Payments
- Align payment provider naming and services (PayFast vs PayPal) across code and docs.
- Implement secure webhook/IPN handling with signature validation and idempotent processing.
- Map provider payment states to internal statuses consistently.

## Security & Auth
- Optional MFA for production; review password policy before go-live.
- Add audit logs for subscription/role changes and payment transitions.

## Configuration
- Introduce environment toggles: `AllowLegacyTierFallback`, `EnableDiagnostics`, `UseSandbox`.
- Ensure secrets are not in production appsettings; use env vars/secret store.

## Misc
- Implement Forgot Password flow (`AuthController` placeholder).
- Consider external time validation for trial security (`SecureTrialService` TODO comment).


