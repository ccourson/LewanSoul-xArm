using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;

namespace xArmDotNet
{
    public class Robot
    {
        private const int VendorId = 0x0483;
        private const int ProductId = 0x5750;

        private static HidDevice _device;

        public bool IsConnected => _device==null ? false : _device.IsConnected;
        public static class Servo
        {
            
        }

        public void Initialize()
        {

        }

        /// <summary>
        /// Connect to xArm.
        /// </summary>
        /// <returns>True if connection succeeded.</returns>
        public bool Connect()
        {
            _device = HidDevices.Enumerate(VendorId, ProductId).FirstOrDefault();

            if (_device != null) // xArm found.
            {

                _device.Inserted += DeviceAttachedHandler;
                _device.Removed += DeviceRemovedHandler;

                _device.MonitorDeviceEvents = true;

                DeviceAttached?.Invoke(this, null);
            }
            return IsConnected;
        }

        /// <summary>
        /// Event handler for API hook into USB report received.
        /// </summary>
        public event EventHandler<ReportReceivedEventArgs> ReportReceived;
        /// <summary>
        /// Called when USB report received.
        /// </summary>
        /// <param name="report"></param>
        private void OnReport(HidReport report)
        {
            ReportReceived?.Invoke(this, new ReportReceivedEventArgs()
            {
                ReportId = report.ReportId,
                Data = report.Data,
                ReadStatus = (ReadStatus)report.ReadStatus
            });

            _device.ReadReport(OnReport); // Resubscribe to event.
        }

        public event EventHandler DeviceRemoved;

        private void DeviceRemovedHandler()
        {
            DeviceRemoved?.Invoke(this, null);
            _device.Dispose();
        }

        public event EventHandler DeviceAttached;

        private void DeviceAttachedHandler()
        {
            _device.ReadReport(OnReport);
        }

        /// <summary>
        /// Inherit HidDeviceData.ReadStatus.
        /// </summary>
        public enum ReadStatus
        {
            NoDataRead = HidDeviceData.ReadStatus.NoDataRead,
            NotConnected = HidDeviceData.ReadStatus.NotConnected,
            ReadError = HidDeviceData.ReadStatus.ReadError,
            Success = HidDeviceData.ReadStatus.Success,
            WaitFail = HidDeviceData.ReadStatus.WaitFail,
            WaitTimedOut = HidDeviceData.ReadStatus.WaitTimedOut
        }

        public class ReportReceivedEventArgs
        {
            public uint ReportId;
            public ReadStatus ReadStatus;
            public byte[] Data;
        }
    }
}
