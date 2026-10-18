using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace DesktopCollage;

/// <summary>Uses the Windows Shell to display actual file, folder and shortcut icons.</summary>
internal sealed class ShellIconConverter : IValueConverter
{
    private const uint ShgfiIcon = 0x000000100;
    private const uint ShgfiLargeIcon = 0x000000000;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ShFileInfo
    {
        public nint Icon;
        public int IconIndex;
        public uint Attributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string DisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string TypeName;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "SHGetFileInfoW")]
    private static extern nint SHGetFileInfo(string path, uint attributes,
        ref ShFileInfo info, uint fileInfoSize, uint flags);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(nint icon);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string path || (!File.Exists(path) && !Directory.Exists(path)))
            return DependencyProperty.UnsetValue;

        var info = new ShFileInfo();
        try
        {
            var result = SHGetFileInfo(path, 0, ref info,
                (uint)Marshal.SizeOf<ShFileInfo>(), ShgfiIcon | ShgfiLargeIcon);
            if (result == 0 || info.Icon == 0) return DependencyProperty.UnsetValue;

            var image = Imaging.CreateBitmapSourceFromHIcon(info.Icon, Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(48, 48));
            image.Freeze();
            return image;
        }
        catch (Exception)
        {
            return DependencyProperty.UnsetValue;
        }
        finally
        {
            if (info.Icon != 0) DestroyIcon(info.Icon);
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
