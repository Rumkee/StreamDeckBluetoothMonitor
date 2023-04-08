using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothController
{
   
        internal static class Constants
        {
            public enum ActionState { Disconnected = 0, Connected = 1, Unknown = 2, Sandtimer = 3 };
            public enum IconStateOfset { Red = 0, Green = 1, Grey = 2, };
            public enum IconOfset { Generic = 0, Airpods = 3, Keyboard = 6, Speaker = 9, Controller = 12, Fitness = 15, Mouse = 18, Printer = 21 };

            public static string ConnectivityTypePatternMatch = @"(?<=\{[0-9a-fA-F]{4})[0-9a-fA-F]{4}(?=-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\})";

    }
    
}
