using System.Linq;
using H.NotifyIcon;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MineLink_App.Pages;
using MineLink_App.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MineLink_App;

public sealed partial class MainWindow : Window
{
    // アプリ全体で共有する状態（公開中/接続中などの表示に使う）。
    public AppStateService AppState => AppStateService.Instance;

    // トレイアイコンのダブルクリックで呼ばれるコマンド。x:Bindは
    // InitializeComponent()より前に値がセットされている必要がある。
    public RelayCommand RestoreCommand { get; }

    public MainWindow()
    {
        RestoreCommand = new RelayCommand(RestoreFromTray);

        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        AppWindow.SetIcon("Assets/AppIcon.ico");
        AppWindow.Closing += AppWindow_Closing;
        WindowSizing.SetSizeAndCenter(this, 1040, 680);

        // TaskbarIconはXAMLに置いただけでは実体が作られないため、
        // ここで明示的に生成しておく。これを忘れるとウィンドウを閉じた際に
        // タスクトレイに何も表示されず、アプリを復帰させる手段がなくなる。
        TrayIcon.ForceCreate();

        // Home画面の「公開する/参加する」ボタンはサイドバーを経由せず
        // NavFrame.Navigateを直接呼ぶため、Navigatedを監視して
        // サイドバーの選択項目(緑のハイライト)を後から同期させる。
        NavFrame.Navigated += NavFrame_Navigated;

        NavFrame.Navigate(typeof(HomePage));
    }

    // NavView.SelectedItemをコードから変更すると再びSelectionChangedが
    // 発火してしまうため、その間だけ二重ナビゲーションを防ぐガード。
    private bool _isSyncingSelection;

    private void NavFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        var tag = e.SourcePageType.Name switch
        {
            nameof(HomePage) => "home",
            nameof(RoomHubPage) => "room",
            nameof(StatusPage) => "status",
            nameof(LogPage) => "log",
            nameof(SettingsPage) => "settings",
            _ => null,
        };

        _isSyncingSelection = true;
        try
        {
            if (tag == "settings")
            {
                NavView.SelectedItem = NavView.SettingsItem;
                return;
            }

            foreach (var item in NavView.MenuItems.OfType<NavigationViewItem>())
            {
                if (Equals(item.Tag, tag))
                {
                    NavView.SelectedItem = item;
                    break;
                }
            }
        }
        finally
        {
            _isSyncingSelection = false;
        }
    }

    // ウィンドウ右上のXを押しても、状態に関わらず常にタスクトレイへ格納するだけで
    // プロセスは終了させない。完全終了はトレイアイコンのコンテキストメニュー「終了」からのみ行う。
    private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        args.Cancel = true;
        this.Hide();
    }

    private void RestoreFromTray()
    {
        this.Show();
        this.Activate();
    }

    private void TrayOpen_Click(object sender, RoutedEventArgs e) => RestoreFromTray();

    private void TrayExit_Click(object sender, RoutedEventArgs e)
    {
        TrayIcon.Dispose();
        Application.Current.Exit();
    }

    private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        NavView.IsPaneOpen = !NavView.IsPaneOpen;
    }

    private void TitleBar_BackRequested(TitleBar sender, object args)
    {
        NavFrame.GoBack();
    }

    // サイドバーの選択項目からFrame内のページへ振り分ける。
    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        // NavFrame_Navigatedがハイライトを同期させるためだけにSelectedItemを
        // 書き換えたとき、ここで再度Navigateされてしまうのを防ぐ。
        if (_isSyncingSelection)
        {
            return;
        }

        if (args.IsSettingsSelected)
        {
            NavFrame.Navigate(typeof(SettingsPage));
        }
        else if (args.SelectedItem is NavigationViewItem item)
        {
            switch (item.Tag)
            {
                case "home":
                    NavFrame.Navigate(typeof(HomePage));
                    break;
                case "room":
                    NavFrame.Navigate(typeof(RoomHubPage));
                    break;
                case "status":
                    NavFrame.Navigate(typeof(StatusPage));
                    break;
                case "log":
                    NavFrame.Navigate(typeof(LogPage));
                    break;
                default:
                    throw new InvalidOperationException($"Unknown navigation item tag: {item.Tag}");
            }
        }
    }
}
