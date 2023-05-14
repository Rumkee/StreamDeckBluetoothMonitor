using BluetoothController.Helpers;
using BluetoothController.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using StreamDeckLib;
using StreamDeckLib.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Radios;

namespace BluetoothController
{


    [ActionUuid(Uuid= "me.rumkee.bluetooth.monitor")]
  public class BluetoothMonitorAction : BaseStreamDeckActionWithSettingsModel<BluetoothMonitorActionModel>, IDisposable
    {
     
        private  BluetoothDeviceHelper selectedBluetoothDevice;
        private readonly BluetoothRadioHelper radioHelper;
        private readonly ActionHelper actionHelper;
        

        //constructor
        public BluetoothMonitorAction()
        {
        
            actionHelper = new ActionHelper();
            radioHelper = new BluetoothRadioHelper(Logger);
            radioHelper.RadioStatusChangedEvent += BluetoothThing_RadioStatusChangedEvent;
            CreateSelectedBluetoothDevice();

           

        }

        private void CreateSelectedBluetoothDevice()
        {
            Logger?.LogDebug("CreateSelectedBluetoothDevice");
            selectedBluetoothDevice = new BluetoothDeviceHelper();
            selectedBluetoothDevice.BluetoothConnectionStatusChangedEvent += BluetoothThing_BluetoothConnectionStatusChangedEvent;
           
        }

        private void RemoveSelectedBluetoothDevice()
        {
            Logger?.LogDebug("RemoveSelectedBluetoothDevice");
            selectedBluetoothDevice.BluetoothConnectionStatusChangedEvent -= BluetoothThing_BluetoothConnectionStatusChangedEvent;
         
            selectedBluetoothDevice = null;
        }



        public override async Task OnKeyUp(StreamDeckEventPayload args)
	{

            Logger?.LogDebug("OnKeyUp {@args}{@SettingsModel}", args,SettingsModel);
            actionHelper.Context = args.context;

            if (selectedBluetoothDevice !=null && SettingsModel.SelectedMode != 0 && SettingsModel.BluetoothOn)
            {
            await ActionHelper.SetGreyIcon(args.context,SettingsModel,Manager,Logger);

            await selectedBluetoothDevice.Connect(SettingsModel.SelectedMode,Logger);
            await CheckSetStatusIcon(args.context);
            }
            else
            {
                Logger?.LogWarning("Key Up but no device or mode or bluetooth off{@selectedBluetoothDevice} {@SettingsModel}", selectedBluetoothDevice, SettingsModel);
                await CheckSetStatusIcon(args.context);
                await Manager.ShowAlertAsync(args.context);
            }
          

         
        }

