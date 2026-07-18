using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using ATL;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace KugouAvaloniaPlayer.Converters;

public sealed class LocalCachedBitmapConverter : IValueConverter
{
    private const int DefaultDecodeWidth = 96;
    private const int MaxCacheEntries = 256;
    private const string OriginalSizeParameter = "Original";
    private const string DefaultSongCover = "avares://KugouAvaloniaPlayer/Assets/default_song.png";
    private static readonly Lock CacheGate = new();
    private static readonly Dictionary<string, LinkedListNode<CacheEntry>> Cache = new(StringComparer.Ordinal);
    private static readonly LinkedList<CacheEntry> CacheUsage = [];
    private static readonly ConcurrentDictionary<int, Lazy<Bitmap>> DefaultBitmapCache = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var decodeOriginalSize = IsOriginalSizeRequested(parameter);
        var decodeWidth = ResolveDecodeWidth(parameter);
        var source = value as string;
        if (string.IsNullOrWhiteSpace(source))
            return GetDefaultBitmap(decodeWidth, decodeOriginalSize);

        var cacheKey = BuildCacheKey(source, decodeWidth, decodeOriginalSize);

        if (TryGetCachedBitmap(cacheKey, out var cached))
            return cached;

        if (LocalImageSourceHelper.TryGetEmbeddedCoverFilePath(source, out var embeddedTrackPath))
        {
            var bitmap = TryDecodeEmbeddedCoverBitmap(embeddedTrackPath!, decodeWidth, decodeOriginalSize);
            if (bitmap != null)
                CacheBitmap(cacheKey, bitmap);
            else
                RemoveCachedBitmap(cacheKey);

            return bitmap ?? GetDefaultBitmap(decodeWidth, decodeOriginalSize);
        }

        var path = LocalImageSourceHelper.GetLocalFilePath(source);
        if (path == null)
            return GetDefaultBitmap(decodeWidth, decodeOriginalSize);

        try
        {
            using var stream = File.OpenRead(path);
            var bitmap = DecodeBitmap(stream, decodeWidth, decodeOriginalSize);
            CacheBitmap(cacheKey, bitmap);
            return bitmap;
        }
        catch
        {
            RemoveCachedBitmap(cacheKey);
            return GetDefaultBitmap(decodeWidth, decodeOriginalSize);
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static Bitmap? TryDecodeEmbeddedCoverBitmap(string trackPath, int decodeWidth, bool decodeOriginalSize)
    {
        try
        {
            var track = new Track(trackPath);
            var picture = track.EmbeddedPictures.Count > 0 ? track.EmbeddedPictures[0] : null;
            if (picture?.PictureData == null || picture.PictureData.Length == 0)
                return null;

            using var stream = new MemoryStream(picture.PictureData, writable: false);
            return DecodeBitmap(stream, decodeWidth, decodeOriginalSize);
        }
        catch
        {
            return null;
        }
    }

    private static int ResolveDecodeWidth(object? parameter)
    {
        if (parameter is int width && width > 0)
            return width;

        if (parameter is string s &&
            int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) &&
            parsed > 0)
        {
            return parsed;
        }

        return DefaultDecodeWidth;
    }

    private static bool IsOriginalSizeRequested(object? parameter)
    {
        return parameter is string value &&
               string.Equals(value, OriginalSizeParameter, StringComparison.OrdinalIgnoreCase);
    }

    private static Bitmap DecodeBitmap(Stream stream, int decodeWidth, bool decodeOriginalSize)
    {
        return decodeOriginalSize
            ? new Bitmap(stream)
            : Bitmap.DecodeToWidth(stream, decodeWidth, BitmapInterpolationMode.LowQuality);
    }

    private static string BuildCacheKey(string source, int decodeWidth, bool decodeOriginalSize)
    {
        return decodeOriginalSize
            ? source + "|original"
            : source + "|w=" + decodeWidth.ToString(CultureInfo.InvariantCulture);
    }

    private static bool TryGetCachedBitmap(string cacheKey, out Bitmap? bitmap)
    {
        lock (CacheGate)
        {
            if (!Cache.TryGetValue(cacheKey, out var node))
            {
                bitmap = null;
                return false;
            }

            if (!node.Value.Bitmap.TryGetTarget(out bitmap))
            {
                Cache.Remove(cacheKey);
                CacheUsage.Remove(node);
                return false;
            }

            CacheUsage.Remove(node);
            CacheUsage.AddFirst(node);
            return true;
        }
    }

    private static void CacheBitmap(string cacheKey, Bitmap bitmap)
    {
        lock (CacheGate)
        {
            if (Cache.Remove(cacheKey, out var existingNode))
                CacheUsage.Remove(existingNode);

            var entry = new CacheEntry(cacheKey, new WeakReference<Bitmap>(bitmap));
            Cache[cacheKey] = CacheUsage.AddFirst(entry);

            while (Cache.Count > MaxCacheEntries && CacheUsage.Last is { } oldestNode)
            {
                Cache.Remove(oldestNode.Value.Key);
                CacheUsage.RemoveLast();
            }
        }
    }

    private static void RemoveCachedBitmap(string cacheKey)
    {
        lock (CacheGate)
        {
            if (Cache.Remove(cacheKey, out var node))
                CacheUsage.Remove(node);
        }
    }

    private static Bitmap GetDefaultBitmap(int decodeWidth, bool decodeOriginalSize)
    {
        var cacheKey = decodeOriginalSize ? 0 : decodeWidth;
        return DefaultBitmapCache.GetOrAdd(cacheKey, width => new Lazy<Bitmap>(() =>
        {
            using var stream = AssetLoader.Open(new Uri(DefaultSongCover));
            return decodeOriginalSize
                ? new Bitmap(stream)
                : Bitmap.DecodeToWidth(stream, width, BitmapInterpolationMode.LowQuality);
        })).Value;
    }

    private sealed record CacheEntry(string Key, WeakReference<Bitmap> Bitmap);
}
