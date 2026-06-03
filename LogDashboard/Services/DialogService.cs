using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace LogDashboard.Services;

public class DialogService : IDialogService
{
    public async Task<string?> OpenFolderAsync(string title = "選擇資料夾")
    {
        var mainWindow = (Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        if (mainWindow is null) return null;

        var topLevel = TopLevel.GetTopLevel(mainWindow);
        if (topLevel is null) return null;

        var results = await topLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false
            });

        return results.Count > 0 ? results[0].Path.LocalPath : null;
    }
}