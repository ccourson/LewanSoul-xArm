using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Linq;
using xArmDotNet;
using Windows.Storage.Streams;

namespace xArmConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Robot robot = new Robot();

        DispatcherTimer dispatchTimer;
        List<Line> blips = new List<Line>();
        int tick = -1;
        int pingInterval = 20;
        int canvasDivisions = 10;
        double canvasHorizontalDivisionSize;
        double canvasVerticalDivisionSize;
        bool polling;

        public MainWindow()
        {
            InitializeComponent();

            dispatchTimer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = new TimeSpan(100000) // 100ns * 1,000,000 = 100ms   .0001
            };
            dispatchTimer.Tick += DispatchTimer_Tick;
            dispatchTimer.Start();

            robot.OnConnected += Robot_OnConnected;
            robot.OnDisconnected += Robot_OnDisconnected;
            robot.OnReportReceived += Robot_OnReportReceived;
        }

        private void DispatchTimer_Tick(object sender, EventArgs e)
        {
            tick = ++tick % pingInterval;

            StatusCanvas.Children.Clear();

            DrawGridOnCanvas();

            DrawCanvasBaseline();

            if (tick++ == 0)
            {
                if (robot.IsConnected)
                {
                    UpdateUIAxes();

                    DrawCanvasBlip(polling ? Brushes.Red : (Brush)FindResource("Brush01"), 0.8);

                    polling = true;
               }
                else
                {
                    DrawCanvasBlip((Brush)FindResource("Brush05"), 0.8);
                    // set a random ping interval for visual character.
                    pingInterval = new Random().Next(8, 24);

                    polling = false;
                }
            }

            blips.RemoveAll(l => l.X1 < 1); // Remove old blips.

            foreach (var item in blips)
            {
                StatusCanvas.Children.Add(item);
                item.X1--;
                item.X2--;
            }
        }

        private void UpdateUIAxes()
        {
            robot.GetServoAxes(1, 2, 3, 4, 5, 6);
        }

        private void DrawCanvasBlip(Brush brush, double amplitude, double opacity = 1)
        {
            int verticalOffset = (int)(amplitude * canvasVerticalDivisionSize * canvasDivisions) / 2;
            Line line = new Line()
            {
                Opacity = opacity,
                Stroke = brush,
                X1 = StatusCanvas.ActualWidth - 1,
                X2 = StatusCanvas.ActualWidth - 1,
                Y1 = StatusCanvas.ActualHeight / 2 - verticalOffset,
                Y2 = StatusCanvas.ActualHeight / 2 + verticalOffset
            };

            blips.Add(line);
        }

        private void DrawCanvasBaseline()
        {
            Line baseline = new Line()
            {
                Stroke = (robot != null && robot.IsConnected) ? (Brush)FindResource("Brush01") : (Brush)FindResource("Brush04"),
                StrokeDashArray = new DoubleCollection(new double[] { 1 }),
                X1 = 0,
                X2 = StatusCanvas.ActualWidth - 1,
                Y1 = StatusCanvas.ActualHeight / 2,
                Y2 = StatusCanvas.ActualHeight / 2
            };
            StatusCanvas.Children.Add(baseline);
        }

        private void DrawGridOnCanvas()
        {
            // vertical grid
            for (int i = 0; i <= canvasDivisions; i ++)
            {
                StatusCanvas.Children.Add(new Line()
                {
                    StrokeThickness = 1,
                    Stroke = (Brush)FindResource("Brush05"),
                    X1 = i * canvasHorizontalDivisionSize,
                    X2 = i * canvasHorizontalDivisionSize,
                    Y1 = 0,
                    Y2 = StatusCanvas.ActualHeight,
                    StrokeDashArray = new DoubleCollection(new double[] { 1 })
                });
            }

            // horizontal grid
            for (int i = 0; i <= canvasDivisions; i++)
            {
                StatusCanvas.Children.Add(new Line()
                {
                    StrokeThickness = 1,
                    Stroke = (Brush)FindResource("Brush05"),
                    X1 = 0,
                    X2 = StatusCanvas.ActualWidth,
                    Y1 = i * canvasVerticalDivisionSize,
                    Y2 = i * canvasVerticalDivisionSize,
                    StrokeDashArray = new DoubleCollection(new double[] { 1 })
                });
            }
        }

        private void Robot_OnDisconnected(object sender, EventArgs e)
        {

        }

        private void Robot_OnConnected(object sender, EventArgs e)
        {
            pingInterval = 16;
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

                        DrawCanvasBlip(polling ? Brushes.Green : Brushes.Yellow, 0.5);

                        while (count-- > 0)
                        {
                            int servo = dataReader.ReadByte();
                            int angle = dataReader.ReadInt16();
                            (RobotAxisControlGrid.Children[servo - 1] as RobotAxisControl).sliderAngle.Value = angle;
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
    }
}
