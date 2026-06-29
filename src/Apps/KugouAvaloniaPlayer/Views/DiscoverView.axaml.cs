using System;
using Avalonia;
using Avalonia.Controls;

namespace KugouAvaloniaPlayer.Views;

public partial class DiscoverView : UserControl
{
    public DiscoverView()
    {
        InitializeComponent();
    }

    private void OnTopCardsScrollLeft(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ScrollHorizontal(TopCardsScrollViewer, -1);
    }

    private void OnTopCardsScrollRight(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ScrollHorizontal(TopCardsScrollViewer, 1);
    }

    private void OnPlaylistsScrollLeft(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ScrollHorizontal(PlaylistsScrollViewer, -1);
    }

    private void OnPlaylistsScrollRight(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ScrollHorizontal(PlaylistsScrollViewer, 1);
    }

    private static void ScrollHorizontal(ScrollViewer scrollViewer, int direction)
    {
        var currentOffset = scrollViewer.Offset;
        var step = Math.Max(220, scrollViewer.Viewport.Width * 0.72);
        var maxOffsetX = Math.Max(0, scrollViewer.Extent.Width - scrollViewer.Viewport.Width);
        var targetX = Math.Clamp(currentOffset.X + direction * step, 0, maxOffsetX);

        scrollViewer.Offset = new Vector(targetX, currentOffset.Y);
    }
}
