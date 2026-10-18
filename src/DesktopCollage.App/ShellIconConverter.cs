using System;
using System.Drawing;
using System.IO;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;

namespace DesktopCollage;

/// <summary>Gets the actual shell icon for an existing path without changing its contents.</summary>
internal sealed class ShellIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not string path || (!File.Exists(path) && !Directory.Exists(path)))
            return System.Windows.DependencyProperty.UnsetValue;
        try
        {
            using var icon = Icon.ExtractAssociatedIcon(path);
            if (icon is null) return System.Windows.DependencyProperty.UnsetValue;
            var image = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(48, 48));
            image.Freeze();
            return image;
        }
        catch (Exception)
        {
            return System.Windows.DependencyProperty.UnsetValue;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => throw new NotSupportedException();
}