	    public override async Task OnDidReceiveSettings(StreamDeckEventPayload args)
	{
            Logger?.LogDebug("OnDidReceiveSettings {@args}{@SettingsModel}", args,SettingsModel);
            await base.OnDidReceiveSettings(args);

            var newSelectedIcon = args.payload?.settings?.NewSettingsModel?.SelectedIcon?.Value;
            if (!string.IsNullOrEmpty(newSelectedIcon))
            {
                if (SettingsModel.SelectedIcon != newSelectedIcon)
                {
                    SettingsModel.SelectedIcon = newSelectedIcon;
                    await CheckSetStatusIcon(args.context);
                }
              
            }
            else
            {
                SettingsModel.SelectedIcon = Constants.IconOfset.Generic.ToString();
            }

            string newSelectedDeviceName = Convert.ToString(args.payload?.settings?.NewSettingsModel?.SelectedDeviceName?.Value);
            string newSelectedModeStr = Convert.ToString(args.payload?.settings?.NewSettingsModel?.SelectedMode?.Value);
            _ = int.TryParse(newSelectedModeStr, out int newSelectedMode);

            //if we've picked a device, and it's different to what we had previously selected
            if (!string.IsNullOrEmpty(newSelectedDeviceName) && SettingsModel.SelectedDeviceName!= newSelectedDeviceName)
            {
                Logger?.LogInformation("new Selected Dvice different to previous {@newSelectedDeviceName} {@SettingsModel}", newSelectedDeviceName, SettingsModel);
                SettingsModel.SelectedDeviceName = newSelectedDeviceName;
                SettingsModel.Modes = BluetoothProfileData.GetBlank();
                SettingsModel.SelectedMode = 0;
                SettingsModel.SelectedDeviceID= actionHelper.GetDeviceIdFromName(SettingsModel.SelectedDeviceName, Logger);
                

                if (SettingsModel.SelectedDeviceID != null)
                {
                    Logger?.LogDebug("LoadDeviceFromId {@deviceId} {@SettingsModel}", SettingsModel.SelectedDeviceID, SettingsModel);
                    if (selectedBluetoothDevice == null)
                    {
                        CreateSelectedBluetoothDevice();
                    }
                    SettingsModel.BluetoothOn = await radioHelper.CheckBluetoothOn();
                    await selectedBluetoothDevice.LoadDeviceFromId(SettingsModel.SelectedDeviceID, Logger);
                    SettingsModel.Modes = BluetoothProfileData.GetListFromValues(selectedBluetoothDevice.SupportedModes);
                    SettingsModel.SelectedMode = SettingsModel.Modes.Last().Value;
                }
                else
                {
                    Logger?.LogDebug("could not find device {SelectedDeviceName} {@SettingsModel}", SettingsModel.SelectedDeviceName, SettingsModel);
                    await Manager.ShowAlertAsync(args.context);
                }
            }

                if (SettingsModel.SelectedMode != newSelectedMode)
                {
                Logger?.LogInformation("new SelectedMode {@newSelectedMode} {@SettingsModel}", newSelectedMode, SettingsModel);
                SettingsModel.SelectedMode = newSelectedMode;
                    await ActionHelper.SetGreyIcon(args.context, SettingsModel, Manager, Logger);
                    await CheckSetStatusIcon(args.context);
                }


                await PushToPropertyInspector(args.context);
     

        }

        public override async Task OnWillAppear(StreamDeckEventPayload args)
        {
            //   await base.OnWillAppear(args);
            //
            Logger?.LogDebug("OnWillAppear  {@args}{@SettingsModel}", args, SettingsModel);



            if (args.payload.settings?.settingsModel != null && ((JContainer)args.payload.settings.settingsModel).Count > 0)
            {

                //copy settings to SettingsModel
                SettingsModel.SelectedDeviceName = args.payload.settings.settingsModel.SelectedDeviceName;
                SettingsModel.SelectedDeviceID = args.payload.settings.settingsModel.SelectedDeviceID;
                SettingsModel.SelectedIcon = args.payload.settings.settingsModel.SelectedIcon;
                Logger?.LogDebug("SelectedDeviceName and SelectedIcon set {@SettingsModel}", SettingsModel);

                string newSelectedModeStr = Convert.ToString(args.payload?.settings?.settingsModel?.SelectedMode?.Value);
                _ = int.TryParse(newSelectedModeStr, out int newSelectedMode);
                SettingsModel.SelectedMode = newSelectedMode;// args.payload.settings.settingsModel.SelectedMode;

                JArray modesArray = args.payload.settings.settingsModel.Modes;

                if (SettingsModel.Modes == null && modesArray != null && modesArray.Any())
                {

                    SettingsModel.Modes = new List<KeyValuePair<string, int>>();
                    foreach (var mode in modesArray)
                    {
                        int value = (int)mode["Value"];
                        string key = (string)mode["Key"];
                        SettingsModel.Modes.Add(new KeyValuePair<string, int>(key, value));
                    }
                    Logger?.LogDebug("Modles updated {@SettingsModel}", SettingsModel);
                }

            }
            else
            {
                SettingsModel.SelectedIcon = ((int)Constants.IconOfset.Generic).ToString();
            }

            actionHelper.Context = args.context;

            //load devices and send to UI
            SettingsModel.BluetoothOn = await radioHelper.CheckBluetoothOn();
            await actionHelper.LoadAllDevices(SettingsModel, Logger);
            await PushToPropertyInspector(args.context);
            await LoadDeviceFromSelectedDeviceID(args.context);

        }

