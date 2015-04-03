using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// Mind wave sensors. Read from the neurosky device using the ThinkGear and ThinkGearController plugins.
/// </summary>
public class MindWaveSensors : MonoBehaviour
{
	// the class assumes that the following Text properties are not null!
	public Text statusText;
	public Text poorSignalText;
	public Text attentionText;
	public Text meditationText;
	// Other data is read, but no Texts need to be set for it
	[Header("All data in order: " + ThinkGearController.dataLabelsString)]
	public Text otherDataText;
	// cache of last updated values:
	private IDictionary<string, double> lastReceivedData = new Dictionary<string, double>();

	// Use this for Mindwave connection
	private ThinkGearController tgController;
	// initially tried port can be set in the inspector of the Panel for now
	// common name on Mac OS: /dev/tty.usbmodemXYZ (where XYZ are numbers)
	public string Portname = "COM5";

	// Called once initally, setting up UI components
	void Awake ()
	{
		statusText.text = "Not connected";
		poorSignalText.text = "N/A yet";
		attentionText.text = "N/A yet";
		meditationText.text = "N/A yet";
		if (otherDataText != null) {
			otherDataText.text = "N/A yet";
		}
		foreach (string label in ThinkGearController.dataLabels) {
			lastReceivedData.Add (label, 0.0);
		}
	}
	
	// Called once when enabled, using this for hardware initialization
	void Start ()
	{
		Debug.Log ("MindWave device Initializing");
		tgController = gameObject.AddComponent<ThinkGearController>();
		// set up listeners for events:
		tgController.OnHeadsetConnected += this.HandleOnHeadsetConnected;
		// tgController.OnHeadsetDisconnected ignored for now.
		tgController.OnHeadsetDataReceived += this.HandleOnHeadsetDataReceived;
		tgController.OnHeadsetConnectionError += this.HandleOnHeadsetConnectionError;
		tgController.setup (Portname);
		statusText.text = "Initialized";
	}
	
	public void HandleOnHeadsetConnected(int packetCount)
	{
		statusText.text = string.Format ("Connected and {0} packets read initially", packetCount);
	}

	public void HandleOnHeadsetConnectionError(int packetError)
	{
		statusText.text = string.Format ("Connection Error: {0}", packetError);
		// potential TODO: from here we should set the UI checkbox to unchecked again
	}

	public void HandleOnHeadsetDataReceived(IDictionary<string, double> data)
	{
		double timestamp = data["Time"];
		foreach (string key in data.Keys) {
			if (key == "Time") continue;
			lastReceivedData[key] = data[key];
			WriteMindWaveDataFile(timestamp, key, data[key]);
		}
		UpdateMindWaveDataUIText(lastReceivedData);
	}

	void UpdateMindWaveDataUIText (IDictionary<string, double> data)
	{
		poorSignalText.text = data["PoorSignal"].ToString ("R");
		attentionText.text = data["Attention"].ToString ("R");
		meditationText.text = data["Meditation"].ToString ("R");
		if (otherDataText != null) {
			string others = "";
			foreach (string label in ThinkGearController.dataLabels) {
				others += string.Format ("{0}: \t{1}\n", label, data[label].ToString ("R"));
			}
			otherDataText.text = others;
		}
	}
	
	void WriteMindWaveDataFile (double timestamp, string key, double dataValue)
	{
		// TODO: also write timestamp information to data files!
		FileWriter.TxtSaveByStr ("MindWave_" + key, dataValue.ToString ("R"));
	}

	void OnDestroy ()
	{
		if (tgController != null) {
			Debug.Log ("MindWave Device Exiting");
			// remove listeners for events:
			tgController.OnHeadsetConnected -= this.HandleOnHeadsetConnected;
			tgController.OnHeadsetDataReceived -= this.HandleOnHeadsetDataReceived;
			tgController.OnHeadsetConnectionError -= this.HandleOnHeadsetConnectionError;
			tgController.disconnect ();
		}
	}
}
