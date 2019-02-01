using System;
using System.Collections.Generic;

namespace UsbHidLibrary
{
    internal class UsbHidDevice
    {
        public int Hid { get; internal set; }
        public int Vid { get; internal set; }
        private bool attached;
        public bool Attached => EnumerateDevices().GetEnumerator().MoveNext();

        internal bool Connect()
        {
            if (Attached)
            {
                return true;
            }
            return false;
        }
    }
}