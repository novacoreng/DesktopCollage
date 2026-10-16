# Phase 2.3: Shared workspace controller

One process-wide controller now owns the collection records, the collection manager, and the registry of open collection windows.

- Reopening the manager activates the existing instance.
- Opening a collection twice activates its existing window.
- Renames update open window titles.
- Deletion closes the associated window before removing the collection record.
- All collection changes save through the same workspace store.
- Loading a corrupt or inaccessible workspace displays an error and avoids overwriting the existing file until a later explicit change.

Not yet implemented:
- Automatically restoring all collection windows at application startup.
- Safe transactional backup/recovery of workspace JSON.
- Native Explorer desktop embedding, multi-monitor placement validation, and lock controls.

Manual Windows verification and CI compilation remain necessary.
