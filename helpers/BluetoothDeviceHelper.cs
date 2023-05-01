using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Devices.Radios;
using Windows.Networking.Sockets;
using System.Linq;
using static StreamDeckLib.Messages.Info;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using BluetoothController.Models;

namespace BluetoothController.Helpers
{
    public class BluetoothDeviceHelper
    {
        private string targetDeviceId = string.Empty;
        private BluetoothDevice device;
        private readonly ILogger _logger;

        public List<int> SupportedModes { get; set; }


        public delegate Task BluetoothConnectionStatusChanged(BluetoothDevice sender, object args);
    

        public event BluetoothConnectionStatusChanged BluetoothConnectionStatusChangedEvent;
      

        public BluetoothDeviceHelper(ILogger logger)
        {
            _logger = logger;
        }

        // Load the Bluetooth device using the provided device ID
        public async Task<bool> LoadDeviceFromId(string _targetDeviceId)
        {
            try
            {
                targetDeviceId = _targetDeviceId;

             

                device = await BluetoothDevice.FromIdAsync(targetDeviceId);
                SupportedModes = GetConnectivityOptionsForDevice(device);

                if (device != null)
                {
                    device.ConnectionStatusChanged += Device_ConnectionStatusChanged;
                }

                return device != null;
            }  
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading Bluetooth device.");
                return false;
            }
        }

        // Get a list of supported connectivity options for the given Bluetooth device
        private static List<int> GetConnectivityOptionsForDevice(BluetoothDevice device)
        {
            List<int> output = new();

            foreach (var svc in device.RfcommServices)
            {
                output.Add(ExtractConnectivityType(svc.ConnectionServiceName));
            }

            return output;
        }

        // Extract the connectivity type from the given input string
        private static int ExtractConnectivityType(string inputString)
        {

            int output = 0;
            Match match = Regex.Match(inputString, Constants.ConnectivityTypePatternMatch);

            if (match.Success)
            {
                try
                {
                    output = Convert.ToInt32(match.Value, 16);
                }
                catch (FormatException)
                {
                    // Couldn't parse
                }
            }

            return output;
        }

        // Handle the device connection status change event
        private void Device_ConnectionStatusChanged(BluetoothDevice sender, object args)
        {
            // Trigger a callback
            BluetoothConnectionStatusChangedEvent?.Invoke(sender, args);

            _logger?.LogInformation("{Name} {ConnectionStatus}", sender.Name, sender.ConnectionStatus);
        }

        public BluetoothConnectionStatus Status => device?.ConnectionStatus ?? BluetoothConnectionStatus.Disconnected;

        // Connect to the Bluetooth device with the specified mode
        public async Task<bool> Connect(int Mode)
        {
            try
            {
                if (device.ConnectionStatus != BluetoothConnectionStatus.Connected)
                {
                    using var streamSocket = new StreamSocket();
                    var targetServiceId = RfcommServiceId.FromShortId((uint)Mode);

                    RfcommDeviceServicesResult services = await device.GetRfcommServicesForIdAsync(targetServiceId);
                    RfcommDeviceService service = services.Services.FirstOrDefault();

                    await streamSocket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName,
                    SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error connecting to Bluetooth device.");
                return false;
            }

            return false;
        }

        // Disconnect from the Bluetooth device
        //public async Task<bool> Disconnect()
        //{
        //    // TODO: Implement the disconnect functionality
        //    throw new NotImplementedException();
        //}

        // Get a dictionary of all paired Bluetooth devices
        public static async Task<Dictionary<string, string>> GetAllPairedBluetoothDevices()
        {
            var selector = BluetoothDevice.GetDeviceSelector(); // All paired devices

            var returnDict = new Dictionary<string, string>();

            var devices = await DeviceInformation.FindAllAsync(selector);

            // Add all the devices to a dictionary
            foreach (var device in devices)
            {
                returnDict.Add(device.Name, device.Id);
            }

            return returnDict;
        }


    

        // Check if Bluetooth is on and turn it on if it's off
        //private async Task<bool> CheckBluetoothOn()
        //{
        //    BluetoothAdapter adapter = await BluetoothAdapter.GetDefaultAsync();

        //    if (adapter is not null && btRadio is null)
        //    {
        //        btRadio = await adapter.GetRadioAsync();
        //        btRadio.StateChanged += Radio_StateChanged;
        //    }

        //    if (btRadio.State == RadioState.Off)
        //    {
        //     //   await btRadio.SetStateAsync(RadioState.On);

        //        _logger?.LogInformation("Bluetooth radio is off {@btRadio}",btRadio);
        //        return false;
        //    }
            

        //    return true;
        //}
    }
}
