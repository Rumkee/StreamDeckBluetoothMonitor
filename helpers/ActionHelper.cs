using BluetoothController.Helpers;
using BluetoothController.Models;
using Microsoft.Extensions.Logging;
using StreamDeckLib;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace BluetoothController.Helpers
{
    public class ActionHelper

    {
        private readonly ILogger _logger;
        private Dictionary<string, string> bluetoothDevices;
        //constructor
        public ActionHelper(ILogger logger)
        {
            _logger = logger;
            bluetoothDevices = new Dictionary<string, string>();
        }
        public async Task LoadAllDevices(BluetoothMonitorActionModel settings)
        {
            _logger?.LogInformation("LoadAllDevices");
            var devices = await BluetoothDeviceHelper.GetAllPairedBluetoothDevices();
            bluetoothDevices = devices;

            settings.DeviceNames = new List<string>();

            foreach (string key in bluetoothDevices.Keys)
            {
                settings.DeviceNames.Add(key);
            }

        }

        public string Context { get; set; }
        public string GetDeviceIdFromName(string deviceName)
        {

            bluetoothDevices.TryGetValue(deviceName, out string deviceValue);
            _logger?.LogInformation("GetDeviceIdFromName {deviceName} {deviceValue}", deviceName, deviceValue);
            return deviceValue;
        }

        private static int GetActionStateIndex(string selectedIcon, string stateOfIcon)
        {
            _ = int.TryParse(selectedIcon, out int intSelectedIcon);
            _ = int.TryParse(stateOfIcon, out int intStateOfIcon);

            return intSelectedIcon + intStateOfIcon;
        }

        private static async Task SetStateWithIcon(string context, Constants.IconStateOfset state, BluetoothMonitorActionModel settings, ConnectionManager manager)
        {
            string icon = settings.SelectedIcon is null || settings.SelectedIcon == Constants.IconOfset.Generic.ToString()
                ? Constants.IconOfset.Generic.ToString()
                : settings.SelectedIcon;

            await manager.SetStateAsync(context, GetActionStateIndex(icon, ((int)state).ToString()));
        }

        public static async Task SetGreyIcon(string context, BluetoothMonitorActionModel settings, ConnectionManager manager)
        {
            await SetStateWithIcon(context, Constants.IconStateOfset.Grey, settings, manager);
        }

        public static async Task SetConnectedIcon(string context, BluetoothMonitorActionModel settings, ConnectionManager manager)
        {
            await SetStateWithIcon(context, Constants.IconStateOfset.Green, settings, manager);
        }

        public static async Task SetDisconnectedIcon(string context, BluetoothMonitorActionModel settings, ConnectionManager manager)
        {
            await SetStateWithIcon(context, Constants.IconStateOfset.Red, settings, manager);
        }




    }
}
