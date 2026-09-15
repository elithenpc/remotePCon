using System;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace RemotePCon
{
    public sealed class NotificationBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            try
            {
                var listener = UserNotificationListener.Current;
                if (listener.GetAccessStatus() != UserNotificationListenerAccessStatus.Allowed)
                    return;

                var notifications = await listener.GetNotificationsAsync(NotificationKinds.Toast);
                var now = DateTimeOffset.Now;

                foreach (var notification in notifications)
                {
                    try
                    {
                        // Only react to notifications created very recently. The trigger
                        // fires for additions and removals, so this prevents an old
                        // matching notification from waking the helper on every removal.
                        if ((now - notification.CreationTime).TotalSeconds > 15)
                            continue;

                        var appName = notification.AppInfo?.DisplayInfo?.DisplayName ?? string.Empty;
                        if (appName.IndexOf("Gmail", StringComparison.OrdinalIgnoreCase) < 0)
                            continue;

                        var binding = notification.Notification?.Visual?.GetBinding(KnownNotificationBindings.ToastGeneric);
                        if (binding == null)
                            continue;

                        var text = binding.GetTextElements();
                        var title = text?.FirstOrDefault()?.Text ?? string.Empty;

                        // The command is deliberately exact and case-insensitive.
                        if (!string.Equals(title.Trim(), "wake", StringComparison.OrdinalIgnoreCase))
                            continue;

                        await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("/wake");
                        break;
                    }
                    catch
                    {
                        // One malformed notification must not stop processing the others.
                    }
                }
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}
