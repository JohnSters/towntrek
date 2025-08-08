# TownTrek CSS Architecture

## Entrypoints
- `css/entrypoints/public.css`: Public site bundle (foundation + components + landing)
- `css/entrypoints/admin.css`: Admin/Client portal bundle (foundation + components + admin layout + common features)

## Foundations
- `css/foundation/variables.css`
- `css/foundation/base.css`

## Components
- `css/components/buttons.css`
- `css/components/cards.css`
- `css/components/forms.css`
- `css/components/navigation.css`
- `css/components/footer.css`
- `css/components/alerts.css`
- `css/components/badges.css`
- `css/components/modal.css`

## Layouts
- `css/layouts/admin-layout.css`

## Features
- `css/features/landing/landing.css`
- `css/features/business/bundle.css` (imports form-shell, operating-hours, uploads, image-preview, category-sections, notifications)
- `css/features/admin/subscription.css`
- `css/features/admin/business.css`
- `css/features/subscription/{plans.css,limits.css,extras.css}`
- `css/features/towns/towns.css`
- `css/features/image-gallery/gallery.css`
- `css/features/auth/auth.css` (linked via `css/auth.css`)

## View linking guideline
- Public pages: include `css/entrypoints/public.css`
- Admin/Client pages: include `css/entrypoints/admin.css`
- Page-specific: include feature CSS under `css/features/...` as needed

## Legacy root wrappers
These exist for backward compatibility and can be removed once all views link to entrypoints/features directly:
- `css/site.css` (now imports entrypoints/site.css → superseded by entrypoints/public.css)
- `css/variables.css` (wrapper)
- `css/base.css` (wrapper)
- `css/buttons.css` (wrapper)
- `css/cards.css` (wrapper)
- `css/forms.css` (wrapper)
- `css/navigation.css` (wrapper)
- `css/footer.css` (wrapper)
- `css/landing.css` (wrapper)
- `css/confirmation-modal.css` (wrapper)
- `css/admin-subscription.css` (wrapper)
- `css/add-town.css` (migrated to `features/towns/towns.css`)
- `css/image-gallery.css` (migrated to `features/image-gallery/gallery.css`)
- `css/add-business.css` (migrated to `features/business/bundle.css`)
- `css/client-admin.css` (migrated into admin entrypoint + layout/components)
- `css/subscription.css` (wrapper: now imports components + features/subscription/*)
- `css/auth.css` (kept as the minimal entry for auth)

## Removal plan
- Safe to remove now if no view references remain: `client-admin.css`, `image-gallery.css`, `add-town.css`.
- Remove `subscription.css` after updating `_ClientLayout.cshtml` to rely solely on `entrypoints/admin.css` and feature links.
- Remove `site.css` and all other wrappers after confirming all layouts use entrypoints and pages use feature CSS.