# TownTrek – Mailtrap + PayFast Sandbox Integration Guide

This document captures the end‑to‑end process to integrate transactional emails (Mailtrap) and PayFast payments in a sandbox environment, including pitfalls, fixes, and a production hardening checklist.

## Architecture overview

- Email service: MailKit + Razor templates
  - Service: `Services/EmailService.cs`
  - Template renderer: `Services/EmailTemplateRenderer.cs`
  - Templates: `Views/Emails/*` with layout `Views/Emails/_EmailBaseLayout.cshtml`
  - Options: `Options/EmailOptions.cs` bound in `Program.cs` (section `Email`)
- PayFast integration
  - Registration creates a pending subscription and generates a signed payment request
  - Return/cancel/notify URLs are built from `BaseUrl` (config or derived from request)
  - IPN notify activates subscription (success email) or flags failure (failure email)
  - Options: `Options/PayFastOptions.cs` bound in `Program.cs` (section `PayFast`)
  - Server‑side form POST to PayFast avoids manual URL/sig issues: `GET /Api/Payment/Process?subscriptionId={id}` → view `Views/Payment/AutoPost.cshtml`

## Sandbox prerequisites (Mailtrap, ngrok, PayFast)

- Mailtrap (Email Testing)
  - Host: `sandbox.smtp.mailtrap.io`
  - Port: `2525`
  - STARTTLS: on
  - Username/Password: from testing inbox
- Public dev URL (ngrok/Cloudflare)
  - Example: `https://<id>.ngrok-free.app`
  - Stable during the test; changing it requires regenerating the payment URL/signature
- PayFast (Sandbox)
  - Passphrase: keep empty or match in both PayFast and app exactly
  - URLs must be public (notify uses POST from PayFast)

## Development configuration (user‑secrets)

```powershell
# Base URL (public dev URL)
dotnet user-secrets set "BaseUrl" "https://<id>.ngrok-free.app"

# Mailtrap SMTP
dotnet user-secrets set "Email:Host" "sandbox.smtp.mailtrap.io"
dotnet user-secrets set "Email:Port" "2525"
dotnet user-secrets set "Email:Username" "<MAILTRAP_USERNAME>"
dotnet user-secrets set "Email:Password" "<MAILTRAP_PASSWORD>"
dotnet user-secrets set "Email:FromName" "TownTrek Dev"
dotnet user-secrets set "Email:FromAddress" "no-reply@dev.towntrek.local"
dotnet user-secrets set "Email:UseStartTls" "true"
dotnet user-secrets set "Email:UseSsl" "false"

# PayFast sandbox
dotnet user-secrets set "PayFast:MerchantId" "10040964"
dotnet user-secrets set "PayFast:MerchantKey" "mieu9ydfgtqo4"
dotnet user-secrets set "PayFast:PassPhrase" ""            # keep blank OR match dashboard
dotnet user-secrets set "PayFast:PaymentUrl" "https://sandbox.payfast.co.za/eng/process"
dotnet user-secrets set "PayFast:IsLive" "false"
dotnet user-secrets set "PayFast:Environment" "sandbox"

# Some accounts expect encoded signature values
dotnet user-secrets set "PayFast:UseEncodedSignature" "true"
```

Notes
- If your PayFast sandbox uses a passphrase, set the exact same string in user‑secrets and on the PayFast dashboard; otherwise keep both empty.
- `BaseUrl` in `appsettings.json` is left empty; code derives from the current request when not supplied.

## Difficulties we faced and fixes

- 400 Bad Request from PayFast (localhost URLs)
  - Fix: use ngrok public URL; update `BaseUrl` and regenerate link/signature
- Signature mismatches
  - Fixes: sort keys A→Z; exclude `signature` and empty values; append `&passphrase=...` last (raw) when set; add `PayFast:UseEncodedSignature` toggle; keep `BaseUrl` stable
- Manual URL edits
  - Fix: use server‑side POST at `GET /Api/Payment/Process?subscriptionId={id}`
- Email templates failing outside MVC
  - Fix: supply full `ActionContext` in renderer; use absolute layout path
- SMTP invalid credentials
  - Fix: set correct Mailtrap credentials in secrets
- Attribute‑routes 404
  - Fix: `app.MapControllers()` and proper `[Route]` usage

## Issues corrected

- Removed hardcoded localhost fallbacks and read `BaseUrl` from config or request
- Corrected passphrase handling and signature building
- Added missing views and opened auth endpoints where appropriate

## Safe improvements

