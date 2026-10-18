# Phase 3.4: Device identity verification

The local companion service now exposes POST /local/auth/verify with a deviceId and token. The server hashes the provided token and compares it to the stored SHA-256 digest using a constant-time comparison. Unknown or revoked devices are rejected.

This is a loopback-only protocol step, not production Android authentication. The raw token is submitted in JSON over HTTP and MUST NOT be used over LAN. Pairing approval still uses a developer console and is not an OS-enforced trust boundary.

Next steps: TLS with pinned certificate, one-time Windows approval UI, per-device permission grants, authenticated session protocol, and Android client. Remote command endpoints remain disabled.
