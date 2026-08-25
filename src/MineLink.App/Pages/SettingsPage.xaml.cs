using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MineLink_App.Services;

namespace MineLink_App.Pages;

/// <summary>UI-07 設定画面。LocalSettingsServiceの読み書きだけを行う。</summary>
public sealed partial class SettingsPage : Page
{
    // 各コントロールに保存済みの値を流し込む(Loaded内)最中もSelectionChangedなどが
    // 発火するため、そのタイミングで保存処理が空振りしないようにするガード。
    // falseになるまでは「ユーザー操作ではなく初期化中」とみなして何もしない。
    private bool _isLoading = true;

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += SettingsPage_Loaded;
    }

    private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
    {
        ThemeRadios.SelectedIndex = LocalSettingsService.Theme switch
        {
            "Dark" => 0,
            "Light" => 1,
            _ => 2,
        };
        DefaultPortBox.Value = LocalSettingsService.DefaultPort;
        StartupCheckBox.IsChecked = LocalSettingsService.LaunchAtStartup;
        LogLevelCombo.SelectedIndex = LocalSettingsService.LogLevel switch
        {
            "詳細" => 0,
            "警告のみ" => 2,
            _ => 1,
        };

        _isLoading = false;
    }

    private void ThemeRadios_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoading || ThemeRadios.SelectedItem is not RadioButton { Tag: string tag })
        {
            return;
        }

        LocalSettingsService.Theme = tag;
        ThemeService.Apply((FrameworkElement)XamlRoot.Content, tag);
    }

    private void DefaultPortBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (_isLoading || double.IsNaN(args.NewValue))
        {
            return;
        }

        LocalSettingsService.DefaultPort = (int)args.NewValue;
    }

    private void StartupCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        LocalSettingsService.LaunchAtStartup = StartupCheckBox.IsChecked ?? false;
    }

    private void LogLevelCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoading || LogLevelCombo.SelectedItem is not ComboBoxItem { Content: string content })
        {
            return;
        }

        LocalSettingsService.LogLevel = content;
    }
}
