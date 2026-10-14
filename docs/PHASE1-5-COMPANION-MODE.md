# Phase 1.5: Experimental desktop companion mode

## Implemented
A user-controlled **Desktop mode (test)** button sends the collection window to the bottom of the normal Windows Z-order using SetWindowPos(HWND_BOTTOM). It also hides the window from the taskbar while this experimental mode is enabled. The button toggles back to normal window mode. On activation, the prototype requests bottom placement again.

## Limitations
This is **not native desktop embedding**. Explorer may display its icon surface over the prototype, hiding it. Behavior can differ by Windows version, Explorer restarts, virtual desktop, Show Desktop, and other shell transitions. No hooks or reparenting are used.

## Manual testing
1. Start in normal mode, verify drag and resize.
2. Select Desktop mode (test). Open another app and verify the prototype is behind it.
3. Confirm the prototype is still visible and usable on the desktop. Record whether icons cover it.
4. Test Win+D, Alt+Tab, Explorer restart, monitor disconnect/reconnect.
5. Return to Normal window before exiting if the prototype is difficult to locate.

If this experiment cannot meet the desktop requirements, implement a supported desktop companion alternative rather than forcing unsupported WorkerW parenting.
