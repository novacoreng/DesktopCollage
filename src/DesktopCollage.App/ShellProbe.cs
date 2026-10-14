using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DesktopCollage;

/// <summary>
/// Read-only Windows Explorer desktop hierarchy inspection.
/// Does not reparent windows, send undocumented messages, or alter Explorer.
/// </summary>
internal static partial class ShellProbe
{
    [LibraryImport("user32.dll", EntryPoint = "FindWindowW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint FindWindow(string? className, string? windowName);

    [LibraryImport("user32.dll", EntryPoint = "FindWindowExW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint FindWindowEx(nint parent, nint after, string? className, string? windowName);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnumWindows(EnumWindowsProc callback, nint data);

    private delegate bool EnumWindowsProc(nint hwnd, nint data);

    [LibraryImport("user32.dll", EntryPoint = "GetClassNameW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int GetClassName(nint hwnd, StringBuilder name, int maxCount);

    public static string GetReport()
    {
        var progman = FindWindow("Progman", null);
        var workers = new List<string>();
        var desktopHosts = new List<string>();
        EnumWindows((hwnd, _) =>
        {
            var name = new StringBuilder(256);
            GetClassName(hwnd, name, name.Capacity);
            if (name.ToString() == "WorkerW") workers.Add($"0x{hwnd.ToInt64():X}");
            var defView = FindWindowEx(hwnd, 0, "SHELLDLL_DefView", null);
            if (defView != 0) desktopHosts.Add($"0x{hwnd.ToInt64():X}");
            return true;
        }, 0);

        return $"Read-only Windows desktop diagnostics{Environment.NewLine}" +
               $"Progman found: {progman != 0}{Environment.NewLine}" +
               $"WorkerW windows: {workers.Count}{Environment.NewLine}" +
               $"SHELLDLL_DefView host(s): {desktopHosts.Count}{Environment.NewLine}" +
               $"Desktop host handles: {string.Join(", ", desktopHosts)}{Environment.NewLine}" +
               "This does NOT establish a supported desktop embedding mechanism.";
    }
}
