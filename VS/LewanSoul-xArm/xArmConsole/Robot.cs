using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using HidLibrary;
using System.IO;

namespace xArmDotNet
{
    public class Robot
    {
        public ushort vendorId = 0x0483;
        public ushort productId = 0x5750;
        public ushort usagePage = 0x008C;   // not used
        public ushort usageId = 0x0001;     // not used

        public bool IsConnected { get { return device != null && device.IsConnected; } }

        public static HidDevice device;

        public Robot()
        {
            device = HidDevices.Enumerate(vendorId, productId).FirstOrDefault();

            if (device != null)
            {
                device.Inserted += DeviceAttachedHandler;
                device.Removed += DeviceRemovedHandler;

                device.MonitorDeviceEvents = true;

                device.ReadReport(OnReport);
                Debug.WriteLine("HidLibrary installed.");
            }
        }

        private void OnReport(HidReport report)
        {
            if (!device.IsConnected) { return; }

            Debug.WriteLine("HID Input Report: " + report.Data.ToString());
            Debug.WriteLine("Total number of bytes received: " + report.Data.Length.ToString());

            foreach (var item in report.Data)
            {
                Debug.Write(string.Format("{0:X}-", item));
            }
            Debug.Write("");

            device.ReadReport(OnReport);
        }

        private void DeviceRemovedHandler()
        {
            Debug.WriteLine("Robot disconnected.");
        }

        private void DeviceAttachedHandler()
        {
            Debug.WriteLine("Robot connected.");
            //foreach (var item in device.Capabilities.)
            //{
            //    Debug.WriteLine("   " + item.Key.ToString() + ": " + Convert.ToString(item.Value));
            //}


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

            HidReport report = device.CreateReport();

            DataWriter dataWriter = new DataWriter() { ByteOrder = ByteOrder.LittleEndian };
            dataWriter.WriteByte(0); // packet id
            dataWriter.WriteUInt16(0x5555); // header
            dataWriter.WriteByte((byte)(parameters.Length + 2));
            dataWriter.WriteByte((byte)RobotCommand.BusServoInfoRead);
            dataWriter.WriteBytes(parameters);

            byte[] bytes = new byte[device.Capabilities.OutputReportByteLength - 1];

            DataReader dataReader = new DataReader((IInputStream)dataWriter.DetachStream()) { ByteOrder = ByteOrder.LittleEndian };
            dataReader.ReadBytes(bytes);

            await device.WriteAsync(bytes);

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
