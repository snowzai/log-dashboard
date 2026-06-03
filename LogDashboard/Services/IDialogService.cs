using System.Threading.Tasks;

namespace LogDashboard.Services;

public interface IDialogService
{
    Task<string?> OpenFolderAsync(string title = "選擇資料夾");
}