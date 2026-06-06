using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace KugouAvaloniaPlayer.Converters;

public sealed class LocalCachedBitmapConverter : IValueConverter
{
    private const int DecodeWidth = 96;
    private static readonly ConcurrentDictionary<string, WeakReference<Bitmap>> Cache = new(StringComparer.Ordinal);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var path = LocalImageSourceHelper.GetLocalFilePath(value as string);
        if (path == null)
            return null;

        if (Cache.TryGetValue(path, out var weakReference) &&
            weakReference.TryGetTarget(out var cached))
        {
            return cached;
        }

        try
        {
            using var stream = File.OpenRead(path);
            var bitmap = Bitmap.DecodeToWidth(stream, DecodeWidth, BitmapInterpolationMode.LowQuality);
            Cache[path] = new WeakReference<Bitmap>(bitmap);
            return bitmap;
        }
        catch
        {
            Cache.TryRemove(path, out _);
            return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

