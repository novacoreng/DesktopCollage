# Phase 2.7: Workspace recovery and startup restoration

On startup, the workspace controller restores collections marked RestoreOnStartup. New and legacy collection records default to true. Opening a collection marks it for future restoration, and closing one marks it as not scheduled for restoration. The controller suppresses that change during dispatcher shutdown.

WorkspaceStore validates the workspace JSON and attempts to read the .bak file if the primary workspace is malformed or unsupported. Recovery does not overwrite the original file at load time. A later explicit change will save the recovered data. If both files fail to load, the controller shows an error and starts with an empty in-memory workspace; the existing disk files remain until a save.

Limitations: the backup is the previous saved state, not a full history. Window visibility persistence during shutdown still needs Windows runtime testing. No file or shortcut originals are modified.
