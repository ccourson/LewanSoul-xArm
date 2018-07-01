using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Linq;
using xArmDotNet;

namespace xArmConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Robot robot;
        public bool RobotIsConnected { get; set; }

        DispatcherTimer dispatchTimer;


        public MainWindow()
        {
            InitializeComponent();

            dispatchTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(10000)
            };
            dispatchTimer.Tick += DispatchTimer_Tick;
            dispatchTimer.Start();
        }

        static List<Line> ticks = new List<Line>();
        static int tickframe = -1;

        private void DispatchTimer_Tick(object sender, EventArgs e)
        {
            tickframe = ++tickframe % 32;

            StatusCanvas.Children.Clear();

            if (tickframe++ == 0)
            {
                if (robot == null || !robot.IsConnected)
                {
                    robot = new Robot();
                }


                Line tick = new Line()
                {
                    StrokeThickness = 1,
                    Stroke = robot.IsConnected ? (Brush)FindResource("Brush01") : (Brush)FindResource("Brush04"),
                    //StrokeDashArray = new DoubleCollection(new double[] { 1 }),
                    X1 = StatusCanvas.ActualWidth - 1,
                    Y1 = 10,
                    X2 = StatusCanvas.ActualWidth - 1,
                    Y2 = StatusCanvas.ActualHeight - 11
                };
                ticks.Add(tick);
            }

            ticks.RemoveAll(x => x.X1 < 1);

            foreach (var item in ticks)
            {
                item.X1--;
                item.X2--;
                StatusCanvas.Children.Add(item);
            }

            Line baseline = new Line()
            {
                StrokeThickness = 1,
                Stroke = robot.IsConnected ? (Brush)FindResource("Brush01") : (Brush)FindResource("Brush04"),
                StrokeDashArray = new DoubleCollection(new double[] { 1 }),
                X1 = 0,
                Y1 = StatusCanvas.ActualHeight - 10,
                X2 = StatusCanvas.ActualWidth - 1,
                Y2 = StatusCanvas.ActualHeight - 10
            };
            StatusCanvas.Children.Add(baseline);
        }
    }
}
