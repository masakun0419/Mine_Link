using System.Text.Json;

namespace MineLink_App.Services;

/// <summary>
/// ローカル設定の保存先。
/// WinUI3標準のWindows.Storage.ApplicationDataはMSIXパッケージID必須で、
/// unpackaged実行(単体.exe配布)では使えないため、%LOCALAPPDATA%配下の
/// JSONファイルへ直接読み書きする方式にしている。
/// </summary>
public static class LocalSettingsService
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MineLink",
        "settings.json");

    private static readonly Dictionary<string, JsonElement> Values = Load();

    public static bool IsFirstRun
    {
        get => !TryGet(nameof(IsFirstRun), out bool v) || v;
        set => Set(nameof(IsFirstRun), value);
    }

    public static string DisplayName
    {
        get => TryGet(nameof(DisplayName), out string? v) ? v! : "Player";
        set => Set(nameof(DisplayName), value);
    }

    public static string Theme
    {
        get => TryGet(nameof(Theme), out string? v) ? v! : "Dark";
        set => Set(nameof(Theme), value);
    }

    public static int DefaultPort
    {
        get => TryGet(nameof(DefaultPort), out int v) ? v : 25565;
        set => Set(nameof(DefaultPort), value);
    }

    public static bool LaunchAtStartup
    {
        get => TryGet(nameof(LaunchAtStartup), out bool v) && v;
        set => Set(nameof(LaunchAtStartup), value);
    }

    public static string LogLevel
    {
        get => TryGet(nameof(LogLevel), out string? v) ? v! : "情報";
        set => Set(nameof(LogLevel), value);
    }

    private static bool TryGet<T>(string key, out T? value)
    {
        if (Values.TryGetValue(key, out var element))
        {
            value = element.Deserialize<T>();
            return value is not null || typeof(T) == typeof(bool) || typeof(T) == typeof(int);
        }

        value = default;
        return false;
    }

    private static void Set<T>(string key, T value)
    {
        Values[key] = JsonSerializer.SerializeToElement(value);
        Save();
    }

    private static Dictionary<string, JsonElement> Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)
                       ?? new Dictionary<string, JsonElement>();
            }
        }
        catch (Exception)
        {
            // 設定ファイルが壊れている場合は初期状態から始める。
        }

        return new Dictionary<string, JsonElement>();
    }

    private static void Save()
    {
        var directory = Path.GetDirectoryName(SettingsPath)!;
        Directory.CreateDirectory(directory);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(Values));
    }
}
