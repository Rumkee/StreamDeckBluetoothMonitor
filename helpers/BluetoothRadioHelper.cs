using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Radios;
using Microsoft.Extensions.Logging;

namespace BluetoothController.Helpers
{
    public class BluetoothRadioHelper
    {
      
        private Radio btRadio;
        private readonly ILogger _logger;

      

     
        public delegate Task RadioStatusChanged(Radio sender, object args);

     
        public event RadioStatusChanged RadioStatusChangedEvent;

        public BluetoothRadioHelper(ILogger logger)
        {
            _logger = logger;
        }

      
        private void Radio_StateChanged(Radio sender, object args)
        {
            _logger?.LogInformation("Bluetooth adapter state changed to {State}", sender.State);
            // Trigger a callback
            RadioStatusChangedEvent?.Invoke(sender, args);
        }

        // Check if Bluetooth is on and turn it on if it's off
        public async Task<bool> CheckBluetoothOn()
        {
            _logger?.LogDebug("CheckBluetoothOn {@btRadio}", btRadio);
            BluetoothAdapter adapter = await BluetoothAdapter.GetDefaultAsync();

            if (adapter is not null && btRadio is null)
            {
                btRadio = await adapter.GetRadioAsync();
                btRadio.StateChanged += Radio_StateChanged;
            }

            if (btRadio.State == RadioState.Off)
            {
             //   await btRadio.SetStateAsync(RadioState.On);

                _logger?.LogInformation("Bluetooth radio is off {@btRadio}",btRadio);
                return false;
            }
            

            return true;
        }
    }
}
