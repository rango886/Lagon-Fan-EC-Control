using System.Collections.Specialized;
using System.Web;

using FanControl.Contracts.Services;
using FanControl.ViewModels;

using Microsoft.Windows.AppNotifications;

namespace FanControl.Notifications;

public class AppNotificationService : IAppNotificationService
{
    private readonly INavigationService _navigationService;

    public AppNotificationService(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    ~AppNotificationService()
    {
        Unregister();
    }

    public void Initialize()
    {
        AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;

        AppNotificationManager.Default.Register();
    }

    public void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        // TODO: Handle notification invocations when your app is already running.

        // Navigate to a specific page based on the notification arguments.
        switch (ParseArguments(args.Argument)["action"])
        {
            case "EcError":
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                    {
                        App.MainWindow.ShowMessageDialogAsync("Model not Supported", "EC Error");

                        App.MainWindow.BringToFront();
                    });
                    break;
                }
            case "WinRingError":
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                    {
                        App.MainWindow.ShowMessageDialogAsync(Utils.WinRing.GetDllStatus().ToString(), "WinRing Error");

                        App.MainWindow.BringToFront();
                    });
                    break;
                }


            default:
                App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    App.MainWindow.BringToFront();
                });
                break;
        }




    }

    public bool Show(string payload)
    {
        var appNotification = new AppNotification(payload);

        AppNotificationManager.Default.Show(appNotification);

        return appNotification.Id != 0;
    }

    public NameValueCollection ParseArguments(string arguments)
    {
        return HttpUtility.ParseQueryString(arguments);
    }

    public void Unregister()
    {
        AppNotificationManager.Default.Unregister();
    }
}
