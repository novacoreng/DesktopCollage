# DesktopCollage Windows companion service

An isolated ASP.NET Core (.NET 10) foundation for the planned Android-to-Windows connection.

## Run
```powershell
dotnet run --project services/DesktopCollage.Companion
```

Local endpoints:
- `http://127.0.0.1:47831/health`
- `http://127.0.0.1:47831/api/v1/capabilities`

The service binds to loopback only. No remote control endpoints, authentication, or Android pairing are enabled yet. It is not ready for LAN exposure. Do not change the bind address to 0.0.0.0 until transport security, pairing, authentication, device revocation and rate limiting have been implemented and tested.

Future phases: QR pairing with Windows confirmation, pinned device identities, authenticated commands, approved application catalog, optional LAN discovery, Android UI.
