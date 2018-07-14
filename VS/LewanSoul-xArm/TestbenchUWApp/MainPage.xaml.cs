using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestbenchUWApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Robot robot;

        public MainPage()
        {
            InitializeComponent();

            robot = new Robot();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MyTextBlock.Text += MyTextBox.Text + "\r\n";
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key.Equals(VirtualKey.Enter))
            {
                Button_Click(sender, new RoutedEventArgs());
                MyScrollViewer.UpdateLayout();
                MyScrollViewer.ChangeView(0, double.MaxValue, 0);
                e.Handled = true;
            }
        }
    }

    public class Robot
    {
        public ushort vendorId = 0x0483;
        public ushort productId = 0x5750;
        public ushort usagePage = 0x008C;
        public ushort usageId = 0x0001;

        public bool Connected { get { return device != null; } }

        string deviceSelector;
        DeviceWatcher deviceWatcher;
        public static HidDevice device;

        public Robot()
        {
            deviceSelector = HidDevice.GetDeviceSelector(usagePage, usageId, vendorId, productId);
            deviceWatcher = DeviceInformation.CreateWatcher(deviceSelector);
            deviceWatcher.Added += DeviceWatcher_DeviceAdded;
            deviceWatcher.Removed += DeviceWatcher_DeviceRemoved;
            deviceWatcher.Start();
            Debug.WriteLine("DeviceWatcher installed.");
        }

        private void DeviceWatcher_DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            Debug.WriteLine("Robot disconnected.");
            device.Dispose();
        }

        private async void DeviceWatcher_DeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            Debug.WriteLine("Robot connected.");
            foreach (var item in args.Properties.ToList())
            {
                Debug.WriteLine("   " + item.Key.ToString() + ": " + Convert.ToString(item.Value));
            }

            // Open the target HID device.
            device = await HidDevice.FromIdAsync(args.Id, FileAccessMode.ReadWrite);

            if (device != null)
            {
                Debug.WriteLine("Installing report handler.");

                // Input reports contain data from the device.
                device.InputReportReceived += Device_InputReportReceived;
            }
        }

        private void Device_InputReportReceived(HidDevice sender, HidInputReportReceivedEventArgs args)
        {
            HidInputReport report = args.Report;
            IBuffer buffer = report.Data;
            int ms = DateTime.Now.Subtract(now).Milliseconds;
            Debug.WriteLine("RX - {0} bytes received in {1}ms.", buffer.Length, ms);
            Debug.WriteLine(BitConverter.ToString(report.Data.ToArray()));
        }

        public void Connect()
        {

        }

        public void Disconnect()
        {

        }

        DateTime now;
        public async void SendReport()
        {
            byte[] parameters = { 0, 85, 85, 9, (int)RobotCommand.BusServoInfoRead, 6, 1, 2, 3, 4, 5, 6 };

            HidOutputReport report = device.CreateOutputReport();
            Array.Resize(ref parameters, (int)report.Data.Length);
            report.Data = parameters.AsBuffer();

            now = DateTime.Now;
            await device.SendOutputReportAsync(report);
            int ms = DateTime.Now.Subtract(now).Milliseconds;

            Debug.WriteLine("TX - {0} bytes sent in {1}ms.", report.Data.Length.ToString(), ms);
        }

        internal async void SendReport(IBuffer buffer)
        {
            byte[] parameters = buffer.ToArray();

            HidOutputReport report = device.CreateOutputReport();
            Array.Resize(ref parameters, (int)report.Data.Length);
            report.Data = parameters.AsBuffer();

            now = DateTime.Now;
            await device.SendOutputReportAsync(report);
            int ms = DateTime.Now.Subtract(now).Milliseconds;

            Debug.WriteLine("TX - {0} bytes sent in {1}ms.", report.Data.Length.ToString(), ms);
        }

        public class Parameter
        {
            public byte motor;
            public uint position;
        }

        public enum RobotCommand
        {
            ServoMove = 3,  // (byte)count (ushort)time { (byte)id (ushort)position }
            GroupRunRepeat = 5,  // (byte)group[255=all] (byte)times 
            GroupRun = 6,  // (byte)group (ushort)count[0=continuous]
            GroupStop = 7,  // -none-
            GroupErase = 8,  // (byte)group[255=all]
            GroupSpeed = 11, // (byte)group (ushort)percentage
            xServoOffsetWrite = 12,
            xServoOffsetRead = 13,
            xServoOffsetAdjust = 14,
            GetBatteryVoltage = 15, // -none-; (ushort)millivolts
            ServoOff = 20, // (byte)count { (byte)id }
            ServoPositionRead = 21, // (byte)count { (byte)id }; (byte)count { (byte)id (byte)offset }
            ServoPositionWrite = 22, // (byte)count { (byte)id }
            ServoOffsetRead = 23, // (byte)count { (byte)id }; (byte)count { (byte)id (byte)offset }
            ServoOffsetWrite = 24, // (byte)id (ushort)value
            BusServoMoroCtrl = 26, // (byte)id (byte)??? (ushort)speed
            BusServoInfoWrite = 27, // (byte)id (ushort)pos_min (ushort)pos_max (ushort)volt_min (ushort)volt_max
                                    //         (ushort)temp_max (byte)led_status (byte)led_warning
            BusServoInfoRead = 28  // -none-; (byte)id (ushort)pos_min (ushort)pos_max (ushort)volt_min (ushort)volt_max 
                                   //         (ushort)temp_max (byte)led_status (byte)led_warning (byte)dev_offset
                                   //         (ushort)pos (byte)temp (ushort)volt
        }
    }
}