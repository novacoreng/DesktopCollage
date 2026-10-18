# Phase 3.3: Local pairing review prototype

A separate .NET console utility lists pending pairing requests and lets the person at the Windows PC approve or reject them. Approval requires typing the one-time code. The tool connects only to the companion service's loopback address.

## Development steps
1. Run the companion service.
2. POST a pairing request to /local/pairing/start from the Windows PC.
3. Run `dotnet run --project tools/DesktopCollage.PairingConsole`.
4. Choose a pending request, type APPROVE or REJECT, and enter the one-time code for approval.

## Important security limitations
This is a developer-only prototype, not a secure production approval boundary. Any local process can still call loopback endpoints, including approval. Enrollment secrets are printed to the console and must not be logged or shared. The Windows GUI approval dialog, OS access control, LAN TLS transport, and Android connection are not implemented. Do not expose the service to LAN or the internet.
