using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using FanControl.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Path = Microsoft.UI.Xaml.Shapes.Path;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FanControl.Control;
public sealed partial class DraggableGraph : UserControl
{
    private readonly List<Slider> _sliders = new();
    private readonly byte[] _graphValue = new byte[16];
    public event EventHandler OnGraphChange;
    public int BackGroundColumn
    {
        get => (int)GetValue(BackGroundColumnProperty);
        set => SetValue(BackGroundColumnProperty, value);
    }


    public int SliderMax
    {
        get => (int)GetValue(SliderMaxProperty);
        set => SetValue(SliderMaxProperty, value);
    }

    public int SliderValueDivisor
    {
        get => (int)GetValue(SliderValueDivisorProperty);
        set => SetValue(SliderValueDivisorProperty, value);
    }

    public int NoOfSlider
    {
        get => (int)GetValue(NoOfSliderProperty);
        set => SetValue(NoOfSliderProperty, value);
    }

    public bool EnableEdit
    {
        get => (bool)GetValue(EnableEditProperty);
        set => SetValue(EnableEditProperty, value);
    }

    public bool ForceMonotonicDecreasing
    {
        get => (bool)GetValue(ForceMonotonicDecreasingProperty);
        set => SetValue(ForceMonotonicDecreasingProperty, value);
    }
    public bool ForceMonotonicIncreasing
    {
        get => (bool)GetValue(ForceMonotonicIncreasingProperty);
        set => SetValue(ForceMonotonicIncreasingProperty, value);
    }


    public int BackGroundRow
    {
        get => (int)GetValue(BackGroundRowProperty);
        set => SetValue(BackGroundRowProperty, value);
    }

    public readonly DependencyProperty BackGroundColumnProperty = DependencyProperty.Register
           (
                "BackGroundColumn",
                typeof(int),
                typeof(DraggableGraph),
                new PropertyMetadata(1, new PropertyChangedCallback(BackGroundColumnChanged))
           );

    public readonly DependencyProperty SliderMaxProperty = DependencyProperty.Register
       (
            "SliderMax",
            typeof(int),
            typeof(DraggableGraph),
            new PropertyMetadata(1, new PropertyChangedCallback(SliderMaxChanged))
       );
    public readonly DependencyProperty SliderValueDivisorProperty = DependencyProperty.Register
       (
            "SliderValueDivisor",
            typeof(int),
            typeof(DraggableGraph),
            new PropertyMetadata(1, new PropertyChangedCallback(SliderValueDivisorChanged))
       );

    public static readonly DependencyProperty BackGroundRowProperty = DependencyProperty.Register
       (
            "BackGroundRow",
            typeof(int),
            typeof(DraggableGraph),
            new PropertyMetadata(1, new PropertyChangedCallback(BackGroundRowChanged))
       );
    public static readonly DependencyProperty NoOfSliderProperty = DependencyProperty.Register
       (
            "NoOfSlider",
            typeof(int),
            typeof(DraggableGraph),
            new PropertyMetadata(1, new PropertyChangedCallback(NoOfSliderChanged))
       );
    public static readonly DependencyProperty EnableEditProperty = DependencyProperty.Register
   (
        "EnableEdit",
        typeof(bool),
        typeof(DraggableGraph),
        new PropertyMetadata(default(bool), new PropertyChangedCallback(EditModeChanged))
   );



    public static readonly DependencyProperty ForceMonotonicIncreasingProperty = DependencyProperty.Register
(
     "ForceMonotonicIncreasing",
     typeof(bool),
     typeof(DraggableGraph),
     new PropertyMetadata(false)
);

    public static readonly DependencyProperty ForceMonotonicDecreasingProperty = DependencyProperty.Register
   (
        "ForceMonotonicDecreasing",
        typeof(bool),
        typeof(DraggableGraph),
        new PropertyMetadata(true)
   );



    public DraggableGraph()
    {
        this.InitializeComponent();
    }


