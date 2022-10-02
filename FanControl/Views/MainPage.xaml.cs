using System;
using System.Drawing;
using System.Reflection;
using FanControl.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.UI;
using WinRT;
using Color = Windows.UI.Color;
using Path = Microsoft.UI.Xaml.Shapes.Path;
using Point = Windows.Foundation.Point;
using FanControl.Utils;
using Microsoft.UI.Xaml.Navigation;
using System.Timers;
using Microsoft.UI.Dispatching;
using Newtonsoft.Json.Linq;
using System.Reflection.Emit;
using ABI.Windows.Foundation;
using System.Linq;
using FanControl.Control;
using Windows.ApplicationModel.Store;

namespace FanControl.Views;

public sealed partial class MainPage : Page
{
    bool IsEditMode = false;
    System.Timers.Timer aTimer;
    public MainViewModel ViewModel
    {
        get;
    }



    public MainPage()
    {

        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();

        FanLeft.setDataName("FAN1 Speed");
        FanRight.setDataName("FAN2 Speed");
        FanRight.setDataValue(0 + " RPM");
        FanLeft.setDataValue(0 + " RPM");
        FanAcc.setDataName("FAN1 Accelleration");
        FanDec.setDataName("FAN2 Accelleration");
        FanDec.setDataValue(0 + " ms");
        FanAcc.setDataValue(0 + " ms");
        FanLeft.OnGraphChange += onFanLeftChange;
        FanRight.OnGraphChange += onFanRightChange;
        FanAcc.OnGraphChange += onFanAccChange;
        FanDec.OnGraphChange += onFanDecChange;

        CPU_TEMP.OnGraphChange += OnCPU_TempChange;
        GPU_TEMP.OnGraphChange += OnGPU_TempChange;
        VRM_TEMP.OnGraphChange += OnVRM_TempChange;


        CPU_TEMP_HYST.OnGraphChange += OnCPU_HYST_TempChange;
        GPU_TEMP_HYST.OnGraphChange += OnGPU_HYST_TempChange;
        VRM_TEMP_HYST.OnGraphChange += OnVRM_HYST_TempChange;

        _fan_point_no.ValueChanged += _fan_point_no_ValueChanged;


        aTimer = new System.Timers.Timer(500);
        aTimer.Elapsed += updateUI;
        aTimer.AutoReset = true;
        aTimer.Enabled = false;
        updateUI(null, null);
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
            if (WinRing.WinRingInitOk)
            {

                byte Low = EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN1_RPM_LSB);
                byte High = EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN1_RPM_MSB);
                UInt16 FanSpeed = (ushort)(High << 8 | Low);
                FanLeft.setDataValue(FanSpeed + " RPM");

