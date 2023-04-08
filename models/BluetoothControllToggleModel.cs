using System.Collections.Generic;

namespace BluetoothController.Models
{
  public class BluetoothMonitorActionModel
  {
        public List<string> DeviceNames { get; set; }
        public List<KeyValuePair<string, int>> Modes { get; set; }
        public int SelectedMode { get; set; }
        public string SelectedDeviceName { get; set; }
        public string SelectedIcon { get; set; }


    }
}
