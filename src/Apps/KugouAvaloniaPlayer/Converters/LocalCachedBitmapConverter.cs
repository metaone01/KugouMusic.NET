using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using ATL;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace KugouAvaloniaPlayer.Converters;

public sealed class LocalCachedBitmapConverter : IValueConverter
{
    private const int DecodeWidth = 96;
    private const string DefaultSongCover = "avares://KugouAvaloniaPlayer/Assets/default_song.png";
    private static readonly ConcurrentDictionary<string, WeakReference<Bitmap>> Cache = new(StringComparer.Ordinal);
    private static readonly Lazy<Bitmap> DefaultBitmap = new(() =>
    {
        using var stream = AssetLoader.Open(new Uri(DefaultSongCover));
        return Bitmap.DecodeToWidth(stream, DecodeWidth, BitmapInterpolationMode.LowQuality);
    });

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var source = value as string;
        if (string.IsNullOrWhiteSpace(source))
            return DefaultBitmap.Value;

        if (Cache.TryGetValue(source, out var weakReference) &&
            weakReference.TryGetTarget(out var cached))
        {
            return cached;
        }

        if (LocalImageSourceHelper.TryGetEmbeddedCoverFilePath(source, out var embeddedTrackPath))
        {
            var bitmap = TryDecodeEmbeddedCoverBitmap(embeddedTrackPath!);
            if (bitmap != null)
                Cache[source] = new WeakReference<Bitmap>(bitmap);
            else
                Cache.TryRemove(source, out _);

            return bitmap ?? DefaultBitmap.Value;
        }

        var path = LocalImageSourceHelper.GetLocalFilePath(source);
        if (path == null)
            return DefaultBitmap.Value;

        try
        {
            using var stream = File.OpenRead(path);
            var bitmap = Bitmap.DecodeToWidth(stream, DecodeWidth, BitmapInterpolationMode.LowQuality);
            Cache[source] = new WeakReference<Bitmap>(bitmap);
            return bitmap;
        }
        catch
        {
            Cache.TryRemove(source, out _);
            return DefaultBitmap.Value;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static Bitmap? TryDecodeEmbeddedCoverBitmap(string trackPath)
    {
        try
        {
            var track = new Track(trackPath);
            var picture = track.EmbeddedPictures.Count > 0 ? track.EmbeddedPictures[0] : null;
            if (picture?.PictureData == null || picture.PictureData.Length == 0)
                return null;

            using var stream = new MemoryStream(picture.PictureData, writable: false);
            return Bitmap.DecodeToWidth(stream, DecodeWidth, BitmapInterpolationMode.LowQuality);
        }
        catch
        {
            return null;
        }
    }
}
