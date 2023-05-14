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
using Serilog.Core;

namespace BluetoothController.Helpers
{
    public class BluetoothDeviceHelper
    {
        private string targetDeviceId = string.Empty;
        private BluetoothDevice device;
     //   private readonly ILogger _logger;

        public List<int> SupportedModes { get; set; }


        public delegate Task BluetoothConnectionStatusChanged(BluetoothDevice sender, object args);
    

        public event BluetoothConnectionStatusChanged BluetoothConnectionStatusChangedEvent;
      

        public BluetoothDeviceHelper()
        {
         
        }

        // Load the Bluetooth device using the provided device ID
        public async Task<bool> LoadDeviceFromId(string _targetDeviceId, ILogger logger)
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
                logger?.LogError(ex, "Error loading Bluetooth device.");
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

            //_logger?.LogInformation("{Name} {ConnectionStatus}", sender.Name, sender.ConnectionStatus);
        }

        public BluetoothConnectionStatus Status => device?.ConnectionStatus ?? BluetoothConnectionStatus.Disconnected;

        // Connect to the Bluetooth device with the specified mode
        public async Task<bool> Connect(int Mode, ILogger logger)
        {
            try
            {
                if (device.ConnectionStatus != BluetoothConnectionStatus.Connected)
                {
                    logger?.LogDebug("trying to connect {@device}", device);
                    using var streamSocket = new StreamSocket();
                    var targetServiceId = RfcommServiceId.FromShortId((uint)Mode);

                    RfcommDeviceServicesResult services = await device.GetRfcommServicesForIdAsync(targetServiceId);
                    RfcommDeviceService service = services.Services.FirstOrDefault();

                    logger?.LogDebug("issuing connect command {@device}", device);
                     await streamSocket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName,
                    SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

                    if (device.ConnectionStatus == BluetoothConnectionStatus.Connected)
                    {
                        logger?.LogDebug("device is connected now");
                    }
                    else
                    {
                        logger?.LogDebug("issued connect but didn't connect {@streamSocket}", @streamSocket);                 
                    }

                        return true;
                }
                else
                {
                    logger?.LogDebug("Device Already Connected {@device}",device);
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error connecting to Bluetooth device (usually device is just not seen.");
                return false;
            }

            return false;
        }



        // Get a dictionary of all paired Bluetooth devices
        public static async Task<Dictionary<string, string>> GetAllPairedBluetoothDevices(ILogger logger)
        {
            var selector = BluetoothDevice.GetDeviceSelector(); // All paired devices

            var returnDict = new Dictionary<string, string>();

            var devices = await DeviceInformation.FindAllAsync(selector);

            // Add all the devices to a dictionary
            foreach (var device in devices)
            {
                returnDict.Add(device.Name, device.Id);
            }

            logger?.LogDebug("found the following devices {@returnDict}",returnDict);
            return returnDict;
        }


    }
}