                Low = EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN2_RPM_LSB);
                High = EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN2_RPM_MSB);
                FanSpeed = (ushort)(High << 8 | Low);
                FanRight.setDataValue(FanSpeed + " RPM");
                _fan_point_no.Value = EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN_POINT);

                FanAcc.setDataValue(100 * EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN1_CUR_ACC) + " ms");
                FanDec.setDataValue(100 * EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN2_CUR_ACC) + " ms");

                if (!IsEditMode)
                {
                    CPU_Source.IsChecked = EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.CPU_TEMP_EN) != 0;
                    GPU_Source.IsChecked = EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.GPU_TEMP_EN) != 0;
                    VRM_Source.IsChecked = EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.VRM_TEMP_EN) != 0;
                    FanLeft.SetGraphValue(EC.DirectECReadArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN1_BASE, (int)FanLeft.NoOfSlider));
                    FanRight.SetGraphValue(EC.DirectECReadArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN2_BASE, (int)FanRight.NoOfSlider));
                    FanAcc.SetGraphValue(EC.DirectECReadArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN_ACC_BASE, (int)FanAcc.NoOfSlider));
                    FanDec.SetGraphValue(EC.DirectECReadArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN_DEC_BASE, (int)FanDec.NoOfSlider));
                    CPU_TEMP.SetGraphValue(EC.DirectECReadArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.CPU_TEMP, (int)CPU_TEMP.NoOfSlider));
                    GPU_TEMP.SetGraphValue(EC.DirectECReadArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.GPU_TEMP, (int)GPU_TEMP.NoOfSlider));
                    VRM_TEMP.SetGraphValue(EC.DirectECReadArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.VRM_TEMP, (int)CPU_TEMP_HYST.NoOfSlider));
                    CPU_TEMP_HYST.SetGraphValue(EC.DirectECReadArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.CPU_TEMP_HYST, (int)CPU_TEMP_HYST.NoOfSlider));
                    GPU_TEMP_HYST.SetGraphValue(EC.DirectECReadArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.GPU_TEMP_HYST, (int)CPU_TEMP_HYST.NoOfSlider));
                    VRM_TEMP_HYST.SetGraphValue(EC.DirectECReadArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.VRM_TEMP_HYST, (int)CPU_TEMP_HYST.NoOfSlider));
                }
                //DrawGraph();
            }
        }));

    }


    static void onFanAccChange(object sender, EventArgs e)
    {
        if (((DraggableGraph)sender).EnableEdit && WinRing.WinRingInitOk)
        {
            byte Target = EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN_CUR_POINT);
            EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN1_CUR_ACC, ((DraggableGraph)sender).GetGraphValue()[Target]);
            EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN2_CUR_ACC, ((DraggableGraph)sender).GetGraphValue()[Target]);
            EC.DirectECWriteArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN_ACC_BASE, ((DraggableGraph)sender).GetGraphValue());
        }
    }
    static void onFanDecChange(object sender, EventArgs e)
    {
        if (((DraggableGraph)sender).EnableEdit && WinRing.WinRingInitOk)
        {
            byte Target = EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN_CUR_POINT);
            EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN1_CUR_DEC, ((DraggableGraph)sender).GetGraphValue()[Target]);
            EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN2_CUR_DEC, ((DraggableGraph)sender).GetGraphValue()[Target]);
            EC.DirectECWriteArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN_DEC_BASE, ((DraggableGraph)sender).GetGraphValue());
        }
    }
    static void onFanLeftChange(object sender, EventArgs e)
    {
        if (((DraggableGraph)sender).EnableEdit && WinRing.WinRingInitOk)
        {

            EC.DirectECWriteArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN1_BASE, ((DraggableGraph)sender).GetGraphValue());
            byte Target = EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, 0xC5FC);
            EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, 0xC5FC - 0x18, Target);
            EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.DCR5, (byte)(Target * 255 / 45));
        }
    }
    static void onFanRightChange(object sender, EventArgs e)
    {
        if (((DraggableGraph)sender).EnableEdit && WinRing.WinRingInitOk)
        {
            EC.DirectECWriteArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.FAN2_BASE, ((DraggableGraph)sender).GetGraphValue());
            byte Target = EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, 0xC5FD);
            EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, 0xC5FD - 0x18, Target);
            EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.DCR4, (byte)(Target * 255 / 45));
        }
    }

    static void OnCPU_TempChange(object sender, EventArgs e)
    {
        if (((DraggableGraph)sender).EnableEdit && WinRing.WinRingInitOk)
        {
            EC.DirectECWriteArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.CPU_TEMP, ((DraggableGraph)sender).GetGraphValue());
        }
    }

    static void OnGPU_TempChange(object sender, EventArgs e)
    {
        if (((DraggableGraph)sender).EnableEdit && WinRing.WinRingInitOk)
        {
            EC.DirectECWriteArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.GPU_TEMP, ((DraggableGraph)sender).GetGraphValue());
        }
    }

    static void OnVRM_TempChange(object sender, EventArgs e)
    {
        if (((DraggableGraph)sender).EnableEdit && WinRing.WinRingInitOk)
        {
            EC.DirectECWriteArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.VRM_TEMP, ((DraggableGraph)sender).GetGraphValue());
        }
    }

    static void OnCPU_HYST_TempChange(object sender, EventArgs e)
    {
        if (((DraggableGraph)sender).EnableEdit && WinRing.WinRingInitOk)
        {
            EC.DirectECWriteArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.CPU_TEMP_HYST, ((DraggableGraph)sender).GetGraphValue());
        }
    }

    static void OnGPU_HYST_TempChange(object sender, EventArgs e)
    {
        if (((DraggableGraph)sender).EnableEdit && WinRing.WinRingInitOk)
        {
            EC.DirectECWriteArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.GPU_TEMP_HYST, ((DraggableGraph)sender).GetGraphValue());
        }
    }

    static void OnVRM_HYST_TempChange(object sender, EventArgs e)
    {
        if (((DraggableGraph)sender).EnableEdit && WinRing.WinRingInitOk)
        {
            EC.DirectECWriteArray((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.VRM_TEMP_HYST, ((DraggableGraph)sender).GetGraphValue());
        }
    }

    private void _fan_point_no_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        FanRight.NoOfSlider = (int)_fan_point_no.Value;
        FanLeft.NoOfSlider = (int)_fan_point_no.Value;
        FanAcc.NoOfSlider = (int)_fan_point_no.Value;
        FanDec.NoOfSlider = (int)_fan_point_no.Value;
        CPU_TEMP.NoOfSlider = (int)_fan_point_no.Value;
        GPU_TEMP.NoOfSlider = (int)_fan_point_no.Value;
        VRM_TEMP.NoOfSlider = (int)_fan_point_no.Value;
        CPU_TEMP_HYST.NoOfSlider = (int)_fan_point_no.Value;
        GPU_TEMP_HYST.NoOfSlider = (int)_fan_point_no.Value;
        VRM_TEMP_HYST.NoOfSlider = (int)_fan_point_no.Value;


        if (IsEditMode && WinRing.WinRingInitOk)
        {
            if (args.NewValue > args.OldValue)
            {
                EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)((ushort)EC.ITE_REGISTER_MAP.FAN1_BASE + args.NewValue - 1), FanLeft.GetGraphValue()[Convert.ToInt32(args.OldValue) - 1]);
                EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)((ushort)EC.ITE_REGISTER_MAP.FAN2_BASE + args.NewValue - 1), FanRight.GetGraphValue()[Convert.ToInt32(args.OldValue) - 1]);
            }
            else
            {
                for (int i = Convert.ToInt32(args.NewValue); i < Convert.ToInt32(args.OldValue); i++)
                {
                    EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)((ushort)EC.ITE_REGISTER_MAP.FAN1_BASE + i), 0);
                    EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)((ushort)EC.ITE_REGISTER_MAP.FAN2_BASE + i), 0);
                }
            }
            EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)((ushort)EC.ITE_REGISTER_MAP.FAN_POINT), (byte)_fan_point_no.Value);
        }
    }

    private void EditMode_Checked(SplitButton sender, SplitButtonClickEventArgs args)
    {

        IsEditMode = (bool)EditButton.IsChecked;
        FanLeft.EnableEdit = IsEditMode;
        FanRight.EnableEdit = IsEditMode;
        FanDec.EnableEdit = IsEditMode;
        FanAcc.EnableEdit = IsEditMode;
        CPU_TEMP.EnableEdit = IsEditMode;
        GPU_TEMP.EnableEdit = IsEditMode;
        VRM_TEMP.EnableEdit = IsEditMode;
        CPU_TEMP_HYST.EnableEdit = IsEditMode;
        GPU_TEMP_HYST.EnableEdit = IsEditMode;
        VRM_TEMP_HYST.EnableEdit = IsEditMode;
        TempSource.IsEnabled = IsEditMode;
        _fan_point_no.IsEnabled = IsEditMode;
    }


    private void modeButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
    {
        if (modeButton.IsChecked)
        {
            if (Monotonic_inc_radio.IsChecked == true)
            {
                FanAcc.ForceMonotonicDecreasing = false;
                FanAcc.ForceMonotonicIncreasing = true;
                FanDec.ForceMonotonicDecreasing = false;
                FanDec.ForceMonotonicIncreasing = true;
            }
            else
            {
                FanAcc.ForceMonotonicDecreasing = true;
                FanAcc.ForceMonotonicIncreasing = false;
                FanDec.ForceMonotonicDecreasing = true;
                FanDec.ForceMonotonicIncreasing = false;

            }
        }
        else
        {
            FanAcc.ForceMonotonicDecreasing = false;
            FanAcc.ForceMonotonicIncreasing = false;
            FanDec.ForceMonotonicDecreasing = false;
            FanDec.ForceMonotonicIncreasing = false;
        }
    }

    private void TurboButton_Click(object sender, RoutedEventArgs e)
    {

        if (WinRing.WinRingInitOk)
        {
            if (TurboButton.IsChecked == true)
            {
                byte HI = EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, 0xC5F1);
                HI &= 0b11111100;
                EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, 0xC5f1, (byte)HI);
                EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.DCR4, 255);
                EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.DCR5, (byte)255);
            }
            else
            {
                byte HI = EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, 0xC5f1);
                HI |= 0b11;
                EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, 0xC5f1, (byte)HI);
                byte Target = EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, 0xC5FD);
                EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, 0xC5FD - 0x18, Target);
                EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.DCR4, (byte)(Target * 255 / 45));
                Target = EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, 0xC5FC);
                EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, 0xC5FC - 0x18, Target);
                EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.DCR5, (byte)(Target * 255 / 45));
            }
        }
    }


    private void TempSourceHandler(object sender, RoutedEventArgs e)
    {
        CPU_TEMP.IsEnabled = (bool)CPU_Source.IsChecked;
        GPU_TEMP.IsEnabled = (bool)GPU_Source.IsChecked;
        VRM_TEMP.IsEnabled = (bool)VRM_Source.IsChecked;
        CPU_TEMP_HYST.IsEnabled = (bool)CPU_Source.IsChecked;
        GPU_TEMP_HYST.IsEnabled = (bool)GPU_Source.IsChecked;
        VRM_TEMP_HYST.IsEnabled = (bool)VRM_Source.IsChecked;
        CPU_TEMP.Visibility = (bool)CPU_Source.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        GPU_TEMP.Visibility = (bool)GPU_Source.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        VRM_TEMP.Visibility = (bool)VRM_Source.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        CPU_TEMP_HYST.Visibility = (bool)CPU_Source.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        GPU_TEMP_HYST.Visibility = (bool)GPU_Source.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        VRM_TEMP_HYST.Visibility = (bool)VRM_Source.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        if (IsEditMode)
        {
            EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.CPU_TEMP_EN, (byte)(CPU_TEMP.IsEnabled ? 0x80 : 0));
            EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.GPU_TEMP_EN, (byte)(GPU_TEMP.IsEnabled ? 0x81 : 0));
            EC.DirectECWrite((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)EC.ITE_REGISTER_MAP.VRM_TEMP_EN, (byte)(VRM_TEMP.IsEnabled ? 0x82 : 0));

        }
    }
}
