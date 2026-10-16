# Phase 2.1 Collection Manager

Added a collection manager window with create, rename, delete and JSON workspace persistence at %LOCALAPPDATA%/DesktopCollage/workspace-v1.json. The manager is separate from the original single-window prototype.

## Not yet implemented
- Opening each collection as an independent desktop surface.
- Dragging existing items into individual managed collections.
- Moving/resizing each managed collection and saving that position.
- Per-collection layout locks and icon reflow.

## Manual checks
1. Open Collections from the prototype.
2. Create, rename and remove a collection.
3. Restart application and confirm the collection list persists.
4. Confirm removing a collection never deletes source files.
5. Verify no file mutation is performed when creating or renaming collections.

Build verification required on Windows GitHub Actions.
