using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace SerialPort_TestBench
{
    class Program
    {
        static void Main(string[] args)
        {
            SerialPort serialPort = new SerialPort("COM4", 9600, Parity.None, 8, StopBits.Two);

            serialPort.Open();
            //* 0x03 (byte)count (ushort)milliseconds { (byte)servo (ushort)position[FF00=null] }
            byte[] vs = new byte[] { 0x55, 0x55, 8, 3, 1, 0xe8, 3, 1, 0xbf, 1 };
            serialPort.Write(vs, 0, vs.Length);
            
            while (true)
            {
                int data = serialPort.ReadByte();
                if (data > 0)
                {
                    Debug.Write($"{data} ");
                }
            }
        }
    }
}
