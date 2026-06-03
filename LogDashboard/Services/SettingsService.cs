using DynamicData;
using LogDashboard.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace LogDashboard.Services;

public class SettingsService
{
    private readonly string _settingsFilePath;
    private AppSettings? _cache;

    private readonly JsonSerializerOptions _jsonOptions =
        new()
        {
            WriteIndented = true
        };

    public SettingsService()
    {
        var appDataFolder =
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData);

        var applicationFolder =
            Path.Combine(
                appDataFolder,
                "LogDashboard");

        Directory.CreateDirectory(applicationFolder);

        _settingsFilePath =
            Path.Combine(
                applicationFolder,
                "settings.json");
    }

    /// <summary>
    /// 取得設定檔位置
    /// </summary>
    public string SettingsFilePath => _settingsFilePath;

    public AppSettings Load() => _cache ??= LoadFromDisk();

    /// <summary>
    /// 載入設定
    /// </summary>
    private AppSettings LoadFromDisk()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                var defaultSettings = new AppSettings();

                Save(defaultSettings);  

                return defaultSettings;
            }

            var json =
                File.ReadAllText(_settingsFilePath);

            _cache = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            _cache = new AppSettings();
        }

        return _cache;
    }

    /// <summary>
    /// 非同步載入設定
    /// </summary>
    public async Task<AppSettings> LoadAsync()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                var defaultSettings = new AppSettings();

                await SaveAsync(defaultSettings);

                return defaultSettings;
            }

            var json =
                await File.ReadAllTextAsync(
                    _settingsFilePath);

            return JsonSerializer.Deserialize<AppSettings>(
                       json)
                   ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    /// <summary>
    /// 儲存設定
    /// </summary>
    public void Save(AppSettings settings)
    {
        var json =
            JsonSerializer.Serialize(
                settings,
                _jsonOptions);

        File.WriteAllText(
            _settingsFilePath,
            json);
    }

    /// <summary>
    /// 非同步儲存設定
    /// </summary>
    public async Task SaveAsync(AppSettings settings)
    {
        var json =
            JsonSerializer.Serialize(
                settings,
                _jsonOptions);

        await File.WriteAllTextAsync(
            _settingsFilePath,
            json);
    }

    /// <summary>
    /// 更新上次資料夾
    /// </summary>
    public void SaveLastFolder(string folder)
    {
        var settings = Load();

        settings.LastFolder = folder;

        Save(settings);
    }

    /// <summary>
    /// 更新主題
    /// </summary>
    public void SaveTheme(AppTheme theme)
    {
        var settings = Load();

        settings.Theme = theme;

        Save(settings);
    }

    /// <summary>
    /// 重設設定
    /// </summary>
    public void Reset()
    {
        Save(new AppSettings());
    }
}