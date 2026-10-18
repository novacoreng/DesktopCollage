# Phase 3.6: TLS transport identity foundation

The companion service now creates a persistent RSA 3072-bit self-signed TLS certificate and listens on HTTPS loopback port 47832. Its SHA-256 certificate fingerprint is available in the /health response. This is an initial transport identity, not production-grade Android-to-Windows secure pairing.

IMPORTANT: The existing Windows pairing review client and pairing console still use HTTP loopback port 47831. They must be migrated to HTTPS with explicit certificate pinning before they can work with the new service. No Android client is connected yet. The certificate has localhost and 127.0.0.1 SAN entries only. No LAN binding, mutual authentication, or remote commands are enabled.

Follow-up work: migrate local clients to HTTPS using explicit certificate pinning, implement authenticated LAN endpoints with OS user approval, provision device identity securely, handle certificate rotation, and test Windows certificate file permissions. Do not bypass certificate verification globally.
