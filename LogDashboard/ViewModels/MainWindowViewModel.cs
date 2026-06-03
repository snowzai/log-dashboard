using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LogDashboard.Models;
using LogDashboard.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LogDashboard.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;
        private readonly ThemeService _themeService;
        private readonly LogParserService _logParserService;
        private readonly FolderWatcherService _folderWatcherService;
        private readonly IDialogService _dialogService;

        private readonly List<LogEntry> _allLogs = [];

        private const string NoFolderSelected = "尚未選擇資料夾";

        public MainWindowViewModel(
    SettingsService settingsService,
    ThemeService themeService,
    LogParserService logParserService,
    FolderWatcherService folderWatcherService,
    IDialogService dialogService)
        {
            _settingsService = settingsService;
            _themeService = themeService;
            _logParserService = logParserService;
            _folderWatcherService = folderWatcherService;
            _dialogService = dialogService;

            LogLevels =
            [
                "All",
                "Fatal",
                "Error",
                "Warning",
                "Information",
                "Debug",
                "Verbose"
            ];

            SelectedRefreshInterval = RefreshIntervals[1]; // 預設 10 秒

            _folderWatcherService.OnChanged += async () =>
                await LoadLogsAsync();

            LoadSettings();
        }

        private void LoadSettings()
        {
            var settings = _settingsService.Load();

            if (!string.IsNullOrWhiteSpace(settings.LastFolder))
            {
                CurrentFolder = settings.LastFolder;
                StartWatcher();
                _ = LoadLogsAsync();
            }
            else
            {
                CurrentFolder = NoFolderSelected;
            }
        }

        #region Folder

        [ObservableProperty]
        private string currentFolder = string.Empty;

        #endregion

        #region Search

        [ObservableProperty]
        private string searchText = string.Empty;

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilter();
        }

        #endregion

        #region Level Filter

        [ObservableProperty]
        private string selectedLevel = "All";

        partial void OnSelectedLevelChanged(string value)
        {
            ApplyFilter();
        }

        public List<string> LogLevels { get; }

        #endregion

        #region Statistics

        [ObservableProperty]
        private int totalCount;

        [ObservableProperty]
        private int fatalCount;

        [ObservableProperty]
        private int errorCount;

        [ObservableProperty]
        private int warningCount;

        [ObservableProperty]
        private int informationCount;

        [ObservableProperty]
        private int debugCount;

        [ObservableProperty]
        private int verboseCount;

        #endregion

        #region Time Filter

        public List<string> TimeRanges { get; } =
            ["All", "1h", "4h", "12h", "1d", "7d", "1mon"];

        [ObservableProperty]
        private string selectedTimeRange = "1h";

        partial void OnSelectedTimeRangeChanged(string value)
        {
            ApplyFilter();
        }

        private DateTime? GetTimeRangeStart() => SelectedTimeRange switch
        {
            "1h" => DateTime.Now.AddHours(-1),
            "4h" => DateTime.Now.AddHours(-4),
            "12h" => DateTime.Now.AddHours(-12),
            "1d" => DateTime.Now.AddDays(-1),
            "7d" => DateTime.Now.AddDays(-7),
            "1mon" => DateTime.Now.AddMonths(-1),
            _ => null
        };

        #endregion

        #region Logs

        public ObservableCollection<LogEntry> FilteredLogs { get; }
            = [];

        #endregion

        #region Charts Placeholder

        [ObservableProperty]
        private object? pieChartPlaceholder;

        [ObservableProperty]
        private object? timelineChartPlaceholder;

        #endregion

        #region Selection

        [ObservableProperty]
        private LogEntry? selectedLog;

        #endregion

        #region Charts

        [ObservableProperty]
        private ISeries[] pieSeries = [];

        [ObservableProperty]
        private ISeries[] timelineSeries = [];

        [ObservableProperty]
        private Axis[] timelineXAxes = [new Axis { Labels = [] }];

        #endregion

        #region Exception Tab

        public ObservableCollection<LogEntry> ExceptionLogs { get; } = [];

        [ObservableProperty]
        private int exceptionCount;

        #endregion

        #region Date Range Filter

        [ObservableProperty]
        private DateTime? startDate;

        [ObservableProperty]
        private TimeSpan? startTime;

        [ObservableProperty]
        private DateTime? endDate;

        [ObservableProperty]
        private TimeSpan? endTime;

        // 有設定自訂日期時，停用時間範圍下拉
        public bool IsTimeRangeEnabled => !StartDate.HasValue && !EndDate.HasValue;

        private DateTime? CustomStart =>
            StartDate.HasValue
                ? StartDate.Value.Date + (StartTime ?? TimeSpan.Zero)
                : null;

        private DateTime? CustomEnd =>
            EndDate.HasValue
                ? EndDate.Value.Date + (EndTime ?? new TimeSpan(23, 59, 59))
                : null;

        partial void OnStartDateChanged(DateTime? value) { OnPropertyChanged(nameof(IsTimeRangeEnabled)); ApplyFilter(); }
        partial void OnStartTimeChanged(TimeSpan? value) => ApplyFilter();
        partial void OnEndDateChanged(DateTime? value) { OnPropertyChanged(nameof(IsTimeRangeEnabled)); ApplyFilter(); }
        partial void OnEndTimeChanged(TimeSpan? value) => ApplyFilter();

        [RelayCommand]
        private void ClearDateRange()
        {
            StartDate = null;
            StartTime = null;
            EndDate = null;
            EndTime = null;
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task SelectFolder()
        {
            try
            {
                var folderPath = await _dialogService.OpenFolderAsync();
                if (folderPath is null) return;

                CurrentFolder = folderPath;
                _settingsService.SaveLastFolder(folderPath);
                StartWatcher();
                await LoadLogsAsync();
            }
            catch (Exception ex)
            {
                File.WriteAllText("crash.log", ex.ToString());
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            await LoadLogsAsync();
        }

        [RelayCommand]
        private void ToggleTheme()
        {
            _themeService.ToggleTheme();
        }

        #endregion

        #region Public Methods

        public async Task LoadLogsAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentFolder) ||
                CurrentFolder == NoFolderSelected)
                return;

            var logs = await _logParserService.ParseFolderAsync(CurrentFolder);

            await SetLogsAsync(logs);
        }

        #endregion

        #region Filtering

        private void ApplyFilter()
        {
            IEnumerable<LogEntry> query = _allLogs;

            // 自訂日期範圍優先；否則用時間範圍下拉
            if (CustomStart.HasValue || CustomEnd.HasValue)
            {
                if (CustomStart.HasValue)
                    query = query.Where(x => x.Timestamp >= CustomStart.Value);
                if (CustomEnd.HasValue)
                    query = query.Where(x => x.Timestamp <= CustomEnd.Value);
            }
            else
            {
                var since = GetTimeRangeStart();
                if (since.HasValue)
                    query = query.Where(x => x.Timestamp >= since.Value);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(x =>
                    (x.Message?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.Exception?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    x.Properties.Any(p => p.Value?.ToString()?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true));

            if (SelectedLevel != "All")
                query = query.Where(x =>
                    string.Equals(x.Level, SelectedLevel, StringComparison.OrdinalIgnoreCase));

            var filtered = query.ToList();

            FilteredLogs.Clear();
            foreach (var item in filtered)
                FilteredLogs.Add(item);

            // 統計從 filtered 計算，反映目前篩選條件
            UpdateStatistics(filtered);

            // Exception 頁籤
            var exceptions = filtered
                .Where(x => x.Level == "Error" || x.Level == "Fatal")
                .ToList();

            ExceptionLogs.Clear();
            foreach (var item in exceptions)
                ExceptionLogs.Add(item);

            ExceptionCount = ExceptionLogs.Count;
        }

        #endregion

        #region Statistics

        private void UpdateStatistics(List<LogEntry> source)
        {
            TotalCount = source.Count;
            ErrorCount = source.Count(x => x.Level == "Error");
            WarningCount = source.Count(x => x.Level == "Warning");
            InformationCount = source.Count(x => x.Level == "Information");
            DebugCount = source.Count(x => x.Level == "Debug");
            VerboseCount = source.Count(x => x.Level == "Verbose");

            UpdateCharts(source);
        }

        private void UpdateCharts(List<LogEntry> source)
        {
            PieSeries =
            [
                new PieSeries<int> { Values = [FatalCount],       Name = "Fatal",       Fill = new SolidColorPaint(SKColor.Parse("#B00020")) },
                new PieSeries<int> { Values = [ErrorCount],       Name = "Error",       Fill = new SolidColorPaint(SKColor.Parse("#CF6679")) },
                new PieSeries<int> { Values = [WarningCount],     Name = "Warning",     Fill = new SolidColorPaint(SKColor.Parse("#FFCA28")) },
                new PieSeries<int> { Values = [InformationCount], Name = "Information", Fill = new SolidColorPaint(SKColor.Parse("#42A5F5")) },
                new PieSeries<int> { Values = [DebugCount],       Name = "Debug",       Fill = new SolidColorPaint(SKColor.Parse("#66BB6A")) },
                new PieSeries<int> { Values = [VerboseCount],     Name = "Verbose",     Fill = new SolidColorPaint(SKColor.Parse("#9E9E9E")) },
            ];

            var grouped = source
                // ✅ 先 group 再用 DateTime key 排序
                .GroupBy(x => new DateTime(x.Timestamp.Year, x.Timestamp.Month,
                                           x.Timestamp.Day, x.Timestamp.Hour, 0, 0))
                .OrderBy(g => g.Key)
                .Select(g => (Label: g.Key.ToString("MM/dd HH:00"), Count: g.Count()))
                .ToList();

            TimelineSeries =
            [
                new ColumnSeries<int>
                {
                    Values = grouped.Select(g => g.Count).ToArray(),
                    Fill   = new SolidColorPaint(SKColor.Parse("#42A5F5")),
                    Name   = "Count"
                }
            ];

            TimelineXAxes =
            [
                new Axis
                {
                    Labels         = grouped.Select(g => g.Label).ToArray(),
                    LabelsRotation = -45,
                    TextSize       = 10
                }
            ];
        }

        #endregion

        #region Future Integration

        public async Task SetLogsAsync(IEnumerable<LogEntry> logs)
        {
            _allLogs.Clear();
            _allLogs.AddRange(logs.OrderByDescending(x => x.Timestamp));

            ApplyFilter(); // UpdateStatistics 已在內部呼叫，不需要再呼叫一次

            await Task.CompletedTask;
        }

        #endregion

        [RelayCommand]
        private void ClearSelection()
        {
            SelectedLog = null;
        }

        public bool HasSelectedProperties =>
            SelectedLog?.Properties.Count > 0;

        partial void OnSelectedLogChanged(LogEntry? value)
        {
            OnPropertyChanged(nameof(HasSelectedProperties));
        }

        #region Watcher Control

        [ObservableProperty]
        private bool isWatching = true;

        public string WatcherButtonIcon => IsWatching ? "⏸" : "▶";
        public string WatcherButtonTooltip => IsWatching ? "暫停自動刷新" : "開始自動刷新";

        partial void OnIsWatchingChanged(bool value)
        {
            OnPropertyChanged(nameof(WatcherButtonIcon));
            OnPropertyChanged(nameof(WatcherButtonTooltip));

            if (value)
                StartWatcher();
            else
                _folderWatcherService.Stop();
        }

        [RelayCommand]
        private void ToggleWatcher()
        {
            IsWatching = !IsWatching;
        }

        private void StartWatcher()
        {
            if (string.IsNullOrWhiteSpace(CurrentFolder) ||
                CurrentFolder == NoFolderSelected) return;

            var interval = SelectedRefreshInterval.Interval;
            _folderWatcherService.Watch(CurrentFolder, interval);
        }

        #endregion

        #region Refresh Interval

        public record RefreshIntervalOption(string Label, TimeSpan Interval)
        {
            public override string ToString() => Label;
        }

        public List<RefreshIntervalOption> RefreshIntervals { get; } =
        [
            new("5 秒",  TimeSpan.FromSeconds(5)),
            new("10 秒", TimeSpan.FromSeconds(10)),
            new("30 秒", TimeSpan.FromSeconds(30)),
            new("1 分鐘", TimeSpan.FromMinutes(1)),
            new("5 分鐘", TimeSpan.FromMinutes(5)),
        ];

        [ObservableProperty]
        private RefreshIntervalOption selectedRefreshInterval;

        partial void OnSelectedRefreshIntervalChanged(RefreshIntervalOption value)
        {
            if (IsWatching)
                _folderWatcherService.ChangeInterval(value.Interval);
        }

        #endregion
    }
}
