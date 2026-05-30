using System;
using System.Collections.Generic;
using KugouAvaloniaPlayer.ViewModels;

namespace KugouAvaloniaPlayer.Services;

public sealed class NavigationService : INavigationService
{
    private const int MaxHistoryDepth = 4;
    private readonly Stack<PageViewModelBase> _stack = new();

    public PageViewModelBase? CurrentPage => _stack.Count > 0 ? _stack.Peek() : null;

    public bool CanGoBack => _stack.Count > 1;

    public event Action<PageViewModelBase?>? CurrentPageChanged;

    public void NavigateRoot(PageViewModelBase page)
    {
        _stack.Clear();
        _stack.Push(page);
        CurrentPageChanged?.Invoke(CurrentPage);
    }

    public void Navigate(PageViewModelBase page)
    {
        if (CurrentPage == page)
            return;

        _stack.Push(page);
        TrimHistory();
        CurrentPageChanged?.Invoke(CurrentPage);
    }

    public bool GoBack()
    {
        if (!CanGoBack)
            return false;

        _stack.Pop();
        CurrentPageChanged?.Invoke(CurrentPage);
        return true;
    }

    private void TrimHistory()
    {
        if (_stack.Count <= MaxHistoryDepth)
            return;

        var newestToOldest = _stack.ToArray();
        _stack.Clear();

        for (var i = MaxHistoryDepth - 1; i >= 0; i--)
            _stack.Push(newestToOldest[i]);
    }
}
