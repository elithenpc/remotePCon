using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Notifications.Management;

namespace RemotePCon
{
    sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            Window.Current.Content = new MainPage();
            Window.Current.Activate();

            await NotificationController.InitialiseAsync();
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
        }
    }

    internal static class NotificationController
    {
        private const string TaskName = "RemotePCon.NotificationWatcher";

        public static async System.Threading.Tasks.Task InitialiseAsync()
        {
            var listener = UserNotificationListener.Current;
            var access = listener.GetAccessStatus();

            if (access != UserNotificationListenerAccessStatus.Allowed)
                access = await listener.RequestAccessAsync();

            if (access != UserNotificationListenerAccessStatus.Allowed)
                return;

            foreach (var task in BackgroundTaskRegistration.AllTasks.Values.ToList())
            {
                if (task.Name == TaskName)
                    task.Unregister(false);
            }

            var builder = new BackgroundTaskBuilder
            {
                Name = TaskName,
                TaskEntryPoint = "RemotePCon.NotificationBackgroundTask"
            };

            builder.SetTrigger(new UserNotificationChangedTrigger(NotificationKinds.Toast));
            builder.Register();
        }
    }
}
