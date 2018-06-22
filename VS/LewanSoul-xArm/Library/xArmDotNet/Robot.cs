using System;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;


namespace xArmDotNet
{
    public class Robot
    {
        public Robot()
        {
            EnumerateHidDevicesAsync();
        }

        private async void EnumerateHidDevicesAsync()
        {
            ushort vendorId = 0x0483;
            ushort productId = 0x5750;
            ushort usagePage = 0x008C;
            ushort usageId = 0x0001;

            string selector = HidDevice.GetDeviceSelector(usagePage, usageId, vendorId, productId);
            var devices = await DeviceInformation.FindAllAsync(selector);

            if (devices.Any())
            {
                // At this point the device is available to communicate with
                // So we can send/receive HID reports from it or 
                // query it for control descriptions.
                Debug.WriteLine("HID devices found: " + devices.Count);

                foreach (var item in devices.ElementAt(0).Properties.ToList())
                {
                    Debug.WriteLine("   " + item.Key.ToString() + ": " + Convert.ToString(item.Value));
                }

                // Open the target HID device.
                HidDevice device = await HidDevice.FromIdAsync(devices.ElementAt(0).Id, FileAccessMode.ReadWrite);


                if (device != null)
                {
                    Debug.WriteLine("Installing report handler.");

                    // Input reports contain data from the device.
                    device.InputReportReceived += async (sender, args) =>
                    {
                        HidInputReport inputReport = args.Report;
                        IBuffer buffer = inputReport.Data;

                        Debug.WriteLine("HID Input Report: " + inputReport.ToString());
                        Debug.WriteLine("Total number of bytes received: " + buffer.Length.ToString());
                    };
                }
            }
            else
            {
                // There were no HID devices that met the selector criteria.
                Debug.WriteLine("HID device not found");
            }
        }
    }
}
