﻿using System;
using System.Drawing;
using System.Threading.Tasks;

using ABI.Windows.UI;

using Color = Windows.UI.Color;

using FanControl.Activation;
using FanControl.Contracts.Services;
using FanControl.Core.Contracts.Services;
using FanControl.Core.Services;
using FanControl.Helpers;
using FanControl.Models;
using FanControl.Notifications;
using FanControl.Services;
using FanControl.Utils;
using FanControl.ViewModels;
using FanControl.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

using Windows.System;
using Windows.UI;
using Windows.UI.Popups;

namespace FanControl;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

            // Services
            services.AddSingleton<IAppNotificationService, AppNotificationService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddTransient<AdvancedViewModel>();
            services.AddTransient<InfoViewModel>();
            services.AddTransient<InfoPage>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        App.GetService<IAppNotificationService>().Initialize();
        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);




        //Initialize WinRing
        var success = Utils.WinRing.InitializeOls();

        if (!success)
        {
            App.GetService<IAppNotificationService>().Show(string.Format("WinRingInitializeError".GetLocalized(), AppContext.BaseDirectory));
            WinRing.WinRingInitOk = false;
        }
        else
            WinRing.WinRingInitOk = true;

        if (WinRing.WinRingInitOk)
        {
            if (EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.ECHIPID1) != 0x82 || EC.DirectECRead(0x4E, 0x4F, (ushort)EC.ITE_REGISTER_MAP.ECHIPID2) != 0x27)

            {
                App.GetService<IAppNotificationService>().Show(string.Format("EcErrorCode".GetLocalized(), AppContext.BaseDirectory));
                WinRing.WinRingInitOk = false;
            }
            
        }

        await App.GetService<IActivationService>().ActivateAsync(args);
    }

}
