using Microsoft.UI.Xaml;

namespace MineLink_App.Services;

/// <summary>
/// "Dark"/"Light"/"System"の文字列設定を、ウィンドウのルート要素に適用する。
/// ElementTheme.Defaultを指定するとOSのテーマ設定に追従する。
/// </summary>
public static class ThemeService
{
    public static void Apply(FrameworkElement root, string theme)
    {
        root.RequestedTheme = theme switch
        {
            "Light" => ElementTheme.Light,
            "Dark" => ElementTheme.Dark,
            _ => ElementTheme.Default,
        };
    }
}
