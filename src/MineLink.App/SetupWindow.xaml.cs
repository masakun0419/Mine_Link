using Microsoft.UI.Xaml;
using MineLink_App.Services;

namespace MineLink_App;

/// <summary>
/// UI-01 初回セットアップ画面。表示名・エディション・既定ポートのみを保存する。
/// </summary>
public sealed partial class SetupWindow : Window
{
    public SetupWindow()
    {
        InitializeComponent();
        Services.WindowSizing.SetSizeAndCenter(this, 520, 640);
    }

    private void Start_Click(object sender, RoutedEventArgs e)
    {
        var name = DisplayNameBox.Text.Trim();
        LocalSettingsService.DisplayName = string.IsNullOrEmpty(name) ? "Player" : name;
        LocalSettingsService.DefaultPort = (int)PortBox.Value;
        Complete();
    }

    private void Skip_Click(object sender, RoutedEventArgs e) => Complete();

    private void Complete()
    {
        LocalSettingsService.IsFirstRun = false;
        App.LaunchMainWindow();
        Close();
    }
}
