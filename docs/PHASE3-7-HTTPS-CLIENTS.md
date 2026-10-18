# Phase 3.7: HTTPS migration of local clients

The Windows pairing review UI and developer console now connect to the companion service at https://127.0.0.1:47832. Each client reads the local service certificate and pins its SHA-256 fingerprint using a constant-time comparison. The old HTTP endpoint is no longer used by these clients.

The companion certificate file is stored at %LOCALAPPDATA%/DesktopCollage/companion/transport.pfx. This is a local developer convenience, not an appropriate trust bootstrap for an Android device. The PFX contains a private key, so Windows file ACLs must be audited before production use. The HTTPS callback verifies the pinned certificate fingerprint but does not enforce standard chain or hostname validation independently. Do not expose this service to LAN or the internet.

Remaining: automated integration tests, explicit OS-level pairing approval, authenticated LAN transport, device-specific credentials, and Android pairing.
