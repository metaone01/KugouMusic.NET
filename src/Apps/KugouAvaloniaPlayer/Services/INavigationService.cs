using System;
using KugouAvaloniaPlayer.ViewModels;

namespace KugouAvaloniaPlayer.Services;

public interface INavigationService
{
    PageViewModelBase? CurrentPage { get; }
    bool CanGoBack { get; }

    event Action<PageViewModelBase?>? CurrentPageChanged;

    void NavigateRoot(PageViewModelBase page);
    void Navigate(PageViewModelBase page);
    bool GoBack();
}
