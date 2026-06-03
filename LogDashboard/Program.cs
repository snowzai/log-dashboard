using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LogDashboard.Services;
using LogDashboard.ViewModels;
using LogDashboard.Views;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LogDashboard;

internal sealed class Program
{
    public static IServiceProvider Services { get; private set; } = null!;

    [STAThread]
    public static void Main(string[] args)
    {
        ConfigureServices();

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            File.WriteAllText("crash.log", e.ExceptionObject.ToString());
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            File.WriteAllText("crash.log", e.Exception.ToString());
            e.SetObserved();
        };

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
    }

    private static void ConfigureServices()
    {
        var services = new ServiceCollection();

        ConfigureLogging(services);

        RegisterServices(services);

        RegisterViewModels(services);

        RegisterViews(services);

        Services = services.BuildServiceProvider();
    }

    private static void ConfigureLogging(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();

            #if DEBUG
            builder.AddDebug();
            #endif

            builder.SetMinimumLevel(LogLevel.Information);
        });
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<SettingsService>();
        services.AddSingleton<ThemeService>();
        services.AddSingleton<LogParserService>();
        services.AddSingleton<FolderWatcherService>();
        services.AddSingleton<IDialogService, DialogService>();
    }

    private static void RegisterViewModels(IServiceCollection services)
    {
        services.AddSingleton<MainWindowViewModel>();
    }

    private static void RegisterViews(IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
    }
}