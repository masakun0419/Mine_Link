using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MineLink_App.Models;
using MineLink_App.Services;

namespace MineLink_App.Pages;

/// <summary>
/// UI-05 通信状況画面。実測値がまだ取れないため、Ping/送受信量は
/// ランダムな数値で表示だけを再現している。セッション時間だけは
/// このページに来た時刻からの経過時間として実際に計測する。
/// </summary>
public sealed partial class StatusPage : Page
{
    public AppStateService AppState => AppStateService.Instance;

    private DispatcherTimer? _timer;
    private DateTimeOffset _sessionStart;

    public StatusPage()
    {
        InitializeComponent();
        // ページを離れたらタイマーを止めないと、非表示のページを裏で
        // 更新し続けてしまう(メモリリークの原因にもなる)。
        Unloaded += (_, _) => _timer?.Stop();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        var active = AppState.State != ConnectionState.Idle;
        NotConnectedText.Visibility = active ? Visibility.Collapsed : Visibility.Visible;
        StatsPanel.Visibility = active ? Visibility.Visible : Visibility.Collapsed;

        if (!active)
        {
            return;
        }

        _sessionStart = DateTimeOffset.Now;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => Tick();
        _timer.Start();
        Tick();
    }

    private void Tick()
    {
        // TODO: Relay実装後、ここを実際のPing/送受信量測定に置き換える。
        PingText.Text = $"{Random.Shared.Next(18, 65)} ms";
        BandwidthText.Text = $"{Random.Shared.Next(20, 400)} KB/s";
        var elapsed = DateTimeOffset.Now - _sessionStart;
        SessionText.Text = elapsed.ToString(@"hh\:mm\:ss");
    }
}
