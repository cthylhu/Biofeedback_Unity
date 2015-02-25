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
	private const string otherDataLabelsString =
		"delta theta lowAlpha highAlpha lowBeta highBeta lowGamma highGamma";
	[Header("Other data in order: " + otherDataLabelsString)]
	public Text otherDataText;
	private static readonly string[] otherDataLabels = otherDataLabelsString.Split();

	// Use this for Mindwave connection
	private ThinkGearController tgController;
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
	}
	
	// Called once when enabled, using this for hardware initialization
	void Start ()
	{
		Debug.Log (string.Format ("MindWave device Initializing (driver version {0})", ThinkGear.TG_GetDriverVersion ()));
		tgController = gameObject.AddComponent<ThinkGearController>();
		int result = tgController.setup (Portname);
		statusText.text = string.Format("Initialized (result {0})", result);
		// set up listeners for events:
		tgController.OnHeadsetConnected += this.HandleOnHeadsetConnected;
		tgController.OnHeadsetDataReceived += this.HandleOnHeadsetDataReceived;
		tgController.OnHeadsetConnectionError += this.HandleOnHeadsetConnectionError;
		tgController.startReadingData();
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

	public void HandleOnHeadsetDataReceived(IDictionary<string, float> data)
	{
		UpdateMindWaveDataUIText(data);
		WriteMindWaveDataFile(data);
	}

	void UpdateMindWaveDataUIText (IDictionary<string, float> data)
	{
		poorSignalText.text = data["poorSignal"].ToString ("R");
		attentionText.text = data["attention"].ToString ("R");
		meditationText.text = data["meditation"].ToString ("R");
		if (otherDataText != null) {
			string others = "";
			foreach (string label in otherDataLabels) {
				others += string.Format ("{0}: \t{1}\n", label, data[label].ToString ("R"));
			}
			otherDataText.text = others;
		}
	}
	
	void WriteMindWaveDataFile (IDictionary<string, float> data)
	{
		FileWriter.TxtSaveByStr ("MindWave_poorSignal", data["poorSignal"].ToString ("R"));
		FileWriter.TxtSaveByStr ("MindWave_attention", data["attention"].ToString ("R"));
		FileWriter.TxtSaveByStr ("MindWave_meditation", data["meditation"].ToString ("R"));
		foreach (string dataLabel in otherDataLabels) {
			FileWriter.TxtSaveByStr ("MindWave_" + dataLabel, data[dataLabel].ToString ("R"));
		}
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
