using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UsbHidLibrary
{
    internal class NativeMethods
    {
        internal static readonly object DIGCF_PRESENT;

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)] //Enumerator only with null ClassGUID 
        private static extern IntPtr SetupDiGetClassDevs(IntPtr ClassGuid, string Enumerator, IntPtr hwndParent, int Flags);

        private IEnumerable<DeviceInfo> EnumerateDevices()
        {
            var devices = new List<DeviceInfo>();

            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(IntPtr.Zero, "", IntPtr.Zero, 0x12);

        }


    }
}