using UnityEngine;
using System.Collections;
using System.Threading;
//using ThinkGearController;
public class Mindwave : MonoBehaviour {

	// Use this for initialization
	private int handleID = -1;
	private int baudRate = ThinkGear.BAUD_9600;
	private int packetType = ThinkGear.STREAM_PACKETS;
	public string Portname;
	public float poorSignal = 0;
	public float attention = 0;
	public float meditation = 0;
	public int connectStatus = 0;
	public float Value = 0;
//	public ThinkGearController wavemind;
	void Start () {
		Portname = "COM5";
		handleID = ThinkGear.TG_GetNewConnectionId();
		connectStatus = ThinkGear.TG_Connect(handleID, Portname, baudRate, packetType);
// or that the headset is not turned on
		if(connectStatus >= 0){ 
			//yield return new WaitForSeconds(1.5f);
			Thread.Sleep (1500);
	 		int packetCount = ThinkGear.TG_ReadPackets(handleID, -1);
			if(packetCount > 0){
				TriggerEvent("OnHeadsetConnected", null);
				
				// now set up a repeating invocation to update the headset data
				InvokeRepeating("UpdateHeadsetData", 0.0f, 1.0f);
			}
			// if we didn't find anything, then the connection attempt
			// failed. notify the rest of the GOs in the game
			else {
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
	void Update () {
		UpdateHeadsetData ();
		//guiText.text = "Connect Status: " + connectStatus.ToString()+ "Value" + Value.ToString()+ " Signal Strength: " + poorSignal.ToString("F2")+ "Attention: " + attention.ToString("F2")+ "meditation: " + meditation.ToString("F2");//obj.HRV.ToString("F2") + "\n" + "Resistance: " + obj.resistance.ToString("F2") + " Conductance: " + obj.conductance.ToString("F2");
	}
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
			Value = GetDataValue(ThinkGear.DATA_DELTA)+GetDataValue(ThinkGear.DATA_THETA)+GetDataValue(ThinkGear.DATA_ALPHA1)+GetDataValue(ThinkGear.DATA_ALPHA2);
			attention =  GetDataValue(ThinkGear.DATA_ATTENTION);
			meditation = GetDataValue(ThinkGear.DATA_MEDITATION);	
			poorSignal = 200 - GetDataValue(ThinkGear.DATA_POOR_SIGNAL);			
			TriggerEvent("OnHeadsetDataReceived", values);
		}
		
	}
//	
	void OnGUI()
	{
		//		GUILayout.BeginHorizontal();
		//		GUILayout.Space(Screen.width-250);
		//		GUILayout.Label(signalIcons[indexSignalIcons]);	
		//		GUILayout.EndHorizontal();
		//		GUILayout.Space(Screen.width-250);
		//		GUILayout.Label(new Rect(650, 650, 300, 50),"Heart Rate:  + (int)(obj.HrBeat)",largeFont);
		//		GUILayout.Label(Rect(430,320,500,500),"<color=green><size=100>Win</size></color>");
		GUILayout.Label("delta: " + GetDataValue(ThinkGear.DATA_DELTA));
		GUILayout.Label("theta: " + GetDataValue(ThinkGear.DATA_THETA));
		GUILayout.Label("lowAlpha: " + GetDataValue(ThinkGear.DATA_ALPHA1));
		GUILayout.Label("highAlpha: " + GetDataValue(ThinkGear.DATA_ALPHA2));
		GUILayout.Label("lowBeta: " + GetDataValue(ThinkGear.DATA_BETA1));
		GUILayout.Label("highBeta " + GetDataValue(ThinkGear.DATA_BETA2));
		GUILayout.Label("lowGamma: " + GetDataValue(ThinkGear.DATA_GAMMA1));
		GUILayout.Label("highGamma: " + GetDataValue(ThinkGear.DATA_GAMMA2));
		GUILayout.Label("Signal strength: " + (200 - GetDataValue(ThinkGear.DATA_POOR_SIGNAL)));
		GUILayout.Label("attention: " + GetDataValue(ThinkGear.DATA_ATTENTION));
		GUILayout.Label("meditation: " + GetDataValue(ThinkGear.DATA_MEDITATION));
		//		GUILayout.Label("Delta:" + delta);
		
	}

	// Convenience method to trigger an event on all GOs
	private void TriggerEvent(string eventName, System.Object parameter){
		foreach(GameObject go in FindObjectsOfType(typeof(GameObject)))
			go.SendMessage(eventName, parameter, 
			               SendMessageOptions.DontRequireReceiver); 
	}
//	
//	// Convenience method to retrieve a value from the headset
	private float GetDataValue(int valueType){
		return ThinkGear.TG_GetValue(handleID, valueType);
	}
	void OnDestroy()
	{
		OnHeadsetDisconnectionRequest ();
	}
}
