# Phase 3: Windows companion architecture

## Initial implementation
A separate .NET 10 ASP.NET Core service project is introduced. It binds exclusively to 127.0.0.1:47831 and exposes health/capabilities information. All remote-control capabilities explicitly report disabled. The service does not accept remote commands, keyboard input, app launch requests, or device pairing.

## Security requirements before LAN access
1. Establish mutual device identity with Windows-side approval.
2. Encrypt and authenticate connections; reject unauthenticated requests.
3. Restrict remote launches to approved installed applications.
4. Maintain device revocation and reconnect behavior.
5. Rate-limit commands, bound message sizes, and validate every payload.
6. Require explicit confirmation for shutdown/sleep and other disruptive actions.
7. Do not expose an arbitrary command shell or privileged input mechanism.

## Android architecture
Build a React Native/Expo Android client after the secure pairing protocol is functional. Mobile UI should follow the uploaded WinDock reference: dark glass controls, app grid, keyboard, media, and volume.
