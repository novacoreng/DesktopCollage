using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DesktopCollage;

/// <summary>
/// Experimental bottom-of-Z-order companion mode. Does not attach to Explorer,
/// intercept desktop icons, or guarantee placement below Explorer's desktop view.
/// </summary>
internal static class DesktopCompanionMode
{
    private static readonly nint HwndBottom = new(1);
    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoActivate = 0x0010;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter,
        int x, int y, int cx, int cy, uint flags);

    public static bool SendToBottom(Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;
        return handle != 0 && SetWindowPos(handle, HwndBottom, 0, 0, 0, 0,
            SwpNoMove | SwpNoSize | SwpNoActivate);
    }
}
