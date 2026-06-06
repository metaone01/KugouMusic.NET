using System;
using System.IO;

namespace KugouAvaloniaPlayer.Converters;

internal static class LocalImageSourceHelper
{
    public static string? GetLocalFilePath(string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return null;

        if (Uri.TryCreate(source, UriKind.Absolute, out var uri))
        {
            return uri.IsFile && File.Exists(uri.LocalPath)
                ? uri.LocalPath
                : null;
        }

        return File.Exists(source) ? source : null;
    }
}

