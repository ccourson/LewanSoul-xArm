using System;
using System.Diagnostics;
using System.Linq;
using Windows.Storage.Streams;
using HidLibrary;
using System.Collections.Generic;

namespace xArmDotNet
{
    public class Robot
    {
        private const int Timeout = 300;
        public ushort vendorId = 0x0483;
        public ushort productId = 0x5750;
        public ushort usagePage = 0x008C;   // not used
        public ushort usageId = 0x0001;     // not used

        public bool IsConnected { get { return device != null && device.IsConnected; } }

        public static HidDevice device = null;

        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;


        public Robot()
        {
            Connect();
        }

        public void Connect()
        {
            if (device == null)
            {
                device = HidDevices.Enumerate(vendorId, productId).FirstOrDefault();

                if (device != null)
                {
                    device.OpenDevice(DeviceMode.Overlapped, DeviceMode.Overlapped, ShareMode.ShareRead | ShareMode.ShareWrite);
                    device.Inserted += DeviceAttachedHandler;
                    device.Removed += DeviceRemovedHandler;
                    device.MonitorDeviceEvents = true;
                    Debug.WriteLine("HidLibrary installed.");
                }
            }
        }

        private void DeviceRemovedHandler()
        {
            OnDisconnected?.Invoke(this, EventArgs.Empty);
            Debug.WriteLine("Robot disconnected.");
        }

        private void DeviceAttachedHandler()
        {
            OnConnected?.Invoke(this, EventArgs.Empty);
            Debug.WriteLine("Robot connected.");
        }

        private void SendHidReport(RobotCommand command, IBuffer parameters)
        {
            DataWriter dataWriter = new DataWriter();
            dataWriter.WriteByte(0);        // packet id
            dataWriter.WriteUInt16(0x5555); // header
            dataWriter.WriteByte((byte)(parameters.Length + 3));
            dataWriter.WriteByte((byte)command);
            dataWriter.WriteBuffer(parameters);

            IBuffer buffer = dataWriter.DetachBuffer();

            byte[] bytes = new byte[buffer.Length];
            DataReader.FromBuffer(buffer).ReadBytes(bytes);

            device.Write(bytes, SendHidReport_WriteCallback, 10);
        }

        private void SendHidReport_WriteCallback(bool success)
        {
            if(!success)
            {
                Console.WriteLine("SendHidReport fail!");
            }
        }

        public void GetServoOffsets(int[] servos, ReadCallback callback = null)
        {
            DataWriter dataWriter = new DataWriter();
            dataWriter.WriteByte((byte)servos.Length);
            dataWriter.WriteBytes(servos.Select(i => (byte)i).ToArray());

            SendHidReport(RobotCommand.ServoOffsetRead, dataWriter.DetachBuffer());
            device.Read(callback, Timeout);
        }

        public void GetServoPositions(int[] servos, ReadCallback callback = null)
        {
            DataWriter dataWriter = new DataWriter();
            dataWriter.WriteByte((byte)servos.Length);
            dataWriter.WriteBytes(servos.Select(i => (byte)i).ToArray());

            SendHidReport(RobotCommand.ServoPositionRead, dataWriter.DetachBuffer());
            device.Read(callback, Timeout);
        }

        public void SetServoPositions(ushort?[] positions, ReadCallback callback = null)
        {
            Console.WriteLine("SetServoPositions");
            DataWriter dataWriter = new DataWriter() { ByteOrder = ByteOrder.LittleEndian };
            double c = positions.Count(d => d != null);
            dataWriter.WriteByte((byte)c);

            byte i = 1;
            foreach (var item in positions.Where(d => d != null))
            {
                dataWriter.WriteByte(i++);
                dataWriter.WriteUInt16((ushort)item);
            }

            SendHidReport(RobotCommand.ServoPositionWrite, dataWriter.DetachBuffer());
            //device.Read(callback, Timeout);
        }

        // TODO: Incomplete idea.
        public class RobotParameter
        {
            public byte motor;
            public uint position;
        }
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

    public class OnReportReceivedEventArgs : EventArgs
    {
        public byte[] Data;
    }
}