    void CreateSlider()
    {
        SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        for (var i = _sliders.Count(); i < NoOfSlider; i++)
        {
            ColumnDefinition columnDefinition = new ColumnDefinition();
            columnDefinition.Width = new GridLength(1, GridUnitType.Star);
            _slidersGrid.ColumnDefinitions.Add(columnDefinition);
            var slider = new Slider
            {
                // HorizontalAlignment = HorizontalAlignment.Center,
                Orientation = Orientation.Vertical,
                // IsSnapToTickEnabled = true,
                TickFrequency = 1,
                Maximum = SliderMax, //TODO get Maximum
                IsEnabled = EnableEdit,
                TickPlacement = TickPlacement.None,
                Minimum = 0,
                Opacity = 0.5,
                //Todo ReadAnArrayFromInput
                Value = _graphValue[i],
                //Value = WinRing.WinRingInitOk ? EC.DirectECRead((byte)EC.ITE_PORT.EC_ADDR_PORT, (byte)EC.ITE_PORT.EC_DATA_PORT, (ushort)((ushort)EC.ITE_REGISTER_MAP.FAN1_BASE + i)) : 0,
                Tag = i,
            };
            slider.ValueChanged += onValueChange;
            slider.Resources["SliderTrackFill"] = solidColorBrush;
            slider.Resources["SliderTrackValueFill"] = solidColorBrush;
            slider.Resources["SliderTrackValueFillPointerOver"] = solidColorBrush;
            slider.Resources["SliderTrackValueFillDisabled"] = solidColorBrush;
            slider.Resources["SliderTrackFillPointerOver"] = solidColorBrush;
            slider.Resources["SliderTrackValueFillPressed"] = solidColorBrush;
            slider.Resources["SliderTrackFillPressed"] = solidColorBrush;
            slider.Resources["SliderTrackFillDisabled"] = solidColorBrush;
            slider.Resources["SliderThumbBackgroundDisabled"] = solidColorBrush;
            slider.Resources["SliderHeaderForegroundDisabled"] = solidColorBrush;
            slider.Resources["SliderTickBarFillDisabled"] = solidColorBrush;
            slider.Resources["SliderContainerBackground"] = solidColorBrush;
            slider.Resources["SliderContainerBackgroundDisabled"] = solidColorBrush;
            Grid.SetColumn(slider, i);
            _sliders.Add(slider);
            _slidersGrid.Children.Add(slider);
        }

        for (var i = _sliders.Count(); i > NoOfSlider; i--)
        {
            _sliders.RemoveAt(i - 1);
            _slidersGrid.Children.RemoveAt(i - 1);
            _slidersGrid.ColumnDefinitions.RemoveAt(i - 1);
        }
    }

    private void onValueChange(object sender, RangeBaseValueChangedEventArgs e)
    {
        Slider slider = (Slider)sender;

        if (ForceMonotonicIncreasing)
        {
            for (int i = 0; i < Convert.ToInt32(slider.Tag); i++)
            {
                if (_sliders[i].Value > e.NewValue)
                {
                    _sliders[i].Value = e.NewValue;
                }
            }
            for (int i = Convert.ToInt32(slider.Tag); i < NoOfSlider; i++)
            {
                if (_sliders[i].Value < e.NewValue)
                {
                    _sliders[i].Value = e.NewValue;
                }
            }
        }

        if (ForceMonotonicDecreasing)
        {
            for (int i = 0; i < Convert.ToInt32(slider.Tag); i++)
            {
                if (_sliders[i].Value < e.NewValue)
                {
                    _sliders[i].Value = e.NewValue;
                }
            }
            for (int i = Convert.ToInt32(slider.Tag); i < NoOfSlider; i++)
            {
                if (_sliders[i].Value > e.NewValue)
                {
                    _sliders[i].Value = e.NewValue;
                }
            }

        }
        GetGraphValue();
        RenderCanvas();
        OnGraphChange?.Invoke(this, null);
    }

    private Point GetThumbLocation(Slider slider)
    {
        var ratio = ((float)(slider.Value) / (slider.Maximum));
        var _maxHeight = (float)_canvas.ActualHeight;
        var delta = (float)_canvas.ActualWidth - _sliders[^1].ActualOffset.X + _sliders[0].ActualOffset.X;
        var point = new Point(slider.ActualOffset.X + delta / 2, _maxHeight * (1 - ratio));
        return point;
    }

