using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LogDashboard.Services;
using LogDashboard.ViewModels;
using LogDashboard.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

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
            var loadingWindow = new LoadingWindow();
            desktop.MainWindow = loadingWindow;
            loadingWindow.Show();

            var themeService = Program.Services.GetRequiredService<ThemeService>();
            themeService.Initialize();

            // 回到 UI thread 建立並顯示 MainWindow
            var mainWindow = Program.Services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = Program.Services.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = mainWindow;
            mainWindow.Show();
            loadingWindow.Close();
        }

        base.OnFrameworkInitializationCompleted();
    }
}