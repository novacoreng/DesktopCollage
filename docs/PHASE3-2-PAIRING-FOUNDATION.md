# Phase 3.2: Device enrollment foundation

The companion service now supports local-only enrollment requests, one-time codes with a three-minute expiry, cryptographically random device secrets, hashed token storage, device listing, and revocation.

The approval endpoint is intentionally loopback-only and not a secure human-approval workflow yet. It is not exposed to Android. Future development must add an actual Windows-side approval dialog, mutual TLS or equivalent authenticated transport, identity pinning, per-device permissions, rate limiting, and an audit trail before enabling remote control.

The service does not launch apps, inject keyboard input, or operate Windows controls. No cloud accounts are required.
