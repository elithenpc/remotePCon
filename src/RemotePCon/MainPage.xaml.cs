using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.UserDataTasks;
using Windows.UI.Notifications.Management;
using Windows.UI.Popups;
using Windows.ApplicationModel;

namespace RemotePCon
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshStatusAsync();
        }

        private async System.Threading.Tasks.Task RefreshStatusAsync()
        {
            var status = UserNotificationListener.Current.GetAccessStatus();
            StatusText.Text = status == UserNotificationListenerAccessStatus.Allowed
                ? "Enabled. Waiting for a Windows notification containing the exact title/subject wake from Gmail."
                : "Notification access is not enabled yet.";
        }

        private async void Enable_Click(object sender, RoutedEventArgs e)
        {
            await NotificationController.InitialiseAsync();
            await RefreshStatusAsync();
        }

        private async void Wake_Click(object sender, RoutedEventArgs e)
        {
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("/wake");
        }

        private async void Release_Click(object sender, RoutedEventArgs e)
        {
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("/release");
        }
    }
}
