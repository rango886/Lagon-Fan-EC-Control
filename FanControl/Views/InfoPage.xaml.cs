using FanControl.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace FanControl.Views;

using System;
using System.Threading;
using System.Timers;
using FanControl.Utils;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;

public sealed partial class InfoPage : Page
{
    System.Timers.Timer aTimer;
    public InfoViewModel ViewModel
    {
        get;
    }

    public InfoPage()
    {
        ViewModel = App.GetService<InfoViewModel>();
        InitializeComponent();
        byte mayor = 0, minor = 0, revision = 0, release = 0;
        Utils.WinRing.GetDllVersion(ref mayor, ref minor, ref revision, ref release);
        Dll_version.Text = "DLL Version : " + mayor + "." + minor + "." + revision + "." + release;
        Dll_Status.Text = "DLL Status : " + WinRing.GetDllStatus().ToString();
        Utils.WinRing.GetDriverVersion(ref mayor, ref minor, ref revision, ref release);
        GetDriverVersion.Text = "Driver Version : " + mayor + "." + minor + "." + revision + "." + release;
        GetDriverType.Text = "Driver Type : " + WinRing.GetDriverType().ToString();
        if (WinRing.WinRingInitOk)
        {
            ChipID.Text = "Chip ID : " + EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.ECHIPID1).ToString("X2") + EC.DirectECRead(0x4E, 0x4F, (ushort)EC.ITE_REGISTER_MAP.ECHIPID2).ToString("X2") + " v" + EC.DirectECRead(0x4E, 0x4F, (ushort)EC.ITE_REGISTER_MAP.ECHIPVER).ToString("X1");
            FWVersion.Text = "EC FW Ver : " + EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FW_VER);
        }

        aTimer = new System.Timers.Timer(500);
        aTimer.Elapsed += updateUI;
        aTimer.AutoReset = true;
        aTimer.Enabled = false;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        aTimer.Enabled = true;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        aTimer.Enabled = false;
    }

    void updateUI(Object source, ElapsedEventArgs e)
    {
        var dispatcherQueue = ContentArea.DispatcherQueue;

        dispatcherQueue.TryEnqueue(new DispatcherQueueHandler(() =>
        {
            UpdateValue();
        }));
    }

    void UpdateValue()
    {
        current_pwm.Text = "Current PWM : " +
        EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.DCR0).ToString("X2") + " " +
        EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.DCR1).ToString("X2") + " " +
        EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.DCR2).ToString("X2") + " " +
        EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.DCR3).ToString("X2") + " " +
        EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.DCR6).ToString("X2") + " " +
        EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.DCR7).ToString("X2")
        ;

        if (WinRing.WinRingInitOk)
        {
            current_pwm_fan1.Text = "Current Fan1 PWM : " + EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.DCR4).ToString("X2");
            current_pwm_fan2.Text = "Current Fan2 PWM : " + EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.DCR5).ToString("X2");
        }
    }



}
