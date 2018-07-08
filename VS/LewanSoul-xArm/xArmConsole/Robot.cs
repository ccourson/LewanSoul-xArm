using System;
using System.Diagnostics;
using System.Linq;
using Windows.Storage.Streams;
using HidLibrary;

namespace xArmDotNet
{
    public class Robot
    {
        public ushort vendorId = 0x0483;
        public ushort productId = 0x5750;
        public ushort usagePage = 0x008C;   // not used
        public ushort usageId = 0x0001;     // not used

        public bool IsConnected { get { return device != null && device.IsConnected; } }

        public static HidDevice device = null;

        public event EventHandler<OnReportReceivedEventArgs> OnReportReceived;
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

        protected void OnReport(HidReport report)
        {
            if (!device.IsConnected) { return; }

            if (report.ReadStatus != HidDeviceData.ReadStatus.Success)
            {

            }

            //Debug.WriteLine(report.GetBytes().Length.ToString() + " bytes received. Status: " + report.ReadStatus.ToString() + " ID: " + report.ReportId + " Data: " + BitConverter.ToString(report.Data));

            OnReportReceived?.Invoke(this, new OnReportReceivedEventArgs() { Data = report.Data });

            device.ReadReport(OnReport);
        }

        private void DeviceRemovedHandler()
        {
            OnDisconnected?.Invoke(this, EventArgs.Empty);
            Debug.WriteLine("Robot disconnected.");
        }

        private void DeviceAttachedHandler()
        {
            OnConnected?.Invoke(this, EventArgs.Empty);
            device.ReadReport(OnReport);
            Debug.WriteLine("Robot connected.");
        }

        private void SendHidReport(RobotCommand command, IBuffer parameters)
        {
            DataWriter dataWriter = new DataWriter();
            dataWriter.WriteByte(0);        // packet id
            dataWriter.WriteUInt16(0x5555); // header
            dataWriter.WriteByte((byte)(parameters.Length + 5));
            dataWriter.WriteByte((byte)command);
            dataWriter.WriteBuffer(parameters);

            IBuffer buffer = dataWriter.DetachBuffer();

            byte[] bytes = new byte[buffer.Length];
            DataReader.FromBuffer(buffer).ReadBytes(bytes);

            device.Write(bytes, 10);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="servos"></param>
        public void GetServoOffsets(params int[] servos)
        {
            if (servos.Length == 0) throw new ArgumentNullException("At least one servo must be specified.");

            DataWriter dataWriter = new DataWriter();
            dataWriter.WriteByte((byte)servos.Length);         
            dataWriter.WriteBytes(servos.Select(b => (byte)b).ToArray());

            SendHidReport(RobotCommand.ServoOffsetRead, dataWriter.DetachBuffer());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="servos"></param>
        public async System.Threading.Tasks.Task GetServoAxesAsync(params int[] servos)
        {
            if (servos.Length == 0) throw new ArgumentNullException("At least one servo must be specified.");

            DataWriter dataWriter = new DataWriter();
            dataWriter.WriteByte((byte)servos.Length);
            dataWriter.WriteBytes(servos.Select(b => (byte)b).ToArray());

            SendHidReport(RobotCommand.ServoPositionRead, dataWriter.DetachBuffer());
            var data = await device.ReadAsync(300);
            Console.WriteLine("*** data.Status: " + data.Status.ToString());
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
