using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using LogDashboard.ViewModels;
using LogDashboard.Views;
using LogDashboard.Services;

namespace LogDashboard;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var themeService =
                Program.Services.GetRequiredService<ThemeService>();

            themeService.Initialize();

            var mainWindow =
                Program.Services.GetRequiredService<MainWindow>();

            mainWindow.DataContext =
                Program.Services.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}