    void RenderCanvas()
    {
        if (_sliders.Count() > 0)
        {

            var color = (SolidColorBrush)Application.Current.Resources["ControlFillColorDefaultBrush"];
            Color Accent = color.Color;
            Color Accent_Tansparent = color.Color;
            Accent.A = 10;
            Accent_Tansparent.A = 3;
            _canvas.Children.Clear();

            var points = _sliders
                .Select(GetThumbLocation)
                .Select(p => new Point(p.X, p.Y))
                .ToArray();
            // Line

            var pathSegmentCollection = new PathSegmentCollection();
            foreach (var point in points)
                pathSegmentCollection.Add(new LineSegment { Point = point });
            pathSegmentCollection.Add(new LineSegment { Point = new(_canvas.ActualWidth, points[^1].Y) });
            var pathFigure = new PathFigure { StartPoint = new(0, points[0].Y), Segments = pathSegmentCollection };

            var path = new Path
            {
                StrokeThickness = 3,
                Stroke = color,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Data = new PathGeometry { Figures = new PathFigureCollection { pathFigure } },
            };
            _canvas.Children.Add(path);

            for (var col = 0; col <= BackGroundColumn; col++)
            {
                pathSegmentCollection = new PathSegmentCollection();
                pathSegmentCollection.Add(new LineSegment { Point = new((float)col / (float)BackGroundColumn * _canvas.ActualWidth, 0) });
                pathFigure = new PathFigure { StartPoint = new((float)col / (float)BackGroundColumn * _canvas.ActualWidth, _canvas.ActualHeight), Segments = pathSegmentCollection };
                path = new Path
                {
                    StrokeThickness = 3,
                    Stroke = color,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                    Data = new PathGeometry { Figures = new PathFigureCollection { pathFigure } },
                };
                _canvas.Children.Add(path);
            }

            for (var row = 0; row <= BackGroundRow; row++)
            {
                pathSegmentCollection = new PathSegmentCollection();
                pathSegmentCollection.Add(new LineSegment { Point = new(0, (float)row / (float)BackGroundRow * _canvas.ActualHeight) });
                pathFigure = new PathFigure { StartPoint = new(_canvas.ActualWidth, (float)row / (float)BackGroundRow * _canvas.ActualHeight), Segments = pathSegmentCollection };
                path = new Path
                {
                    StrokeThickness = 3,
                    Stroke = color,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                    Data = new PathGeometry { Figures = new PathFigureCollection { pathFigure } },
                };
                _canvas.Children.Add(path);
            }

            // Fill
            pathSegmentCollection.Clear();
            var pointCollection = new PointCollection { new(0, _canvas.ActualHeight) };
            pointCollection.Add(new(0, points[0].Y));
            foreach (var point in points)
                pointCollection.Add(point);
            pointCollection.Add(new(_canvas.ActualWidth, points[^1].Y));
            pointCollection.Add(new(_canvas.ActualWidth, _canvas.ActualHeight));
            pointCollection.Add(new(points[^1].X, _canvas.ActualHeight));

            var polygon = new Polygon
            {
                Fill = color,
                Points = pointCollection
            };
            _canvas.Children.Add(polygon);
        }
    }


    public byte[] GetGraphValue()
    {
        for (var i = 0; i < NoOfSlider; i++)
        {
            _graphValue[i] = (byte)(_sliders[i].Value/ SliderValueDivisor);
        }
        return _graphValue;
    }

    public void SetGraphValue(byte[] graphValue)
    {
        if (graphValue.Length >= NoOfSlider)
        {

            for (var i = 0; i < NoOfSlider; i++)
            {
                _sliders[i].Value = graphValue[i]*SliderValueDivisor;
                _graphValue[i] =graphValue[i];
            }
        }

    }


    private static void BackGroundRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DraggableGraph)d).BackGroundRow = (int)e.NewValue;
        ((DraggableGraph)d).RenderCanvas();
    }
    private static void BackGroundColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DraggableGraph)d).BackGroundColumn = (int)e.NewValue;
        ((DraggableGraph)d).RenderCanvas();
    }

    private static void NoOfSliderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DraggableGraph)d).NoOfSlider = (int)e.NewValue;
        for (var i = (int)e.OldValue; i < (int)e.NewValue; i++)
        {
            ((DraggableGraph)d)._graphValue[i] = ((DraggableGraph)d)._graphValue[(int)e.OldValue - 1];
        }
        for (var i = (int)e.OldValue; i > (int)e.NewValue; i--)
        {
            ((DraggableGraph)d)._graphValue[i] = 0;
        }
        ((DraggableGraph)d).CreateSlider();
        ((DraggableGraph)d).RenderCanvas();
    }


    private static void SliderMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DraggableGraph)d).SliderMax = (int)e.NewValue;
        ((DraggableGraph)d).CreateSlider();
        ((DraggableGraph)d).RenderCanvas();
    }

    private static void SliderValueDivisorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DraggableGraph)d).SliderValueDivisor = (int)e.NewValue;
    }

    public void setDataName(string name)
    {
        _DataName.Text = name;

    }

    public void setDataValue(string name)
    {
        _DataValue.Text = name;
    }

    private static void EditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DraggableGraph)d).EnableEdit = (bool)e.NewValue;
        for (var i = 0; i < ((DraggableGraph)d).NoOfSlider; i++)
        {
            ((DraggableGraph)d)._sliders[i].IsEnabled = ((DraggableGraph)d).EnableEdit;
        }
        ((DraggableGraph)d).RenderCanvas();
    }

    private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        RenderCanvas();
    }
}
