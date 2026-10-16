# Phase 1.6: Desktop-mode recovery

## Change
The experimental bottom-Z-order mode now keeps the Windows taskbar entry. Previously, it hid the taskbar entry and immediately sent the window back to bottom on activation, making recovery difficult if Explorer obscured it.

Users can now activate the application from the taskbar and select **Normal window**. The local Ctrl+Shift+R shortcut also restores the window while it has keyboard focus.

## Limitations
This does not solve native desktop embedding or guarantee that the collection stays visible on top of the wallpaper. No Explorer parenting or shell hooks are used. The keyboard shortcut is application-local, not system-global.

## Windows acceptance checks
- Enter desktop mode, then recover using taskbar.
- Confirm the Restore button is reachable.
- Verify Ctrl+Shift+R while the window is focused.
- Test Win+D, Explorer restart, virtual desktops and mixed-DPI monitors.
- Confirm original desktop shortcuts remain untouched.
