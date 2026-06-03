# Log Dashboard

一個 **日誌查看和分析桌面應用**，使用 C# 和 Avalonia 框架打造，幫助開發者快速檢視、篩選和分析 JSON 格式的日誌文件。

## ✨ 主要功能

### 📁 日誌加載與監控
- **資料夾選擇**：輕鬆選擇包含日誌文件的資料夾
- **遞迴掃描**：自動遞迴掃描子資料夾中的所有 JSON 日誌文件
- **實時監控**：支援自動刷新，及時檢測新增或修改的日誌文件
- **刷新控制**：可配置 5 秒至 5 分鐘的自動刷新間隔

### 🔍 日誌篩選與搜尋
- **多條件篩選**：
  - 按日誌級別篩選（Fatal、Error、Warning、Information、Debug、Verbose）
  - 按時間範圍篩選（預設支援 1 小時、4 小時、12 小時、1 天、7 天、1 個月）
  - 自訂日期時間範圍篩選
  - 全文搜尋（支援訊息、異常堆疊跟蹤和自訂屬性）

### 📊 統計與可視化
- **實時統計**：顯示各級別日誌的數量統計（總數、Fatal、Error 等）
- **圓餅圖**：視覺化顯示不同日誌級別的分布比例
- **時間線圖**：展示按小時統計的日誌數量變化趨勢

### 📋 日誌詳情檢視
- **日誌列表**：表格形式展示所有篩選結果
- **詳細信息**：點擊日誌項可查看完整詳情，包括：
  - 時間戳、日誌級別、訊息內容
  - 異常堆疊跟蹤（若存在）
  - 自訂屬性（Key-Value）
- **異常標籤**：單獨統計並展示 Error 和 Fatal 級別的日誌

### 🎨 用戶體驗
- **主題切換**：支援亮色/暗色主題自由切換
- **直覺界面**：基於 Avalonia Fluent UI 設計，現代化的視覺風格
- **配置保存**：記憶上次選擇的資料夾，應用重啟時自動載入

## 🏗️ 技術架構

- **框架**：[Avalonia 11.3.8](https://avaloniaui.net/) - 跨平台 XAML UI 框架
- **語言**：C# (.NET 10.0)
- **模式**：MVVM（使用 CommunityToolkit.MVVM）
- **圖表庫**：[LiveChartsCore 2.0.4](https://livecharts.dev/) - 實時數據可視化
- **日誌格式**：JSON（相容於 Serilog 結構化日誌格式）

## 📝 支援的日誌格式

應用程式支援標準 JSON 結構化日誌格式（如 Serilog 輸出）：

```json
{
  "@t": "2026-06-03T04:14:13.0000000Z",
  "@l": "Error",
  "@m": "An error occurred during processing",
  "@x": "System.Exception: ...",
  "UserId": "user123",
  "RequestId": "req-456"
}
```

其中：
- `@t` - 時間戳
- `@l` - 日誌級別
- `@m` - 渲染後的訊息（`@mt` 為訊息模板）
- `@x` - 異常堆疊跟蹤
- 其他欄位自動解析為自訂屬性

## 🚀 快速開始

### 前置要求
- .NET 10.0 SDK 或更新版本
- Windows 10+ / macOS 10.13+ / Linux（支援 X11 或 Wayland）

### 構建
```bash
cd LogDashboard
dotnet build
```

### 執行
```bash
dotnet run
```

## 📂 專案結構

```
LogDashboard/
├── Views/           # UI 視圖層（XAML）
├── ViewModels/      # 業務邏輯層
├── Services/        # 服務層（日誌解析、主題、監控等）
├── Models/          # 資料模型
├── Converters/      # XAML 值轉換器
└── Assets/          # 應用資源
```

## 📄 授權

MIT License - 詳見 [LICENSE](LICENSE) 文件

---

**開發者** | Made with ❤️ for log analysis
