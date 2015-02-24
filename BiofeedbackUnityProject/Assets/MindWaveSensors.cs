using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// Mind wave sensors. Read from the neurosky device using the ThinkGear and ThinkGearController plugins.
/// </summary>
public class MindWaveSensors : MonoBehaviour {
	// Use this for Mindwave initialization
	private int handleID = -1;
	private int baudRate = ThinkGear.BAUD_9600;
	private int packetType = ThinkGear.STREAM_PACKETS;
	public string Portname = "COM5";
	// MindWave readings
	public float poorSignal = 0;
	public float attention = 0;
	public float meditation = 0;
	public int connectStatus = 0;

	// Use this for initialization
	void Start () {
		// NeuroSky MindWave
		handleID = ThinkGear.TG_GetNewConnectionId();
		connectStatus = ThinkGear.TG_Connect(handleID, Portname, baudRate, packetType);
		// or that the headset is not turned on
		if(connectStatus >= 0){ 
			//yield return new WaitFor Seconds(1.5f);
//			Thread.Sleep (1500); // doubt this is necessary --strank
			int packetCount = ThinkGear.TG_ReadPackets(handleID, -1);
			if(packetCount > 0){
				TriggerEvent("OnHeadsetConnected", null);
				
				// now set up a repeating invocation to update the headset data
				InvokeRepeating("UpdateHeadsetData", 0.0f, 1.0f);
			} else {
				// if we didn't find anything, then the connection attempt
				// failed. notify the rest of the GOs in the game
				TriggerEvent("OnHeadsetConnectionError", null);
				
				// free the handle
				ThinkGear.TG_FreeConnection(handleID);
			}
		}
		else {
			TriggerEvent("OnHeadsetConnectionError", null);			
			ThinkGear.TG_FreeConnection(handleID);
		}

	}
	
	// Update is called once per frame
	void Update () 
	{
		UpdateHeartRateText();
		UpdateHeadsetData ();
	}
	
	void UpdateHeartRateText() {
		
		string SigStrenString = poorSignal.ToString();
		FileWriter.TxtSaveByStr("MindStrength", SigStrenString);
		
		string AttenString = attention.ToString();
		FileWriter.TxtSaveByStr("Atten", AttenString);
		
		string MedString = meditation.ToString();
		FileWriter.TxtSaveByStr("Med", MedString);
		
	}
	
	void OnGUI()
	{
		//Mindwave
//		GUILayout.Label("Mindwave sensor",largeFont);
//		GUILayout.Label("Signal strength: " + poorSignal,smallFont);
//		GUILayout.Label("attention: " + attention,smallFont);
//		GUILayout.Label("meditation: " + meditation,smallFont);
		//		GUILayout.Label("Delta:" + delta);
	}

	// Mindwave function
	void OnHeadsetDisconnectionRequest(){
		ThinkGear.TG_FreeConnection(handleID);
		CancelInvoke("UpdateHeadsetData");
		
		TriggerEvent("OnHeadsetDisconnected", null);
	}
	
	private void UpdateHeadsetData(){
		int packetCount = ThinkGear.TG_ReadPackets(handleID, -1);
		
		Hashtable values = new Hashtable();
		
		if(packetCount > 0){
			values.Add("poorSignal", GetDataValue(ThinkGear.DATA_POOR_SIGNAL));  
			values.Add("attention", GetDataValue(ThinkGear.DATA_ATTENTION));
			values.Add("meditation", GetDataValue(ThinkGear.DATA_MEDITATION));
			values.Add("delta", GetDataValue(ThinkGear.DATA_DELTA));
			values.Add("theta", GetDataValue(ThinkGear.DATA_THETA));
			values.Add("lowAlpha", GetDataValue(ThinkGear.DATA_ALPHA1));
			values.Add("highAlpha", GetDataValue(ThinkGear.DATA_ALPHA2));
			values.Add("lowBeta", GetDataValue(ThinkGear.DATA_BETA1));
			values.Add("highBeta", GetDataValue(ThinkGear.DATA_BETA2));
			values.Add("lowGamma", GetDataValue(ThinkGear.DATA_GAMMA1));
			values.Add("highGamma", GetDataValue(ThinkGear.DATA_GAMMA2));
			float sumValue = GetDataValue(ThinkGear.DATA_DELTA) + GetDataValue(ThinkGear.DATA_THETA) + GetDataValue(ThinkGear.DATA_ALPHA1) + GetDataValue(ThinkGear.DATA_ALPHA2);
			attention =  GetDataValue(ThinkGear.DATA_ATTENTION);
			meditation = GetDataValue(ThinkGear.DATA_MEDITATION);	
			poorSignal = 200 - GetDataValue(ThinkGear.DATA_POOR_SIGNAL);			
			TriggerEvent("OnHeadsetDataReceived", values);
		}
		
	}
	
	// Convenience method to trigger an event on all GOs
	private void TriggerEvent(string eventName, System.Object parameter){
		foreach(GameObject go in FindObjectsOfType(typeof(GameObject)))
			go.SendMessage(eventName, parameter, 
			               SendMessageOptions.DontRequireReceiver); 
	}
	
	// Convenience method to retrieve a value from the headset
	private float GetDataValue(int valueType){
		return ThinkGear.TG_GetValue(handleID, valueType);
	}

	void OnApplicationQuit()
	{
		Debug.Log("MindWave Device Exiting");
		OnHeadsetDisconnectionRequest(); //todo: check if still necessary?
	}
}
