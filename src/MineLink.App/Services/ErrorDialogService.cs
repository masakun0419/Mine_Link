using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MineLink_App.Services;

/// <summary>
/// UI-08 エラーダイアログ。docs/04_GUI画面設計.md の「8. エラー表示例」に対応する。
/// </summary>
public enum AppErrorCode
{
    ClientPort001,
    HostProbe001,
    RoomCode001,
    RelayConn001,
    Version001,
}

public static class ErrorDialogService
{
    private sealed record ErrorInfo(string Code, string Message, string PrimaryAction);

    private static readonly Dictionary<AppErrorCode, ErrorInfo> Catalog = new()
    {
        [AppErrorCode.ClientPort001] = new("CLIENT-PORT-001", "使用するポートが他のアプリで使われています", "別ポートを自動選択"),
        [AppErrorCode.HostProbe001] = new("HOST-PROBE-001", "Minecraft Serverに接続できません", "再確認"),
        [AppErrorCode.RoomCode001] = new("ROOM-CODE-001", "参加コードが見つからないか、期限切れです", "再入力"),
        [AppErrorCode.RelayConn001] = new("RELAY-CONN-001", "中継サーバーへ接続できません", "再試行"),
        [AppErrorCode.Version001] = new("VERSION-001", "MineLinkの更新が必要です", "更新画面を開く"),
    };

    // ContentDialogはXamlRootごとに同時に1つしか開けず、2つ目を開こうとすると
    // COMExceptionが発生してアプリ全体が落ちる。ボタン連打などで多重に
    // ShowAsyncが呼ばれても無視できるよう、表示中フラグで単純に排他する。
    private static bool _isShowing;

    public static async Task ShowAsync(XamlRoot xamlRoot, AppErrorCode code)
    {
        if (_isShowing)
        {
            return;
        }

        _isShowing = true;
        try
        {
            var info = Catalog[code];

            var dialog = new ContentDialog
            {
                XamlRoot = xamlRoot,
                Title = info.Code,
                Content = info.Message,
                PrimaryButtonText = info.PrimaryAction,
                CloseButtonText = "閉じる",
                DefaultButton = ContentDialogButton.Primary,
            };

            await dialog.ShowAsync();
        }
        finally
        {
            _isShowing = false;
        }
    }
}
