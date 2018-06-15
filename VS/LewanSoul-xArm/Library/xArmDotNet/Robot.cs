using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using HidLibrary;

namespace xArmDotNet
{
    public class Robot
    {
        private const int VendorId = 0x0483;
        private const int ProductId = 0x5750;

        private static HidDevice _device;

        public bool IsConnected { get; set; }

        public bool Connect()
        {
            IsConnected = false;
            _device = HidDevices.Enumerate(VendorId, ProductId).FirstOrDefault();

            if (_device != null)
            {
                IsConnected = true;

                _device.Inserted += DeviceAttachedHandler;
                _device.Removed += DeviceRemovedHandler;

                _device.MonitorDeviceEvents = true;

                _device.ReadReport(OnReport);
            }
            return IsConnected;
        }

        private const int WM_DEVICECHANGE = 0x0219;                 // device change event 
        private const int DBT_DEVICEARRIVAL = 0x8000;               // system detected a new device 
        private const int DBT_DEVICEREMOVEPENDING = 0x8003;         // about to remove, still available 
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;        // device is gone 

        protected override void WndProc(ref Message m)
        {
            if ()
            {

            }

            base.WndProc(ref m);


        }
    }
}
