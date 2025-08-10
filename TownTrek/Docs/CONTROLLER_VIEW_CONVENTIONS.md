### View/Route Convention Migration Notes

- Objective
  - Standardize on ASP.NET MVC conventions: controllers return `View()` without hardcoded paths; views live in `Views/{Controller}/{Action}.cshtml`.
  - Replace hardcoded URLs with tag helpers (`asp-controller`, `asp-action`) for safer navigation.
  - Ensure Client-specific views resolve correctly without path strings.

- Key Implementations
  - User Top Nav: created `ViewComponents/TopUserMenuViewComponent.cs` + `Views/Shared/Components/TopUserMenu/Default.cshtml` and replaced inline header menu in layouts. Header data (name/tier) is now centralized and auto-fetched.
  - View Discovery:
    - Added `Extensions/ClientViewLocationExpander.cs`, registered in `Program.cs` to search:
      - `/Views/Client/{Controller}/{View}.cshtml`
      - `/Views/Client/Shared/{View}.cshtml`
    - This lets Client controllers use `return View()` with Client views staying under `Views/Client`.
  - Public/Home consolidation: moved/created public pages under `Views/Home/*` and converted links to tag helpers.
  - Admin controllers: converted to discovery (`return View(...)`).
  - Client controllers:
    - `ProfileController`: discovery.
    - `SubscriptionController`:
      - Action renamed to `Index()` for convention.
      - Route updated to `[Route("Client/[controller]/[action]")]` so: `/Client/Subscription/Index` (and `/Client/Subscription`).
      - Views remain under `Views/Client/Subscription/{Index,Billing}.cshtml`.
    - `BusinessController`:
      - Edit uses `Views/Client/Businesses/Edit.cshtml`; content reused from existing `EditBusiness.cshtml` (kept temporarily). Add/Manage links updated to tag helpers.

- Links/Redirects Clean-up
  - Replaced `Url.Action(...)` and literal `href="/..."` with tag helpers in:
    - `Views/Shared/_ClientLayout.cshtml`, `Views/Client/Dashboard.cshtml`, `Views/Client/Businesses/{Index,ManageBusinesses,Edit}`, `Views/Image/{Gallery,MediaGallery}`, and the public landing page.
  - Redirects updated to conventional endpoints, e.g. `RedirectToAction("Index", "Subscription")`.

- Known Behavior
  - If a user hits plan limits, Add Business redirects to Subscription. With the expander and route changes, it now resolves to `Views/Client/Subscription/Index.cshtml`.

- Recommendations (Next)
  - Finish rename: inline `EditBusiness.cshtml` content into `Edit.cshtml` and delete `EditBusiness.cshtml`.
  - Sweep for any remaining `Url.Action(...)` to tag helpers across Admin/Client views.
  - Adopt a consistent route template for all Client controllers (e.g., `[Route("Client/[controller]/[action]")]`) to avoid future surprises.
  - Longer-term: consider Areas for “Client” and “Admin” to formalize routing and view structure (`Areas/Client/Views/...`, `Areas/Admin/Views/...`).
  - Add guidance to `Docs/FRONTEND_ARCHITECTURE_RULES.md`:
    - No hardcoded URLs in views; always use tag helpers.
    - Controllers return `View()`; avoid explicit view paths.
    - Client/Admin views should live under `Views/Client/*` and `Views/Admin/*` (or Areas).

- Quick Reference
  - Client Subscription route:
    - Controller: `Controllers/Client/SubscriptionController.cs`
    - Route: `/Client/Subscription/Index` (via `[Route("Client/[controller]/[action]")]`)
    - Views: `Views/Client/Subscription/{Index,Billing}.cshtml`
  - Business Edit:
    - Controller returns `Edit.cshtml`
    - View: `Views/Client/Businesses/Edit.cshtml`
    - Temporary content partial: `EditBusiness.cshtml` (to be inlined and removed)

- Testing Checklist
  - Home landing renders via `Views/Home/Index`.
  - Client sidebar: Dashboard, Add Business, Manage Businesses, Media Gallery resolve correctly.
  - Add Business at plan limit → redirects to `/Client/Subscription` (Index).
  - Admin menus render users/towns/services/categories via discovery without explicit paths.


