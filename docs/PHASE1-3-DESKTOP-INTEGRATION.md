# Phase 1.3: Explorer desktop feasibility

## Scope completed in source
- Added read-only Shell hierarchy inspection using FindWindow, FindWindowEx, EnumWindows and GetClassName.
- Added an explicit **Inspect Explorer** diagnostic control.
- Added a bottom-right Thumb resize control that does not depend on native non-client resize borders.
- Preserved non-destructive drag-and-drop references and layout persistence.

## What is NOT implemented
- Embedding a collection behind Explorer's desktop icons.
- Native desktop icon interception or hiding.
- Wallpaper-aware blur.
- A supported, stable desktop window parenting solution.
- Windows 10/11 runtime verification.

## Safety decisions
The prototype deliberately does not use SetParent against Progman/WorkerW or undocumented Shell messages. WorkerW window topology can change across Windows versions and Explorer restarts. The inspector only reports observed window classes and handles, and must not be used as a guarantee of a supported desktop host.

## Manual verification
1. Run the prototype on Windows 10 and Windows 11.
2. Select Inspect Explorer. Confirm it returns without crashing and records observed hierarchy.
3. Drag label, resize from bottom-right, and drop a shortcut.
4. Restart Explorer, then repeat inspection.
5. Test Show Desktop, Win+D, Alt+Tab, virtual desktops and mixed-DPI monitors.
6. Capture screenshots and results before deciding whether a desktop-level companion surface is viable.

## Gate
Phase 1 remains incomplete until tested on Windows. No production desktop embedding claims should be made.
