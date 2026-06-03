using Avalonia.Threading;
using System;
using System.IO;

namespace LogDashboard.Services;

public class FolderWatcherService : IDisposable
{
    private DispatcherTimer? _timer;

    // ViewModel 訂閱這個 event
    public event Action? OnChanged;

    public void Watch(string folderPath, TimeSpan interval)
    {
        Stop();

        if (!Directory.Exists(folderPath)) return;

        _timer = new DispatcherTimer
        {
            Interval = interval
        };

        _timer.Tick += (_, _) => OnChanged?.Invoke();
        _timer.Start();
    }

    public void Stop()
    {
        _timer?.Stop();
        _timer = null;
    }

    // 只更新間隔，不重新載入資料
    public void ChangeInterval(TimeSpan interval)
    {
        if (_timer is null) return;
        _timer.Interval = interval;
    }

    public void Dispose() => Stop();
}