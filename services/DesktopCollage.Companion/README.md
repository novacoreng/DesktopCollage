# DesktopCollage companion service

Local-only development foundation for Android pairing. Run with:

```powershell
dotnet run --project services/DesktopCollage.Companion
```

Local endpoints:
- GET /health
- GET /api/v1/capabilities
- POST /local/pairing/start with JSON `{"name":"My Android phone"}`
- POST /local/pairing/approve with JSON `{"requestId":"...","code":"..."}`
- GET /local/devices
- DELETE /local/devices/{id}

All pairing management endpoints are bound to 127.0.0.1 only. They are NOT remotely accessible from Android. They are not a production pairing protocol. A locally running process can request enrollment and approve it, so OS-level approval UI and authenticated remote transport are still required before any LAN exposure. Do not expose these endpoints on a public or LAN interface.

Tokens are generated using a cryptographic RNG and stored as hashes in %LOCALAPPDATA%/DesktopCollage/paired-devices-v1.json. Device revocation removes the stored record. Tokens do not authorize any commands yet.
