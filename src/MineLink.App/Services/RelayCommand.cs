using System.Windows.Input;

namespace MineLink_App.Services;

/// <summary>
/// XAMLのCommandプロパティに束縛するための最小限のICommand実装。
/// このアプリでは常にCanExecuteがtrueの単純な操作にのみ使うため、
/// CanExecuteChangedは意図的に発火させていない。
/// </summary>
public sealed class RelayCommand : ICommand
{
    private readonly Action _execute;

    public RelayCommand(Action execute) => _execute = execute;

#pragma warning disable CS0067 // ICommandの仕様上必須だが、このアプリでは使わない
    public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => _execute();
}
