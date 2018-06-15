using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;

namespace TestConsole.USB
{
    class Program
    {
        private const int VendorId = 0x0483;
        private const int ProductId = 0x5750;

        private static HidDevice _device;

        static void Main(string[] args)
        {
            _device = HidDevices.Enumerate(VendorId, ProductId).FirstOrDefault();

            if (_device != null)
            {
                //_device.OpenDevice();

                _device.Inserted += DeviceAttachedHandler;
                _device.Removed += DeviceRemovedHandler;

                _device.MonitorDeviceEvents = true;

                _device.ReadReport(OnReport);

                Console.WriteLine("Reader found, press any key to exit.");
                Console.ReadKey();

                //_device.CloseDevice();

            }
            else
            {
                Console.WriteLine("Could not find reader.");
                Console.ReadKey();
            }
        }

        private static void OnReport(HidReport report)
        {
            if (!_device.IsConnected) { return; }

            //var cardData = new Data(report.Data);
            //Console.WriteLine(!cardData.Error ? Encoding.ASCII.GetString(cardData.CardData) : cardData.ErrorMessage);

            Console.WriteLine("ID: {0}", report.ReportId);

            foreach (var item in report.Data)
            {
                Console.Write("{0:X}-", item);
            }

            Console.WriteLine();
            foreach (var item in report.Data)
            {
                Console.Write("{0}-", item);
            }

            Console.WriteLine();

            _device.ReadReport(OnReport);
        }

        private static void DeviceAttachedHandler()
        {
            Console.WriteLine("Device attached.");

            //Thread.Sleep(100);

            Console.WriteLine("OutputReportByteLength: " + _device.Capabilities.OutputReportByteLength);

            byte[] data = new byte[11];

            data[0] = 85;
            data[1] = 85;
            data[2] = 9;  // length
            data[3] = 23; // command

            data[4] = 6;
            data[5] = 1;
            data[6] = 2;
            data[7] = 3;
            data[8] = 4;
            data[9] = 5;
            data[10] = 6;

            byte[] report = new byte[_device.Capabilities.OutputReportByteLength];
            int reportLength = report.Length - 1;
            int numberOfBlocks = data.Length / reportLength;
            if ((uint)(data.Length % (report.Length - 1)) > 0U)
            {
                ++numberOfBlocks;
            }

            // Send blocks
            for (int i = 0; i < numberOfBlocks; i++)
            {
                report[0] = (byte)i; // packet id

                if (data.Length - i * reportLength > reportLength)
                {
                    Array.Copy(data, i * reportLength, data, 1, reportLength);
                }
                else
                {
                    Array.Copy(data, i * reportLength, report, 1, data.Length - i * reportLength);
                }

                _device.Write(report);
            }

            _device.ReadReport(OnReport);
        }

        private static void DeviceRemovedHandler()
        {
            Console.WriteLine("Device removed.");
        }
    }
}
