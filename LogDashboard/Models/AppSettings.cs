using LogDashboard.Models;

namespace LogDashboard.Models;

public class AppSettings
{
    /// <summary>
    /// 上次開啟的 Log 資料夾
    /// </summary>
    public string LastFolder { get; set; } = string.Empty;

    /// <summary>
    /// 使用者主題
    /// </summary>
    public AppTheme Theme { get; set; } = AppTheme.Dark;

    /// <summary>
    /// 是否遞迴搜尋子目錄
    /// </summary>
    public bool IncludeSubDirectories { get; set; } = true;

    /// <summary>
    /// 自動監控資料夾
    /// </summary>
    public bool AutoWatchFolder { get; set; } = true;

    /// <summary>
    /// 每頁顯示筆數
    /// </summary>
    public int PageSize { get; set; } = 100;
}