- Prefer server‑side auto‑post to PayFast
- Keep dev diagnostics (disable in prod):
  - `/Api/Payment/DebugFields`, `/Api/Payment/DebugFieldsByPayment`
  - `/Api/Payment/NotifyDev`
  - `/Api/Email/Test*`

## Integration flow

1) Register Business Owner → user + pending subscription created; welcome + confirmation emails sent
2) `/Api/Payment/Process?subscriptionId={id}` → POST to PayFast
3) PayFast returns to `return_url`; IPN hits `notify_url`
4) IPN COMPLETE → activate subscription, assign role, send success email
5) User clicks confirmation link → `EmailConfirmed=1`

## Sandbox details

- Email: Mailtrap Email Testing inbox
- PayFast: sandbox merchant creds
- BaseUrl: ngrok public URL
- TLS: STARTTLS on 2525 (UseStartTls=true, UseSsl=false)

## Test via Postman / curl

```bash
curl -X POST "https://<id>.ngrok-free.app/Api/Payment/Notify" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  --data "m_payment_id=33&payment_status=COMPLETE&token=DEV-TOKEN"
```

Dev helpers:
```
GET /Api/Payment/NotifyDev?paymentId=33
GET /Api/Email/TestWelcome?to=<mail>
GET /Api/Email/TestPaymentSuccess?to=<mail>
```

## Sample test URLs (replace <id> and parameters)

- PayFast auto‑post (server‑side):
  - `https://<id>.ngrok-free.app/Api/Payment/Process?subscriptionId=33`
- Email confirmation flow:
  - Resend: `https://<id>.ngrok-free.app/Auth/ResendConfirmation?email=tester@example.com`
  - Confirm: link from Mailtrap points to `/Auth/ConfirmEmail?userId={guid}&token={token}`
- Direct email tests (dev only):
  - Welcome: `https://<id>.ngrok-free.app/Api/Email/TestWelcome?to=tester@example.com&firstName=Tester`
  - Payment success: `https://<id>.ngrok-free.app/Api/Email/TestPaymentSuccess?to=tester@example.com`

## Verifying signature locally

```powershell
$raw = 'amount=199.00&cancel_url=https://<id>.ngrok-free.app/Api/Payment/Cancel&email_address=tester@example.com&item_name=TownTrek-Subscription&m_payment_id=33&merchant_id=10040964&merchant_key=mieu9ydfgtqo4&notify_url=https://<id>.ngrok-free.app/Api/Payment/Notify&return_url=https://<id>.ngrok-free.app/Api/Payment/Success?paymentId=33'
$md5 = [System.Security.Cryptography.MD5]::Create()
([BitConverter]::ToString($md5.ComputeHash([Text.Encoding]::UTF8.GetBytes($raw))).Replace('-','')).ToLower()
```

If your PayFast account expects encoded signature values, compute the hash on the URL‑encoded values (spaces as `+`). Toggle via `PayFast:UseEncodedSignature`.

## Logging & diagnostics

- Email sends log: `Email sent to {Email} with subject ...` (Information)
- Failures are logged with warnings/exceptions in `Services/PaymentService.cs` and `Services/EmailService.cs`
- Use the debug endpoints to inspect payloads and isolate problems, then disable in production

## Production checklist

- BaseUrl → production domain; update `AllowedHosts`
- PayFast → live creds + `https://www.payfast.co.za/eng/process`; align passphrase; verify signature mode
- SMTP → production provider; configure SPF/DKIM/DMARC
- Security → remove/restrict dev endpoints; enforce HTTPS
- Monitoring → log email send and IPN outcomes; alert on failures

## Items to remove/restrict before prod

- `/Api/Payment/NotifyDev`, `/Api/Payment/DebugFields*`, `/Api/Email/Test*`

## Troubleshooting

- 400 from PayFast → public BaseUrl + regenerate link
- Signature mismatch → passphrase alignment; toggle `UseEncodedSignature`; verify sorted keys & stable BaseUrl
- No IPN → keep tunnel alive; use Dev Notify helper
- No emails → verify SMTP secrets; test via `/Api/Email/Test*`; check renderer and layout path

## Key files

- `Options/EmailOptions.cs`, `Options/PayFastOptions.cs`
- `Program.cs` – options binding, `app.MapControllers()`
- `Services/EmailService.cs`, `Services/EmailTemplateRenderer.cs`
- `Views/Emails/*`, `Views/Payment/AutoPost.cshtml`
- `Services/RegistrationService.cs`, `Services/PaymentService.cs`
- `Controllers/Api/PaymentController.cs`, `Controllers/Api/EmailController.cs`
- `Controllers/Auth/AuthController.cs`
