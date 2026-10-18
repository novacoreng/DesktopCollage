# Phase 2.5 Icon grid

Collection windows now display a wrapping grid of file references instead of a text-only list. Each tile includes a label and an icon sourced from the actual path where supported.

Icon extraction uses System.Drawing.Icon.ExtractAssociatedIcon. Some file types, directories, and UWP apps may not yield their correct Explorer icons; those display a generic fallback tile. A complete SHGetFileInfo/IShellItemImageFactory integration is planned.

The grid wraps according to window width. Double click launches the original file/folder through ShellExecute. Drag and drop continues to add non-destructive references.

This is not yet a native desktop icon grid; runtime interaction requires Windows testing.
