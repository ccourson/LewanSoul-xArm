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
        bool polling;

        const int rxTimeout = 300;  // 300ms

        public MainWindow()
        {
            InitializeComponent();
            InitializeDispatcherTimers();

            robot.OnConnected += Robot_OnConnected;
            robot.OnDisconnected += Robot_OnDisconnected;
            //robot.OnReportReceived += Robot_OnReportReceived;
        }

        private void InitializeDispatcherTimers()
        {
            dispatchTimers.Add(new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromMilliseconds(15),
                Tag = DateTime.Now
            });
            dispatchTimers.Last().Tick += MainWindow_16msTick;

            dispatchTimers.Add(new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromMilliseconds(280),
                Tag = DateTime.Now
            });
            dispatchTimers.Last().Tick += MainWindow_300msTick;

            dispatchTimers.ForEach(t => t.Start());
        }

        private void MainWindow_16msTick(object sender, EventArgs e)
        {
            UpdateTickRateLabel((DispatcherTimer)sender, LabelFps);

            StatusCanvas.Children.Clear();

            blips.RemoveAll(l => l.X1 < 1); // Remove old blips.

            foreach (var item in blips) // Slide blips over 1 pixel
            {
                StatusCanvas.Children.Add(item);
                item.X1 -= 1.0;
                item.X2 -= 1.0;
            }
        }

        private void MainWindow_300msTick(object sender, EventArgs e)
        {
            UpdateTickRateLabel((DispatcherTimer)sender, LabelPing, true);

            if (robot.IsConnected)
            {
                UpdateUIAxes();
                TransmitCanvasBlip(polling ? Brushes.Orange : Brushes.Blue, 0.8);
                polling = true;
            }
            else
            {
                TransmitCanvasBlip(Brushes.Red, 0.8);
                polling = false;
            }
        }

        private void UpdateTickRateLabel(DispatcherTimer dispatcherTimer, Label label, bool mode = false)
        {
            DateTime now = DateTime.Now;
            double ms = now.Subtract((DateTime)dispatcherTimer.Tag).Milliseconds; // ms since last time
            double diff = ms - Convert.ToDouble(label.Tag); // positive if change is shorter period
            label.Tag = Convert.ToDouble(label.Tag) + diff * (mode ? 0.2 :0.05); // operator precidence!
            label.Content = mode ? ((double)label.Tag).ToString("N0") + "ms" : (1000.0 / (double)label.Tag).ToString("N0");
            dispatcherTimer.Tag = now; // another nice place to keep private bits of data
        }

        private void UpdateUIAxes()
        {
            robot.GetServoAxesAsync(1, 2, 3, 4, 5, 6);
        }

        private void TransmitCanvasBlip(Brush brush, double amplitude, double opacity = 1)
        {
            int verticalOffset = (int)(amplitude * canvasVerticalDivisionSize * canvasDivisions) / 2;
            Line line = new Line()
            {
                Opacity = opacity,
                Stroke = brush,
                X1 = StatusCanvas.ActualWidth - 2,
                X2 = StatusCanvas.ActualWidth - 2,
                Y1 = 2,
                Y2 = StatusCanvas.ActualHeight / 2
            };

            blips.Add(line);
        }

        private void ReceiveCanvasBlip(Brush brush, double amplitude, double opacity = 1)
        {
            int verticalOffset = (int)(amplitude * canvasVerticalDivisionSize * canvasDivisions) / 2;
            Line line = new Line()
            {
                Opacity = opacity,
                Stroke = brush,
                X1 = StatusCanvas.ActualWidth - 2,
                X2 = StatusCanvas.ActualWidth - 2,
                Y1 = StatusCanvas.ActualHeight - 2,
                Y2 = StatusCanvas.ActualHeight / 2
            };

            blips.Add(line);
        }


        private void Robot_OnDisconnected(object sender, EventArgs e)
        {

        }

        private void Robot_OnConnected(object sender, EventArgs e)
        {
            
        }

        private void Robot_OnReportReceived(object sender, OnReportReceivedEventArgs e)
        {
            DataWriter dataWriter = new DataWriter();
            dataWriter.WriteBytes(e.Data);
            
            DataReader dataReader = DataReader.FromBuffer(dataWriter.DetachBuffer());
            dataReader.ByteOrder = ByteOrder.LittleEndian;

            if (dataReader.ReadUInt16() == 0x5555)
            {
                int length = dataReader.ReadByte();

                RobotCommand command = (RobotCommand)dataReader.ReadByte();

                if (command == RobotCommand.ServoOffsetRead)
                {
                    int count = dataReader.ReadByte();
                    for (int i = 0; i < count; i++)
                    {
                        Console.WriteLine("Motor " + dataReader.ReadByte() + ": " + (sbyte)dataReader.ReadByte());
                    }
                }
                if (command == RobotCommand.ServoPositionRead)
                {
                    Dispatcher.Invoke(new Action(() => 
                    {
                        int count = dataReader.ReadByte();

                        ReceiveCanvasBlip(polling ? Brushes.Green : Brushes.GreenYellow, 0.8);

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
                polling = false;
            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            robot.GetServoOffsets(1, 2, 3, 4, 5, 6);
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

            StatusGrid.Children.Add(new Line() { X1 = 1, Y1 = StatusGrid.ActualHeight * 0.25 - 2, Y2 = StatusGrid.ActualHeight * 0.25 - 2, X2 = StatusGrid.ActualWidth - 2, Stroke = Brushes.White, Opacity = 0.2});
            StatusGrid.Children.Add(new Line() { X1 = 1, Y1 = StatusGrid.ActualHeight * 0.50 - 2, Y2 = StatusGrid.ActualHeight * 0.50 - 2, X2 = StatusGrid.ActualWidth - 2, Stroke = Brushes.White, Opacity = 0.3 });
            StatusGrid.Children.Add(new Line() { X1 = 1, Y1 = StatusGrid.ActualHeight * 0.75 - 2, Y2 = StatusGrid.ActualHeight * 0.75 - 2, X2 = StatusGrid.ActualWidth - 2, Stroke = Brushes.White, Opacity = 0.2 });

            for (double i = StatusGrid.ActualWidth - 20; i > 0; i -= 20)
            {
                StatusGrid.Children.Add(new Line() { X1 = i, X2 = i, Y1 = 1, Y2 = StatusGrid.ActualHeight - 2, Stroke = Brushes.White, Opacity = 0.2 });
            }

            //blips.Clear();

            double scale = e.NewSize.Width - e.PreviousSize.Width;
            foreach (var x in blips)
            {
                x.X1 += scale;
                x.X2 = x.X1;
            }
            blips.RemoveAll(l => l.X1 < 1);
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
