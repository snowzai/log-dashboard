using LogDashboard.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace LogDashboard.Services;

public class LogParserService
{
    public async Task<List<LogEntry>> ParseFolderAsync(string folderPath)
    {
        var result = new List<LogEntry>();

        if (!Directory.Exists(folderPath))
            return result;

        var files = Directory.GetFiles(folderPath, "*.json",
            SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var entries = await ParseFileAsync(file);
            result.AddRange(entries);
        }

        return result;
    }

    public async Task<List<LogEntry>> ParseFileAsync(string filePath)
    {
        var result = new List<LogEntry>();

        if (!File.Exists(filePath))
            return result;

        // 用 FileShare.ReadWrite 避免與 Serilog 的寫入鎖衝突
        using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite);  // ← 關鍵

        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync()) is not null)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            try
            {
                var entry = ParseLine(trimmed);
                if (entry is not null)
                    result.Add(entry);
            }
            catch
            {
                // 略過無法解析的行
            }
        }

        return result;
    }

    private static LogEntry? ParseLine(string line)
    {
        using var doc = JsonDocument.Parse(line);
        var root = doc.RootElement;

        // @t = Timestamp
        var timestamp = root.TryGetProperty("@t", out var t)
            ? DateTime.Parse(t.GetString()!).ToLocalTime()
            : DateTime.MinValue;

        // @l = Level（省略時預設 Information）
        var level = root.TryGetProperty("@l", out var l)
            ? l.GetString() ?? "Information"
            : "Information";

        // @m = 已渲染的訊息；@mt = 訊息模板（fallback）
        var message = root.TryGetProperty("@m", out var m)
            ? m.GetString() ?? string.Empty
            : root.TryGetProperty("@mt", out var mt)
                ? mt.GetString() ?? string.Empty
                : string.Empty;

        // @x = Exception
        var exception = root.TryGetProperty("@x", out var x)
            ? x.GetString()
            : null;

        // 其餘非系統欄位當作 Properties
        var properties = new Dictionary<string, object?>();
        foreach (var prop in root.EnumerateObject())
        {
            if (prop.Name.StartsWith('@')) continue;

            properties[prop.Name] = prop.Value.ValueKind switch
            {
                JsonValueKind.String => prop.Value.GetString(),
                JsonValueKind.Number => prop.Value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => prop.Value.GetRawText()
            };
        }

        return new LogEntry
        {
            Timestamp = timestamp,
            Level = level,
            Message = message,
            Exception = exception,
            Properties = properties
        };
    }
}