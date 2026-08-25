using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MineLink_App.Models;

namespace MineLink_App.Services;

/// <summary>
/// アプリ内で共有する実行時状態。現段階では通信を実装していないため、
/// すべてモックデータで画面遷移とUI挙動のみを再現する。
/// </summary>
public sealed class AppStateService : INotifyPropertyChanged
{
    public static AppStateService Instance { get; } = new();

    private AppStateService()
    {
        Logs.Add(new LogEntry { Timestamp = DateTimeOffset.Now, Severity = LogSeverity.Info, Message = "MineLinkを起動しました。" });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private ConnectionState _state = ConnectionState.Idle;
    public ConnectionState State
    {
        get => _state;
        set
        {
            if (_state == value)
            {
                return;
            }

            _state = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StateLabel));
        }
    }

    public string StateLabel => State switch
    {
        ConnectionState.Idle => "待機中",
        ConnectionState.Publishing => "公開中",
        ConnectionState.Connected => "接続中",
        _ => "不明",
    };

    private string? _joinCode;
    public string? JoinCode
    {
        get => _joinCode;
        set { _joinCode = value; OnPropertyChanged(); }
    }

    private string _lastEdition = "Java";
    public string LastEdition
    {
        get => _lastEdition;
        set { _lastEdition = value; OnPropertyChanged(); OnPropertyChanged(nameof(LastSettingsSummary)); }
    }

    private string _lastAddress = "127.0.0.1:25565";
    public string LastAddress
    {
        get => _lastAddress;
        set { _lastAddress = value; OnPropertyChanged(); OnPropertyChanged(nameof(LastSettingsSummary)); }
    }

    public string LastSettingsSummary => $"{LastEdition} / {LastAddress}";

    public ObservableCollection<RoomMember> Members { get; } = new();

    public ObservableCollection<LogEntry> Logs { get; } = new();

    public void AddLog(LogSeverity severity, string message)
    {
        Logs.Insert(0, new LogEntry { Timestamp = DateTimeOffset.Now, Severity = severity, Message = message });
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
