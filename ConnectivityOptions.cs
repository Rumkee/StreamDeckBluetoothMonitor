using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothController
{

    public class BluetoothProfileData
    {

        public static List<KeyValuePair<string,int>> GetListFromValues(List<int> values)
        {
            //find from Data where maches in values, including None option
            return Data.Where(kvp => values.Contains(kvp.Value) || kvp.Value==0).ToList();

        }

        internal static List<KeyValuePair<string, int>> GetBlank()
        {
           return new List<KeyValuePair<string, int>>() { Data.First(kvp => kvp.Value == 0) }; 
        }

        private static List<KeyValuePair<string, int>> Data { get; } = new List<KeyValuePair<string, int>>
    {
        new KeyValuePair<string, int>("None", 0),
        new KeyValuePair<string, int>("AdvancedAudioDistribution", 4317),
        new KeyValuePair<string, int>("AudioSink", 4363),
        new KeyValuePair<string, int>("AudioSource", 4266),
        new KeyValuePair<string, int>("AudioVideo", 4396),
        new KeyValuePair<string, int>("AVRemoteControl", 4366),
        new KeyValuePair<string, int>("AVRemoteControlController", 4367),
        new KeyValuePair<string, int>("AVRemoteControlTarget", 4398),
        new KeyValuePair<string, int>("BasicPrinting", 4386),
        new KeyValuePair<string, int>("BrowseGroupDescriptor", 4097),
        new KeyValuePair<string, int>("CommonIsdnAccess", 4392),
        new KeyValuePair<string, int>("CordlessTelephony", 4361),
        new KeyValuePair<string, int>("DialupNetworking", 4355),
        new KeyValuePair<string, int>("DirectPrinting", 4472),
        new KeyValuePair<string, int>("DirectPrintingReferenceObjects", 4392),
        new KeyValuePair<string, int>("Fax", 4369),
        new KeyValuePair<string, int>("GenericAudio", 4611),
        new KeyValuePair<string, int>("GenericFileTransfer", 4610),
        new KeyValuePair<string, int>("GenericNetworking", 4609),
        new KeyValuePair<string, int>("GenericTelephony", 4612),
        new KeyValuePair<string, int>("GN", 4471),
        new KeyValuePair<string, int>("Handsfree", 4382),
        new KeyValuePair<string, int>("HandsfreeAudioGateway", 4383),
        new KeyValuePair<string, int>("HardcopyCableReplacement", 4397),
        new KeyValuePair<string, int>("HardcopyCableReplacementPrint", 4398),
        new KeyValuePair<string, int>("HardcopyCableReplacementScan", 4399),
        new KeyValuePair<string, int>("Headset", 4360),
        new KeyValuePair<string, int>("HeadsetAudioGateway", 4370),
        new KeyValuePair<string, int>("HeadsetHeadset", 4401),
        new KeyValuePair<string, int>("HealthDevice", 5120),
        new KeyValuePair<string, int>("HealthDeviceSink", 5122),
        new KeyValuePair<string, int>("HealthDeviceSource", 5121),
        new KeyValuePair<string, int>("HumanInterfaceDevice", 4392),
        new KeyValuePair<string, int>("Imaging", 4506),
        new KeyValuePair<string, int>("ImagingAutomaticArchive", 4508),
        new KeyValuePair<string, int>("ImagingReferenceObjects", 4509),
        new KeyValuePair<string, int>("ImagingResponder", 4507),
        new KeyValuePair<string, int>("Intercom", 4368),
        new KeyValuePair<string, int>("IrMCSync", 4356),
        new KeyValuePair<string, int>("IrMCSyncCommand", 4359),
        new KeyValuePair<string, int>("LanAccessUsingPpp", 4354),
        new KeyValuePair<string, int>("MessageAccessProfile", 4404),
        new KeyValuePair<string, int>("MessageAccessServer", 4402),
        new KeyValuePair<string, int>("MessageNotificationServer", 4403),
        new KeyValuePair<string, int>("Nap", 4470),
        new KeyValuePair<string, int>("ObexFileTransfer", 4358),
        new KeyValuePair<string, int>("ObexObjectPush", 4357),
        new KeyValuePair<string, int>("Panu", 4469),
        new KeyValuePair<string, int>("PhonebookAccess", 4400),
        new KeyValuePair<string, int>("PhonebookAccessPce", 4398),
        new KeyValuePair<string, int>("PhonebookAccessPse", 4399),
        new KeyValuePair<string, int>("PnPInformation", 4608),
        new KeyValuePair<string, int>("PrintingStatus", 4387),
        new KeyValuePair<string, int>("PublicBrowseGroup", 4098),
        new KeyValuePair<string, int>("ReferencePrinting", 4505),
        new KeyValuePair<string, int>("ReflectedUI", 4385),
        new KeyValuePair<string, int>("SerialPort", 4353),
        new KeyValuePair<string, int>("ServiceDiscoveryServer", 4096),
        new KeyValuePair<string, int>("SimAccess", 4397),
        new KeyValuePair<string, int>("UdiMT", 4394),
        new KeyValuePair<string, int>("UdiTA", 4395),
        new KeyValuePair<string, int>("UPnp", 4613),
        new KeyValuePair<string, int>("UPnpIP", 4614),
        new KeyValuePair<string, int>("UPnpIPL2Cap", 4866),
        new KeyValuePair<string, int>("UPnpIPLap", 4865),
        new KeyValuePair<string, int>("UPnpIPPan", 4864),
        new KeyValuePair<string, int>("VideoConferencingGW", 4393),
        new KeyValuePair<string, int>("VideoDistribution", 4869),
        new KeyValuePair<string, int>("VideoSink", 4868),
        new KeyValuePair<string, int>("VideoSource", 4867),
        new KeyValuePair<string, int>("Wap", 4371),
        new KeyValuePair<string, int>("WapClient", 4372)
        };
    }


}
