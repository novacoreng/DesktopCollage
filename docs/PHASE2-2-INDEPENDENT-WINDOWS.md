# Phase 2.2: Independent collection windows

Implemented in source:
- Each saved collection can open its own borderless translucent WPF window.
- Each window has a distinct title, drag-to-move header and native resizing support.
- Each collection stores its own geometry and file/folder references in workspace-v1.json.
- Dragging files and folders into a collection records references, not file moves.
- Double-click opens the original file or folder.
- Collection manager can create, rename, open and remove collections.
- Duplicate windows for the same collection are prevented while the manager remains open.

Limitations:
- This is NOT native Windows desktop embedding.
- Multiple collection windows are not automatically restored at startup.
- Reopening the manager currently creates a separate manager instance and registry. Windows remain open when the manager closes, but this can lead to duplicate windows and stale state. A shared app-level workspace controller is required next.
- Windows runtime interaction and mixed-DPI behavior remain unverified.

Manual tests:
1. Create two collections, move and resize them separately.
2. Drop different files into each collection, restart and reopen.
3. Rename and delete one collection, ensure original files remain untouched.
4. Confirm the Windows CI build and then perform real Windows interaction tests.
