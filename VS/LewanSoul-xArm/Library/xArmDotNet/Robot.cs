using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;


namespace xArmDotNet
{
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
            Debug.Write("");
        }

        public void Connect()
        {

        }

        public void Disconnect()
        {

        }

        public int[] ReadServoOffsets(int[] servos)
        {
            return null;
        }

        public async void SendReport(RobotCommand command)
        {
            byte[] parameters = { };

            HidOutputReport report = device.CreateOutputReport();

            DataWriter dataWriter = new DataWriter() { ByteOrder = ByteOrder.LittleEndian };
            dataWriter.WriteByte(0); // packet id
            dataWriter.WriteUInt16(0x5555); // header
            dataWriter.WriteByte((byte)(parameters.Length + 2));
            dataWriter.WriteByte((byte)RobotCommand.BusServoInfoRead);
            dataWriter.WriteBytes(parameters);

            IBuffer buffer = dataWriter.DetachBuffer();
            buffer.Length = report.Data.Length;

            report.Data = buffer;
            await device.SendOutputReportAsync(report);

            Debug.WriteLine(report.Data.Length.ToString() + " bytes sent.");
        }

        public class RobotParameter
        {
            public byte motor;
            public uint position;
        }

        public enum RobotCommand
        {
            ServoMove =             3,  // (byte)count (ushort)time { (byte)id (ushort)position }
            GroupRunRepeat =        5,  // (byte)group[255=all] (byte)times 
            GroupRun =              6,  // (byte)group (ushort)count[0=continuous]
            GroupStop =             7,  // -none-
            GroupErase =            8,  // (byte)group[255=all]
            GroupSpeed =            11, // (byte)group (ushort)percentage
            xServoOffsetWrite =      12, 
            xServoOffsetRead =       13, 
            xServoOffsetAdjust =     14,
            GetBatteryVoltage =     15, // -none-; (ushort)millivolts
            ServoOff =              20, // (byte)count { (byte)id }
            ServoPositionRead =     21, // (byte)count { (byte)id }; (byte)count { (byte)id (byte)offset }
            ServoPositionWrite =    22, // (byte)count { (byte)id }
            ServoOffsetRead =       23, // (byte)count { (byte)id }; (byte)count { (byte)id (byte)offset }
            ServoOffsetWrite =      24, // (byte)id (ushort)value
            BusServoMoroCtrl =      26, // (byte)id (byte)??? (ushort)speed
            BusServoInfoWrite =     27, // (byte)id (ushort)pos_min (ushort)pos_max (ushort)volt_min (ushort)volt_max
                                        //         (ushort)temp_max (byte)led_status (byte)led_warning
            BusServoInfoRead =      28  // -none-; (byte)id (ushort)pos_min (ushort)pos_max (ushort)volt_min (ushort)volt_max 
                                        //         (ushort)temp_max (byte)led_status (byte)led_warning (byte)dev_offset
                                        //         (ushort)pos (byte)temp (ushort)volt
        }
    }
}
