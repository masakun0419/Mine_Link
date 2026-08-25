using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace MineLink_App.Services;

/// <summary>
/// WinUI3のWindowは既定だと画面に対してかなり大きく開くため、
/// 用途に合わせた固定サイズで中央表示させるための小さなヘルパー。
/// </summary>
public static class WindowSizing
{
    public static void SetSizeAndCenter(Window window, int width, int height)
    {
        var appWindow = window.AppWindow;
        appWindow.Resize(new SizeInt32(width, height));

        var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
        var x = (displayArea.WorkArea.Width - width) / 2;
        var y = (displayArea.WorkArea.Height - height) / 2;
        appWindow.Move(new PointInt32(x, y));
    }
}
