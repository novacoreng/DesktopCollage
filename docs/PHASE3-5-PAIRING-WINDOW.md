# Phase 3.5: Windows pairing review window

DesktopCollage now offers a native WPF pairing review window from its main interface. The UI displays local pending requests, accepts an eight-character code, asks for explicit confirmation, and can reject requests. It connects to the loopback-only companion service.

This is a local development UI, not a complete Android pairing experience. The service still permits local API calls and has no authenticated LAN transport. The approval response includes a secret token, but this UI intentionally does not display or persist it; therefore, it does not yet complete an Android device credential handoff.

Do not expose the service to a network until Windows-side access control, authenticated encrypted transport, and device identity confirmation are implemented.
