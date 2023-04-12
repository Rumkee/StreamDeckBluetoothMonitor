// global websocket, used to communicate from/to Stream Deck software
// as well as some info about our plugin, as sent by Stream Deck software 
var websocket = null,
  uuid = null,
  inInfo = null,
	actionInfo = {},

  settingsModel = {
	  SelectedDeviceName: null,
	  SelectedIcon: "0",
	  SelectedMode: null,
	  Modes: null
  };

function connectElgatoStreamDeckSocket(inPort, inUUID, inRegisterEvent, inInfo, inActionInfo) {
  uuid = inUUID;
  actionInfo = JSON.parse(inActionInfo);
  inInfo = JSON.parse(inInfo);
  websocket = new WebSocket('ws://localhost:' + inPort);

  //initialize values
  //if (actionInfo.payload.settings.settingsModel) {
	 // settingsModel.SelectedIcon = 0;//default
  //}



//	$("mydevices").val(actionInfo.payload.settings.settingsModel.sel);


	console.log("connectElgatoStreamDeckSocket");
	console.log(settingsModel);
		
  websocket.onopen = function () {
	var json = { event: inRegisterEvent, uuid: inUUID };
	// register property inspector to Stream Deck
	websocket.send(JSON.stringify(json));

  };

  websocket.onmessage = function (evt) {
	// Received message from Stream Deck
	  console.log("websocket.onmessage");
	  console.log(evt);

	var jsonObj = JSON.parse(evt.data);
	var sdEvent = jsonObj['event'];
	switch (sdEvent) {
	  case "didReceiveSettings":
			if (jsonObj.payload.settings.settingsModel) {
				loadDevicesListFromJSON(jsonObj.payload.settings.settingsModel)
				loadModesListFromJSON(jsonObj.payload.settings.settingsModel)

				$("#myicons").val(jsonObj.payload.settings.settingsModel.SelectedIcon);
		
		}
		break;
	  default:
		break;
	}
  };
}

const loadDevicesListFromJSON = (data) => {

	let new_ul = [];

	//only add selected to default if nothing else has selected
	if (data.SelectedDeviceName) {
		new_ul.push("<option disabled value> ----- select a device ----- </option>");
	} else {
		new_ul.push("<option disabled selected value> ----- select a device ----- </option>");
	}

	data.DeviceNames.forEach(e => {
		if (e == data.SelectedDeviceName) {
			new_ul.push("<option value='" + e + "' selected>" + e + "</option>")
		} else {
			new_ul.push("<option value='" + e + "'>" + e + "</option>")
		}
	
	});


		// join all the li's 
	$('#mydevices').html(new_ul.join(""))

}


const loadModesListFromJSON = (data) => {

	let new_ul = [];

	//only add selected to default if nothing else has selected
	//if (data.SelectedMode) {
	//	new_ul.push("<option disabled value> ----- select a mode ----- </option>");
	//} else {
	//	new_ul.push("<option disabled selected value> ----- select a mode ----- </option>");
	//}

	//for (var i = 0; i < data.Modes.length; i++) {
	//	if (data.Modes[i].Value == data.Modes[i].SelectedMode) {
	//		new_ul.push("<option value='" + data.Modes[i].Value + "' selected>" + data.Modes[i].Key + "</option>")
	//	} else {
	//		new_ul.push("<option value='" + data.Modes[i].Value + "'>" + data.Modes[i].Key + "</option>")
	//	}
	//}

	data.Modes.forEach(e => {
		if (e.Value == data.SelectedMode) {
			new_ul.push("<option value='" + e.Value + "' selected>" + e.Key + "</option>")
		} else {
			new_ul.push("<option value='" + e.Value + "'>" + e.Key + "</option>")
		}

	});


	// join all the li's 
	$('#mymodes').html(new_ul.join(""))

}

const clickRefresh = () => {
	if (websocket) {

	//	settingsModel.RefreshClicked = true;
		//var payload = {};
		//payload.property_inspector = 'refreshClicked'

		var json = {
			"action":actionInfo["action"],
			"event": "sendToPlugin",
			"context": uuid,
			"payload": {
			//	"settingsModel": settingsModel
			"a":"b"
			}
		};
		websocket.send(JSON.stringify(json));
	//	settingsModel.RefreshClicked = false;
	}
}

//settingsModel

//const setSettings = (value, param) => {
const setSettings = () => {
if (websocket) {
	
	var json = {
	  "event": "setSettings",
	  "context": uuid,
	  "payload": {
		"NewSettingsModel": settingsModel
	  }
	};
	websocket.send(JSON.stringify(json));
  }
};

