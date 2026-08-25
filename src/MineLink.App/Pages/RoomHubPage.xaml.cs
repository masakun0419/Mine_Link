using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MineLink_App.Models;
using MineLink_App.Services;
using Windows.ApplicationModel.DataTransfer;

namespace MineLink_App.Pages;

/// <summary>
/// UI-03(ルーム公開)とUI-04(ルーム参加)を1つのPivotにまとめたページ。
/// まだ通信処理(MineLink.Core相当)が存在しないため、実際のRelay/TCP通信は行わず、
/// 参加コードの発行や参加者リストはすべてこのクラス内でモックしている。
/// docs/08_開発_テスト_運用.md のPhase 0以降で本物の通信に置き換える想定。
/// </summary>
public sealed partial class RoomHubPage : Page
{
    // Home画面の「公開する」「参加する」ボタンから、どちらのタブを開いた状態で
    // 遷移してきたかをFrame.Navigateの引数として渡すために使う。
    public enum Tab
    {
        Publish,
        Join,
    }

    public AppStateService AppState => AppStateService.Instance;

    // 公開直後、誰かが参加してくるまでの間を疑似的に演出するためだけのタイマー。
    private DispatcherTimer? _mockJoinTimer;

    public RoomHubPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is Tab tab)
        {
            RoomPivot.SelectedIndex = tab == Tab.Publish ? 0 : 1;
        }

        // 公開中に一度ルーム画面を離れてまた戻ってきた場合でも、
        // 公開結果パネル(参加コードや接続者一覧)を表示し直す。
        if (AppState.State == ConnectionState.Publishing)
        {
            ShowPublishedState();
        }
    }

    // ------- 公開 (UI-03) -------

    private async void PublishAction_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PublishAddressBox.Text))
        {
            await ErrorDialogService.ShowAsync(XamlRoot, AppErrorCode.HostProbe001);
            return;
        }

        AppState.LastEdition = "Java";
        AppState.LastAddress = $"{PublishAddressBox.Text}:{(int)PublishPortBox.Value}";
        AppState.JoinCode = GenerateJoinCode();
        AppState.State = ConnectionState.Publishing;
        AppState.Members.Clear();
        AppState.AddLog(LogSeverity.Success, $"ルームを公開しました。参加コード: {AppState.JoinCode}");

        ShowPublishedState();
        ScheduleMockJoin();
    }

    private void ShowPublishedState()
    {
        JoinCodeText.Text = AppState.JoinCode ?? string.Empty;
        PublishedPanel.Visibility = Visibility.Visible;
        PublishActionButton.IsEnabled = false;
    }

    // 公開してから4秒後に、ゲストが1人参加してきたように見せかける演出。
    // 実通信を実装したら、このメソッドとタイマーは丸ごと不要になる。
    private void ScheduleMockJoin()
    {
        _mockJoinTimer?.Stop();
        _mockJoinTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
        _mockJoinTimer.Tick += (_, _) =>
        {
            _mockJoinTimer!.Stop();
            if (AppState.State == ConnectionState.Publishing && AppState.Members.Count == 0)
            {
                var member = new RoomMember { DisplayName = "Guest_" + Random.Shared.Next(100, 999), PingMs = Random.Shared.Next(20, 90), ConnectedAt = DateTimeOffset.Now };
                AppState.Members.Add(member);
                AppState.AddLog(LogSeverity.Info, $"{member.DisplayName} が参加しました。");
            }
        };
        _mockJoinTimer.Start();
    }

    private async void CopyCode_Click(object sender, RoutedEventArgs e)
    {
        if (AppState.JoinCode is null)
        {
            return;
        }

        var package = new DataPackage();
        package.SetText(AppState.JoinCode);
        Clipboard.SetContent(package);
        await ShowCopiedTipAsync(sender);
    }

    private void RenewCode_Click(object sender, RoutedEventArgs e)
    {
        AppState.JoinCode = GenerateJoinCode();
        JoinCodeText.Text = AppState.JoinCode;
        AppState.AddLog(LogSeverity.Info, "参加コードを更新しました。");
    }

    private void StopPublish_Click(object sender, RoutedEventArgs e)
    {
        _mockJoinTimer?.Stop();
        AppState.State = ConnectionState.Idle;
        AppState.Members.Clear();
        AppState.JoinCode = null;
        PublishedPanel.Visibility = Visibility.Collapsed;
        PublishActionButton.IsEnabled = true;
        AppState.AddLog(LogSeverity.Info, "公開を停止しました。");
    }

    private void KickMember_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: RoomMember member })
        {
            AppState.Members.Remove(member);
            AppState.AddLog(LogSeverity.Warning, $"{member.DisplayName} を切断しました。");
        }
    }

    // 紛らわしい文字(0/O, 1/I など)を除いた6桁の参加コードを生成する。
    private static string GenerateJoinCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var sb = new StringBuilder();
        for (var i = 0; i < 6; i++)
        {
            sb.Append(chars[Random.Shared.Next(chars.Length)]);
        }

        return sb.ToString();
    }

    // ------- 参加 (UI-04) -------

    // Text自体をこのハンドラ内で書き換えるとTextChangedが再帰的に発火するため、
    // 正規化処理中であることを示すフラグで無限ループを防ぐ。
    private bool _normalizing;

    // docs/04_GUI画面設計.md 6.ルーム参加画面: 「自動で大文字化し、空白とハイフンを無視する」
    private void JoinCodeBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_normalizing)
        {
            return;
        }

        _normalizing = true;
        var caret = JoinCodeBox.SelectionStart;
        var normalized = JoinCodeBox.Text.ToUpperInvariant().Replace(" ", string.Empty).Replace("-", string.Empty);
        if (normalized != JoinCodeBox.Text)
        {
            JoinCodeBox.Text = normalized;
            JoinCodeBox.SelectionStart = Math.Min(caret, normalized.Length);
        }

        _normalizing = false;
    }

    private void LocalPortModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // XAMLのSelectedIndex="0"がInitializeComponent中に即座にこのイベントを発火させるが、
        // その時点ではまだLocalPortBox(このメソッドより後に宣言されている要素)が
        // フィールドに接続されておらずnullのため、ここでガードしないとNREで落ちる。
        if (LocalPortBox is null)
        {
            return;
        }

        LocalPortBox.IsEnabled = LocalPortModeCombo.SelectedIndex == 1;
    }

    private async void JoinAction_Click(object sender, RoutedEventArgs e)
    {
        var name = JoinDisplayNameBox.Text.Trim();
        if (name.Length is < 1 or > 16)
        {
            await ErrorDialogService.ShowAsync(XamlRoot, AppErrorCode.RoomCode001);
            return;
        }

        // 実際のRelayがないため、「同じアプリで直前に公開したコード」とだけ照合する。
        // 未公開の状態で参加しようとすると、常にROOM-CODE-001エラーになる。
        var expected = AppState.JoinCode;
        if (expected is null || !string.Equals(JoinCodeBox.Text, expected, StringComparison.OrdinalIgnoreCase))
        {
            await ErrorDialogService.ShowAsync(XamlRoot, AppErrorCode.RoomCode001);
            return;
        }

        var localPort = (int)(LocalPortModeCombo.SelectedIndex == 1 ? LocalPortBox.Value : 25565);
        AppState.State = ConnectionState.Connected;
        AppState.AddLog(LogSeverity.Success, $"ルーム {expected} に参加しました。");

        LocalAddressText.Text = $"localhost:{localPort}";
        ConnectedPanel.Visibility = Visibility.Visible;
        JoinActionButton.IsEnabled = false;
    }

    private async void CopyLocalAddress_Click(object sender, RoutedEventArgs e)
    {
        var package = new DataPackage();
        package.SetText(LocalAddressText.Text);
        Clipboard.SetContent(package);
        await ShowCopiedTipAsync(sender);
    }

    private void LaunchMinecraft_Click(object sender, RoutedEventArgs e)
    {
        AppState.AddLog(LogSeverity.Info, "Minecraftの起動はデモ版では未実装です。");
    }

    private void Disconnect_Click(object sender, RoutedEventArgs e)
    {
        AppState.State = ConnectionState.Idle;
        ConnectedPanel.Visibility = Visibility.Collapsed;
        JoinActionButton.IsEnabled = true;
        AppState.AddLog(LogSeverity.Info, "ルームから切断しました。");
    }

    private static async Task ShowCopiedTipAsync(object sender)
    {
        if (sender is Button button)
        {
            var original = button.Content;
            button.Content = "コピーしました";
            button.IsEnabled = false;
            await Task.Delay(1200);
            button.Content = original;
            button.IsEnabled = true;
        }
    }
}
