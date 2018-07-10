using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
            robot.SendReport();
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
            HidInputReport inputReport = args.Report;
            IBuffer buffer = inputReport.Data;

            Debug.WriteLine("HID Input Report: " + inputReport.ToString());
            Debug.WriteLine("Total number of bytes received: " + buffer.Length.ToString());

            foreach (var item in buffer.ToArray())
            {
                Debug.Write(string.Format("{0:X}-", item));
            }
        }

        public void Connect()
        {

        }

        public void Disconnect()
        {

        }

        public async void SendReport()
        {
            byte[] parameters = { 85, 85, 9, 23, 6, 1, 2, 3, 4, 5, 6 };

            HidOutputReport report = device.CreateOutputReport();
            byte[] buffer = new byte[report.Data.Length];
            buffer[0] = 0; // packet id
            Array.Copy(parameters, 0, buffer, 1, parameters.Length);
            DataWriter dataWriter = new DataWriter();
            dataWriter.WriteBytes(buffer);
            report.Data = dataWriter.DetachBuffer();
            await device.SendOutputReportAsync(report);

            Debug.WriteLine(buffer.Length.ToString() + " bytes sent.");
        }

        public class Parameter
        {
            public byte motor;
            public uint position;
        }

        public enum Command
        {
            MultiServoMove = 3,
            ActionDownload = 5,
            FullActionRun = 6,
            FullActionStop = 7,
            FullActionErase = 8,
            ServoOffsetWrite = 12,
            ServoOffsetRead = 13,
            ServoOffsetAdjust = 14,
            MultiServoUnload = 20,
            MultiServoPosRead = 21,
            BusServoOffsetWrite = 22,
            BusServoOffsetRead = 23,
            BusServoOffsetAdjust = 24,
            BusServoMoroCtrl = 26, // ????
            BusServoInfoWrite = 27,
            BusServoInfoRead = 28
        }
    }
}
