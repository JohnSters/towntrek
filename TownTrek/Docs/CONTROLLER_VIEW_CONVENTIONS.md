### Controller & View Conventions

- **Purpose**
  - **Consistency**: Use ASP.NET MVC conventions for routing and view discovery.
  - **Safety**: Prefer tag helpers over hardcoded URLs.
  - **Separation**: Keep Client and Admin UIs discoverable without explicit view paths.

- **Global Rules**
  - **Controllers return `View()`**; avoid explicit view path strings.
  - **No hardcoded URLs** in views; use tag helpers (`asp-controller`, `asp-action`).
  - **Naming**: Admin controllers follow `Admin{Name}Controller` (e.g., `AdminTownsController`).
  - **Client URLs** use a `/Client/...` prefix via controller-level route templates.
  - **Views live under** `Views/{AreaLike}/{Controller}/{Action}.cshtml` where `AreaLike` is `Admin` or `Client`.

### Routing & View Discovery

- **Default route** (Program.cs): `{controller=Home}/{action=Index}/{id?}`
- **Admin**
  - Controllers use default routing and rely on discovery.
  - `AdminViewLocationExpander` maps `Admin{Name}Controller` to:
    - `Views/Admin/{Name}/{View}.cshtml`
    - Fallback plural: `Views/Admin/{Name}s/{View}.cshtml`
- **Client**
  - Controllers use `[Route("Client/[controller]/[action]")]` to keep the `/Client/...` URL prefix.
  - `ClientViewLocationExpander` adds discovery paths:
    - `Views/Client/{Controller}/{View}.cshtml`
    - `Views/Client/{Controller}s/{View}.cshtml`
    - `Views/Client/{Controller}es/{View}.cshtml`
    - `Views/Client/Shared/{View}.cshtml`

### Implementations in this codebase

- **Top User Menu**
  - Componentized as `ViewComponents/TopUserMenuViewComponent.cs` + `Views/Shared/Components/TopUserMenu/Default.cshtml`.

- **Admin**
  - Controllers converted to discovery (`return View()`).
  - `AdminViewLocationExpander` registered in `Program.cs`.

- **Client**
  - `SubscriptionController` and `AnalyticsController` use `Index()` and `[Route("Client/[controller]/[action]")]`.
  - `BusinessController`
    - Route template: `[Route("Client/Business/[action]")]`.
    - Views discovered under `Views/Client/Businesses/{Action}.cshtml` (plural folder handled by expander).

### Links & Redirects

- **Use tag helpers** for navigation in Razor:
  - Example: `<a asp-controller="Business" asp-action="ManageBusinesses">`.
- **Post/Redirect endpoints**
  - Admin business moderation uses conventional actions (`/AdminBusinesses/Approve`, etc.). Prefer `asp-controller` + `asp-action` or `Url.Action` rather than literal URLs.

### Quick Reference

- **Client Subscription**
  - Controller: `Controllers/Client/SubscriptionController.cs`
  - Route: `/Client/Subscription/Index`
  - Views: `Views/Client/Subscription/{Index,Billing}.cshtml`
- **Client Analytics**
  - Controller: `Controllers/Analytics/AnalyticsController.cs`
  - Route: `/Client/Analytics/Index`
  - View: `Views/Client/Analytics/Index.cshtml`
- **Client Businesses**
  - Controller: `Controllers/Business/BusinessController.cs`
  - Route: `/Client/Business/ManageBusinesses` (and other actions)
  - Views: `Views/Client/Businesses/{Index,ManageBusinesses,AddBusiness,Edit}.cshtml`
- **Admin Discovery**
  - Expander: `Extensions/AdminViewLocationExpander.cs`
  - Examples: `AdminTownsController` → `Views/Admin/Towns/{View}.cshtml`, `AdminUsersController` → `Views/Admin/Users/{View}.cshtml`

### Migration Checklist (for future changes)

- **Controllers**
  - Remove explicit view paths; ensure actions return `View(model)`.
  - Apply `[Route("Client/[controller]/[action]")]` to Client controllers that should live under `/Client/...`.
  - Keep Admin controllers on default routing.
- **Views**
  - Move files into `Views/Admin/{Name}/{Action}.cshtml` or `Views/Client/{Controller}/{Action}.cshtml`.
  - For pluralized folders (`Businesses`), rely on `ClientViewLocationExpander` plural fallbacks.
- **Links**
  - Replace hardcoded URLs with tag helpers.
  - Update JS form posts to conventional endpoints (or use form tag helpers when possible).

### Known Behavior

- If a user hits plan limits, `AddBusiness` redirects to `Subscription/Index`, which resolves to `Views/Client/Subscription/Index.cshtml`.

### Future Improvements

- Consider using **Areas** for Client/Admin (`Areas/Client/Views/...`, `Areas/Admin/Views/...`) to formalize routing and discovery.
- Add/align guidance in `Docs/FRONTEND_ARCHITECTURE_RULES.md`:
  - No hardcoded URLs in views; always use tag helpers.
  - Controllers return `View()`; avoid explicit view paths.
  - Client/Admin views live under `Views/Client/*` and `Views/Admin/*` (or Areas).
