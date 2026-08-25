using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MineLink_App.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MineLink_App;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private static Window? _window;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();

        // 画面のどこかで想定外の例外が発生してもアプリごと落ちないようにする保険。
        // 本来は個々のバグを直すべきだが、GUI試作段階では未検証の操作経路が
        // 多いため、ここで捕まえてログに残し、ユーザー体験としては継続させる。
        UnhandledException += (_, e) =>
        {
            e.Handled = true;
            AppStateService.Instance.AddLog(Models.LogSeverity.Error, $"予期しないエラーが発生しました: {e.Message}");

            try
            {
                var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MineLink");
                Directory.CreateDirectory(logDir);
                File.AppendAllText(Path.Combine(logDir, "crash.log"), $"[{DateTimeOffset.Now}]\n{e.Exception}\n\n");
            }
            catch
            {
                // ログ出力自体の失敗は無視する。
            }
        };
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        // UI-01 初回セットアップは、初回起動時のみメインウィンドウの前に表示する。
        if (LocalSettingsService.IsFirstRun)
        {
            _window = new SetupWindow();
            ThemeService.Apply((FrameworkElement)_window.Content, LocalSettingsService.Theme);
            _window.Activate();
        }
        else
        {
            LaunchMainWindow();
        }
    }

    // SetupWindowの「開始する/スキップ」からも呼ばれるため、staticメソッドとして公開する。
    public static void LaunchMainWindow()
    {
        var window = new MainWindow();
        ThemeService.Apply((FrameworkElement)window.Content, LocalSettingsService.Theme);
        window.Activate();
        _window = window;
    }
}
