using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MineLink_App.Models;
using MineLink_App.Services;

namespace MineLink_App.Pages;

public sealed partial class LogPage : Page
{
    public AppStateService AppState => AppStateService.Instance;

    public LogPage()
    {
        InitializeComponent();
    }

    public static string GlyphFor(LogSeverity severity) => severity switch
    {
        LogSeverity.Success => "",
        LogSeverity.Warning => "",
        LogSeverity.Error => "",
        _ => "",
    };

    public static Brush BrushFor(LogSeverity severity)
    {
        var key = severity switch
        {
            LogSeverity.Success => "StatusSuccessBrush",
            LogSeverity.Warning => "StatusWarningBrush",
            LogSeverity.Error => "StatusErrorBrush",
            _ => "StatusInfoBrush",
        };

        return (Brush)Application.Current.Resources[key];
    }
}
