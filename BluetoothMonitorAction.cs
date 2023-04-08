using BluetoothController.Helpers;
using BluetoothController.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using StreamDeckLib;
using StreamDeckLib.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;


namespace BluetoothController
{


    [ActionUuid(Uuid= "me.rumkee.bluetooth.monitor")]
  public class BluetoothMonitorAction : BaseStreamDeckActionWithSettingsModel<BluetoothMonitorActionModel>
  {
     
        private readonly BluetoothDeviceHelper selectedBluetoothDevice;
        private readonly ActionHelper actionHelper;

        //constructor
        public BluetoothMonitorAction()
        {
            actionHelper = new ActionHelper(base.Logger);
            selectedBluetoothDevice = new BluetoothDeviceHelper(base.Logger);
            selectedBluetoothDevice.BluetoothConnectionStatusChangedEvent += BluetoothThing_BluetoothConnectionStatusChangedEvent;
        }

      
        public override async Task OnKeyUp(StreamDeckEventPayload args)
	{
            
            Logger?.LogTrace("OnKeyUp, context={args}", args.context);
            actionHelper.Context = args.context;

            if (selectedBluetoothDevice !=null && SettingsModel.SelectedMode != 0)
            {
            await ActionHelper.SetGreyIcon(args.context,SettingsModel,Manager);

            await selectedBluetoothDevice.Connect(SettingsModel.SelectedMode);
            await CheckSetStatusIcon(args.context);
            }
            else
            {
                await CheckSetStatusIcon(args.context);
                await Manager.ShowAlertAsync(args.context);
            }
          

         
        }

	    public override async Task OnDidReceiveSettings(StreamDeckEventPayload args)
	{
            Logger?.LogTrace("OnDidReceiveSettings, context={args}", args.context);
            await base.OnDidReceiveSettings(args);

            var newSelectedIcon = args.payload?.settings?.settings?.SelectedIcon?.Value;
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

            string newSelectedDeviceName = Convert.ToString(args.payload?.settings?.settings?.SelectedDeviceName?.Value);
            string newSelectedModeStr = Convert.ToString(args.payload?.settings?.settings?.SelectedMode?.Value);
            _ = int.TryParse(newSelectedModeStr, out int newSelectedMode);

            //if we've picked a device, and it's different to what we had previously selected
            if (!string.IsNullOrEmpty(newSelectedDeviceName) && SettingsModel.SelectedDeviceName!= newSelectedDeviceName)
            {
                SettingsModel.SelectedDeviceName = newSelectedDeviceName;
                SettingsModel.Modes = BluetoothProfileData.GetBlank();
                SettingsModel.SelectedMode = 0;

                string deviceId = actionHelper.GetDeviceIdFromName(SettingsModel.SelectedDeviceName);

                if (deviceId != null)
                {                 
                    await selectedBluetoothDevice.LoadDeviceFromId(deviceId);
                    SettingsModel.Modes = BluetoothProfileData.GetListFromValues(selectedBluetoothDevice.SupportedModes);
                    SettingsModel.SelectedMode = SettingsModel.Modes.Last().Value;
                }
            }

                if (SettingsModel.SelectedMode != newSelectedMode)
                {
                    SettingsModel.SelectedMode = newSelectedMode;
                    await ActionHelper.SetGreyIcon(args.context, SettingsModel, Manager);
                    await CheckSetStatusIcon(args.context);
                }


                await PushToPropertyInspector(args.context);
     

        }

        public override async Task OnWillAppear(StreamDeckEventPayload args)
        {
            //   await base.OnWillAppear(args);
            //
            Logger?.LogTrace("OnWillAppear, context={args}", args.context);



            if (args.payload.settings?.settingsModel != null && ((JContainer)args.payload.settings.settingsModel).Count > 0)
            {
                //copy settings to SettingsModel
                SettingsModel.SelectedDeviceName = args.payload.settings.settingsModel.SelectedDeviceName;
                SettingsModel.SelectedIcon = args.payload.settings.settingsModel.SelectedIcon;
                
                string newSelectedModeStr = Convert.ToString(args.payload?.settings?.settings?.SelectedMode?.Value);
                _ = int.TryParse(newSelectedModeStr, out int newSelectedMode);
                SettingsModel.SelectedMode = newSelectedMode;// args.payload.settings.settingsModel.SelectedMode;

                JArray modesArray = args.payload.settings.settingsModel.Modes;

                if (SettingsModel.Modes == null && modesArray.Any())
                {
                    SettingsModel.Modes = new List<KeyValuePair<string, int>>();
                    foreach (var mode in modesArray)
                    {
                        int value = (int)mode["Value"];
                        string key = (string)mode["Key"];
                        SettingsModel.Modes.Add(new KeyValuePair<string, int>(key, value));
                    }
                }

            }
            else
            {
                SettingsModel.SelectedIcon = ((int)Constants.IconOfset.Generic).ToString();
            }

            actionHelper.Context = args.context;

            //load devices and send to UI
            await actionHelper.LoadAllDevices(SettingsModel);
            await PushToPropertyInspector(args.context);


          

            if (SettingsModel.SelectedDeviceName != null)
            {
               
                string deviceId = actionHelper.GetDeviceIdFromName(SettingsModel.SelectedDeviceName);

                await selectedBluetoothDevice.LoadDeviceFromId(deviceId);
                await CheckSetStatusIcon(args.context);
            }
           
        }

        public override async Task OnPropertyInspectorDidAppear(StreamDeckEventPayload args)
        {
         
              await Manager.SetSettingsAsync(args.context, SettingsModel);
        }

        public override async Task OnSendToPlugin(StreamDeckEventPayload args)
        {
            Logger?.LogTrace("OnSendToPlugIn, context={context}, SettingsModel={SettingsModel}", args.context,SettingsModel);
           
            await actionHelper.LoadAllDevices(SettingsModel);
            await PushToPropertyInspector(args.context);

        }


        private async Task BluetoothThing_BluetoothConnectionStatusChangedEvent(BluetoothDevice sender, object args)
        {
            await CheckSetStatusIcon(actionHelper.Context);

        }

        private async Task CheckSetStatusIcon(string context)
        {
   
            if (selectedBluetoothDevice == null)
            {               
               await ActionHelper.SetGreyIcon(context, SettingsModel, Manager);
                return;
            }
               

            if (selectedBluetoothDevice.Status==BluetoothConnectionStatus.Connected)
            {
                await ActionHelper.SetConnectedIcon(context, SettingsModel, Manager);
            }
            else
            {
               await ActionHelper.SetDisconnectedIcon(context, SettingsModel, Manager);
            }
        }

        private async Task PushToPropertyInspector(string context)
        {
            await Manager.SetSettingsAsync(context, SettingsModel);
        }

    


    }
}