        private async Task LoadDeviceFromSelectedDeviceID(string context)
        {
            if (SettingsModel.SelectedDeviceID != null)
            {
                Logger?.LogDebug("LoadDeviceFromId {@SettingsModel}", SettingsModel);
             //   string deviceId = actionHelper.GetDeviceIdFromName(SettingsModel.SelectedDeviceName, Logger);

                if (selectedBluetoothDevice == null)
                {
                    CreateSelectedBluetoothDevice();

                }
               
                    await selectedBluetoothDevice.LoadDeviceFromId(SettingsModel.SelectedDeviceID, Logger);
              

                await CheckSetStatusIcon(context);
            }
            else
            {
                Logger?.LogDebug("No SelectedDeviceID found {@SettingsModel}", SettingsModel);
                await Manager.ShowAlertAsync(context);
            }
        }

        public override async Task OnPropertyInspectorDidAppear(StreamDeckEventPayload args)
        {
            Logger?.LogDebug("OnPropertyInspectorDidAppear {@args}", args);
            await Manager.SetSettingsAsync(args.context, SettingsModel);
        }

        public override async Task OnSendToPlugin(StreamDeckEventPayload args)
        {
            Logger?.LogDebug("OnSendToPlugin {@args}", args);

            await actionHelper.LoadAllDevices(SettingsModel, Logger);
            await PushToPropertyInspector(args.context);

        }


        private async Task BluetoothThing_BluetoothConnectionStatusChangedEvent(BluetoothDevice sender, object args)
        {
            Logger?.LogDebug("BluetoothThing_BluetoothConnectionStatusChangedEvent {@Sender}{@args}", sender,args);
            await CheckSetStatusIcon(actionHelper.Context);

        }

        private async Task BluetoothThing_RadioStatusChangedEvent(Radio sender, object args)
        {
            Logger?.LogDebug("BluetoothThing_RadioStatusChangedEvent {@Sender}{@args}", sender, args);
            if (sender.State != RadioState.On)
            {
                SettingsModel.BluetoothOn = false;
              
            }
            else
            {
                SettingsModel.BluetoothOn = true;
                await LoadDeviceFromSelectedDeviceID(actionHelper.Context);
            }
            await PushToPropertyInspector(actionHelper.Context);
            await CheckSetStatusIcon(actionHelper.Context);
        }

        private async Task CheckSetStatusIcon(string context)
        {

            Logger?.LogDebug("CheckSetStatusIcon {@context}{@SettingsModel}", context, SettingsModel);
            if (selectedBluetoothDevice == null || !SettingsModel.BluetoothOn)
            {               
               await ActionHelper.SetGreyIcon(context, SettingsModel, Manager, Logger);
                return;
            }
               

            if (selectedBluetoothDevice.Status==BluetoothConnectionStatus.Connected)
            {
                await ActionHelper.SetConnectedIcon(context, SettingsModel, Manager, Logger);
            }
            else
            {
               await ActionHelper.SetDisconnectedIcon(context, SettingsModel, Manager, Logger);
            }
        }

        private async Task PushToPropertyInspector(string context)
        {
            Logger?.LogDebug("PushToPropertyInspector {@SettingsModel} {@context}", SettingsModel,context);
            await Manager.SetSettingsAsync(context, SettingsModel);
        }

        public void Dispose()
        {
            if (selectedBluetoothDevice != null)
            {
                RemoveSelectedBluetoothDevice();
            }
         
        }
    }
}
