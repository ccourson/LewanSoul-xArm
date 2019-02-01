using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UsbHidLibrary_TestBench
{
    class Program
    {
        static void Main(string[] args)
        {
            UsbHidLibrary.UsbHidDevice device = new UsbHidLibrary.UsbHidDevice()
            {
                Hid = 0x0000,
                Vid = 0x0000
            };

            if (device.Connect())
            {

            }
        }
    }    
}
