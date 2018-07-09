using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Windows.Storage.Streams;
using System.Windows.Controls;
using xArmDotNet;
using System.Windows.Data;
using System.Globalization;
using System.Linq;
using HidLibrary;

namespace xArmConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Robot robot = new Robot();

        List<DispatcherTimer> dispatchTimers = new List<DispatcherTimer>();
        List<Line> blips = new List<Line>();
        const int defaultPingInterval = 6;
        int canvasDivisions = 10;
        double canvasHorizontalDivisionSize;
        double canvasVerticalDivisionSize;
        private int PingCount;
        const int rxTimeout = 300;  // 300ms

        public MainWindow()
        {
            InitializeComponent();
            InitializeDispatcherTimers();

            robot.OnConnected += Robot_OnConnected;
            robot.OnDisconnected += Robot_OnDisconnected;
        }

        private void InitializeDispatcherTimers()
        {
            dispatchTimers.Add(new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromMilliseconds(15),
                Tag = new[] { DateTime.Now }
            });
            dispatchTimers.Last().Tick += MainWindow_16msTick;

            dispatchTimers.Add(new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromMilliseconds(500),
                Tag = new[] { DateTime.Now, DateTime.Now }
            });
            dispatchTimers.Last().Tick += MainWindow_200msTick;

            dispatchTimers.ForEach(t => t.Start());
        }

        private void MainWindow_16msTick(object sender, EventArgs e)
        {
            UpdateStatsLabels((DispatcherTimer)sender, 0, LabelFps);

            StatusCanvas.Children.Clear(); // clean canvas

            blips.RemoveAll(l => l.X1 < 1); // Remove old blips.

            foreach (var item in blips) // Horizontal scroll animation
            {
                StatusCanvas.Children.Add(item); // paint blips
                item.X1 = item.X2 -= 1.5; // animation rate
            }
        }

        private void MainWindow_200msTick(object sender, EventArgs e)
        {
            UpdateStatsLabels((DispatcherTimer)sender, 0, LabelPps, false);

            if (robot.IsConnected)
            {
                if (PingCount == 0)
                {
                    DrawStatusCanvasTransmitCanvasBlip(Brushes.Blue, 2);
                    ((DateTime[])dispatchTimers[1].Tag)[1] = DateTime.Now;
                    robot.GetServoPositions(new[] { 1, 2, 3, 4, 5, 6 }, GetServoAxes_ReadCallback);
                    PingCount++;
                }
                else
                {
                    DrawStatusCanvasTransmitCanvasBlip(Brushes.Yellow, 1);
                }
            }
            else
            {
                DrawStatusCanvasTransmitCanvasBlip(Brushes.Red, 2);
                PingCount = 0;
            }
        }

        private void UpdateStatsLabels(DispatcherTimer dispatcherTimer, int channel, Label label, bool mode_ms = false)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                DateTime now = DateTime.Now;

                double ms = now.Subtract((dispatcherTimer.Tag as DateTime[])[channel]).Milliseconds; // ms since last time
                double diff = ms - Convert.ToDouble(label.Tag); // positive if change is shorter period

                label.Tag = Convert.ToDouble(label.Tag) + diff * (mode_ms ? 0.2 : 0.05); // operator precidence!
                label.Content = mode_ms ? ((double)label.Tag).ToString("N0") + "ms" : (1000.0 / (double)label.Tag).ToString("N0");

                (dispatcherTimer.Tag as DateTime[])[channel] = now; // another nice place to keep private bits of data
            }));
        }

        private void DrawStatusCanvasTransmitCanvasBlip(Brush brush, double width = 1)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Line line = new Line()
                {
                    Stroke = brush,
                    StrokeThickness = width,
                    X1 = StatusCanvas.ActualWidth - 2,
                    X2 = StatusCanvas.ActualWidth - 2,
                    Y1 = 2,
                    Y2 = StatusCanvas.ActualHeight / 2 - 2
                };

                blips.Add(line);
            }));
        }

        private void DrawStatusCanvasReceiveCanvasBlip(Brush brush, double width = 1)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => 
            {
                Line line = new Line()
                {
                    StrokeThickness = width,
                    Stroke = brush,
                    X1 = StatusCanvas.ActualWidth - 2,
                    X2 = StatusCanvas.ActualWidth - 2,
                    Y1 = StatusCanvas.ActualHeight - 3,
                    Y2 = StatusCanvas.ActualHeight / 2 + 1
                };

                blips.Add(line);
            }));
        }

        private void Robot_OnDisconnected(object sender, EventArgs e)
        {

        }

        private void Robot_OnConnected(object sender, EventArgs e)
        {
            
        }

        private void GetServoAxes_ReadCallback(HidDeviceData data)
        {
            UpdateStatsLabels(dispatchTimers[1], 1, LabelPing, true);

            DataWriter dataWriter = new DataWriter();
            dataWriter.WriteBytes(data.Data);
            
            DataReader dataReader = DataReader.FromBuffer(dataWriter.DetachBuffer());
            dataReader.ByteOrder = ByteOrder.LittleEndian;

            if (data.Status == HidDeviceData.ReadStatus.Success)
            {
                if (dataReader.ReadByte() == 0 && dataReader.ReadUInt16() == 0x5555)
                {
                    int length = dataReader.ReadByte();

                    RobotCommand command = (RobotCommand)dataReader.ReadByte();

                    Dispatcher.Invoke(new Action(() =>
                    {
                        int count = dataReader.ReadByte();

                        DrawStatusCanvasReceiveCanvasBlip(PingCount == 1 ? Brushes.Green : Brushes.Yellow, PingCount == 1 ? 2 : 1);
                        PingCount--;

                        while (count-- > 0)
                        {
                            int servo = dataReader.ReadByte();
                            int angle = dataReader.ReadInt16();
                            var grid = (Grid)RobotAxisControlGrid.Children[servo - 1];
                            RobotAxisControl control = (RobotAxisControl)grid.Children[1];
                            control.sliderAngle.Value = angle;
                        }
                    }));
                }
                else
                {
                    DrawStatusCanvasReceiveCanvasBlip(Brushes.Red, 2);
                }
            }
            else
            {
                DrawStatusCanvasReceiveCanvasBlip(Brushes.Orange, 2);
            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            robot.GetServoOffsets(new[] { 1, 2, 3, 4, 5, 6 });
        }

        private void Window_LayoutUpdated(object sender, EventArgs e)
        {
            canvasHorizontalDivisionSize = StatusCanvas.ActualWidth / canvasDivisions;
            canvasVerticalDivisionSize = StatusCanvas.ActualHeight / canvasDivisions;
        }

        private void MyButtonControl_OnClick(object sender, EventArgs e)
        {

        }

        private void TabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem tabItem;

            if (e.RemovedItems.Count > 0)
            {
                tabItem = (TabItem)e.RemovedItems[0];
                ((Label)tabItem.Header).Background = new SolidColorBrush((Color)FindResource("ButtonBackgroundColor"));
            }

            tabItem = (TabItem)e.AddedItems[0];
            ((Label)tabItem.Header).Background = new SolidColorBrush((Color)FindResource("WindowBorder"));
        }

        private void ButtonRetrieveServoHomeDefaultValues_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonSetServoHomeDefaultValues_Click(object sender, RoutedEventArgs e)
        {
            ServoAxisHomePosition1.Text = "500";
            ServoAxisHomePosition2.Text = "500";
            ServoAxisHomePosition3.Text = "500";
            ServoAxisHomePosition4.Text = "500";
            ServoAxisHomePosition5.Text = "500";
            ServoAxisHomePosition6.Text = "500";
        }

        private void Scope_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            StatusGrid.Children.Clear();
            StatusGrid.Children.Add(new Rectangle() { Width = StatusGrid.ActualWidth - 1, Height = StatusGrid.ActualHeight - 1, Stroke = Brushes.White, Opacity = 0.3 });

            StatusGrid.Children.Add(new Line() { X1 = 1, Y1 = StatusGrid.ActualHeight * 0.25, Y2 = StatusGrid.ActualHeight * 0.25, X2 = StatusGrid.ActualWidth - 2, Stroke = Brushes.White, Opacity = 0.2});
            StatusGrid.Children.Add(new Line() { X1 = 1, Y1 = StatusGrid.ActualHeight * 0.50, Y2 = StatusGrid.ActualHeight * 0.50, X2 = StatusGrid.ActualWidth - 2, Stroke = Brushes.White, Opacity = 0.3 });
            StatusGrid.Children.Add(new Line() { X1 = 1, Y1 = StatusGrid.ActualHeight * 0.75, Y2 = StatusGrid.ActualHeight * 0.75, X2 = StatusGrid.ActualWidth - 2, Stroke = Brushes.White, Opacity = 0.2 });

            for (double i = StatusGrid.ActualWidth - 20; i > 0; i -= 20)
            {
                StatusGrid.Children.Add(new Line() { X1 = i, X2 = i, Y1 = 1, Y2 = StatusGrid.ActualHeight - 2, Stroke = Brushes.White, Opacity = 0.2 });
            }

            double scale = e.NewSize.Width - e.PreviousSize.Width;
            foreach (var x in blips)
            {
                x.X1 += scale;
                x.X2 = x.X1;
            }
            blips.RemoveAll(l => l.X1 < 1);
        }

        private void ControlServosHome_Click(object sender, RoutedEventArgs e)
        {
            DrawStatusCanvasTransmitCanvasBlip(Brushes.Green, 2);

            robot.SetServoPositions(new ushort?[] { 500, 500, 500, 500, 500, 500 }, (data) =>
            {
                DrawStatusCanvasTransmitCanvasBlip(data.Status == HidDeviceData.ReadStatus.Success ? Brushes.Green : Brushes.Red, 2);
            });
        }

        private void ControlServosHome_Callback(HidDeviceData data)
        {
            DrawStatusCanvasReceiveCanvasBlip(data.Status == HidDeviceData.ReadStatus.Success ? Brushes.Green : Brushes.Red, 2);
        }
    }

    class Factor : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count() == 2)
            {
                return ((double)values[0] * (double)values[1]);
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
