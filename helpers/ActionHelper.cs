using BluetoothController.Helpers;
using BluetoothController.Models;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using StreamDeckLib;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace BluetoothController.Helpers
{
    public class ActionHelper

    {
        private Dictionary<string, string> bluetoothDevices;
        //constructor
        public ActionHelper()
        {
            bluetoothDevices = new Dictionary<string, string>();
        }
        public async Task LoadAllDevices(BluetoothMonitorActionModel settings, ILogger logger)
        {
            logger?.LogDebug("LoadAllDevices");
            var devices = await BluetoothDeviceHelper.GetAllPairedBluetoothDevices(logger);
            bluetoothDevices = devices;

            settings.DeviceNames = new List<string>();

            foreach (string key in bluetoothDevices.Keys)
            {
                settings.DeviceNames.Add(key);
            }

        }

        public string Context { get; set; }

        public string GetDeviceIdFromName(string deviceName, ILogger logger)
        {

            bluetoothDevices.TryGetValue(deviceName, out string deviceValue);
            logger?.LogDebug("GetDeviceIdFromName {deviceName} {deviceValue}", deviceName, deviceValue);

            if (deviceValue == null)
            {
                logger?.LogDebug("no matching value for key {deviceName} {deviceValue}", deviceName, deviceValue);


                //check appsettings.json for setting

               

                //var fuzzyMatches = bluetoothDevices.Keys.Where(k => k.Contains(deviceName));
                //if (fuzzyMatches.Count() == 1)
                //{
                //    var matchedKey = fuzzyMatches.First();
                //    logger?.LogDebug("found single fuzzy match {deviceName} {matchedKey}", deviceName, matchedKey);

                //    bluetoothDevices.TryGetValue(matchedKey, out string fuzzyMatchedDeviceValue);
                //    return fuzzyMatchedDeviceValue;

                //}
                //else
                //{
                //    logger?.LogDebug("fuzzy match did not find a single match for {deviceName} {@fuzzyMatches}", deviceName, fuzzyMatches);
                //    return null;
                //}
            }

          
            return deviceValue;
        }

        private static int GetActionStateIndex(string selectedIcon, string stateOfIcon)
        {
            _ = int.TryParse(selectedIcon, out int intSelectedIcon);
            _ = int.TryParse(stateOfIcon, out int intStateOfIcon);

            return intSelectedIcon + intStateOfIcon;
        }

        private static async Task SetStateWithIcon(string context, Constants.IconStateOfset state, BluetoothMonitorActionModel settings, ConnectionManager manager,ILogger logger)
        {
            string icon = settings.SelectedIcon is null || settings.SelectedIcon == Constants.IconOfset.Generic.ToString()
                ? Constants.IconOfset.Generic.ToString()
                : settings.SelectedIcon;

            logger?.LogInformation("SetStateWithIcon {icon}",icon);
            await manager.SetStateAsync(context, GetActionStateIndex(icon, ((int)state).ToString()));
        }

        public static async Task SetGreyIcon(string context, BluetoothMonitorActionModel settings, ConnectionManager manager, ILogger logger)
        {
            logger?.LogDebug("SetGreyIcon");
            await SetStateWithIcon(context, Constants.IconStateOfset.Grey, settings, manager,logger);
        }

        public static async Task SetConnectedIcon(string context, BluetoothMonitorActionModel settings, ConnectionManager manager, ILogger logger)
        {
            logger?.LogDebug("SetConnectedIcon");
            await SetStateWithIcon(context, Constants.IconStateOfset.Green, settings, manager, logger);
        }

        public static async Task SetDisconnectedIcon(string context, BluetoothMonitorActionModel settings, ConnectionManager manager, ILogger logger)
        {
            logger?.LogDebug("SetDisconnectedIcon");
            await SetStateWithIcon(context, Constants.IconStateOfset.Red, settings, manager, logger);
        }




    }
}
