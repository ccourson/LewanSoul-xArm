using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using System.Diagnostics;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth.Advertisement;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestConsole.Bluetooth
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            //string filter = BluetoothDevice.GetDeviceSelectorFromDeviceName("xArm");
            //deviceWatcher = DeviceInformation.CreateWatcher(filter);
            deviceWatcher = DeviceInformation.CreateWatcher(
                "System.ItemNameDisplay:~~\"xArm\"",
                new string[] {
                    "System.Devices.Aep.DeviceAddress",
                    "System.Devices.Aep.IsConnected" },
                DeviceInformationKind.AssociationEndpoint);
            deviceWatcher.Added += DeviceWatcher_AddedAsync;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.Start();
        }

        DeviceWatcher deviceWatcher;

        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            Debug.WriteLine("xArm removed.");
        }

        private async void DeviceWatcher_AddedAsync(DeviceWatcher sender, DeviceInformation args)
        {
            Debug.WriteLine("xArm added.");

            if (args.Pairing.CanPair)
            {
                Debug.WriteLine("Set up Pairing...");

                var device = await BluetoothLEDevice.FromIdAsync(args.Id);
                var services = await device.GetGattServicesAsync();

                foreach (var service in services.Services)
                {
                    Debug.WriteLine($"Service: {service.Uuid}");
                    var characteristics = await service.GetCharacteristicsAsync();
                    foreach (var character in characteristics.Characteristics)
                    {
                        Debug.WriteLine($"Characteristic: {character.Uuid}");
                    }
                }
            }
        }
    }
}
