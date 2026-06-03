using Avalonia;
using Avalonia.Styling;
using LogDashboard.Models;

namespace LogDashboard.Services;

public class ThemeService
{
    private readonly SettingsService _settingsService;

    public ThemeService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public AppTheme CurrentTheme =>
        Application.Current?.RequestedThemeVariant == ThemeVariant.Dark
            ? AppTheme.Dark
            : AppTheme.Light;

    public void Initialize()
    {
        var settings = _settingsService.Load();

        ApplyTheme(settings.Theme);
    }

    public void ToggleTheme()
    {
        SetTheme(
            CurrentTheme == AppTheme.Dark
                ? AppTheme.Light
                : AppTheme.Dark);
    }

    public void SetTheme(AppTheme theme)
    {
        ApplyTheme(theme);

        var settings = _settingsService.Load();

        settings.Theme = theme;

        _settingsService.Save(settings);
    }

    public bool IsDarkTheme()
    {
        return CurrentTheme == AppTheme.Dark;
    }

    private static void ApplyTheme(AppTheme theme)
    {
        if (Application.Current is null)
            return;

        Application.Current.RequestedThemeVariant =
            theme switch
            {
                AppTheme.Dark => ThemeVariant.Dark,
                _ => ThemeVariant.Light
            };
    }
}