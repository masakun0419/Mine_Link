using Microsoft.UI.Xaml.Controls;
using MineLink_App.Services;

namespace MineLink_App.Pages;

/// <summary>UI-02 ホーム画面。「公開する」「参加する」からRoomHubPageの該当タブへ遷移させるだけの入口。</summary>
public sealed partial class HomePage : Page
{
    public AppStateService AppState => AppStateService.Instance;

    public HomePage()
    {
        InitializeComponent();
    }

    private void Publish_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        => Frame.Navigate(typeof(RoomHubPage), RoomHubPage.Tab.Publish);

    private void Join_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        => Frame.Navigate(typeof(RoomHubPage), RoomHubPage.Tab.Join);
}
