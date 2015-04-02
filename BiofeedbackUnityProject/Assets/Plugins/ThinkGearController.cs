using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using NeuroSky.ThinkGear;
using NLog;

/**
 * The ThinkGearController class provides an event-based mechanism for scripts
 * in your Unity project to control and receive data from the headset.
 * 
 * Basic methods:
 * setup - sets up the connection
 * startReadingData - triggers the start of the continuous reading loop
 * disconnect
 * 
 * Events are defined in the class using the C# delegate/event mechanism:
 */

public delegate void HeadsetConnected(int packetCount); // Sent when the headset has successfully sent data the first time
public delegate void HeadsetDisconnected(); // Sent when the headset has been disconnected
public delegate void HeadsetDataReceived(IDictionary<string, float> data); // Sent when data is received from the headset
public delegate void HeadsetConnectionError(int packetError); // Sent when a startReadingData attempt failed.

public class ThinkGearController: MonoBehaviour
{
	public event HeadsetConnected OnHeadsetConnected;
	public event HeadsetDisconnected OnHeadsetDisconnected;
	public event HeadsetDataReceived OnHeadsetDataReceived;
	public event HeadsetConnectionError OnHeadsetConnectionError;

	private Connector connector;

	// connect to a new ThinkGear device for reading if found
	public void setup(string portName = "COM7")
	{
		NLog.LogManager.DisableLogging(); // no need for logging from the ThinkGear code
		connector = new Connector();
		connector.DeviceConnected    += this.HandleOnDeviceConnected;
		connector.DeviceValidating   += this.HandleOnDeviceValidating;
		connector.DeviceFound        += this.HandleOnDeviceFound;
		connector.DeviceNotFound     += this.HandleOnDeviceNotFound;
		connector.DeviceConnectFail  += this.HandleOnDeviceConnectFail;
		connector.DeviceDisconnected += this.HandleOnDeviceDisconnected;
		connector.ConnectScan (portName);
	}

	void HandleOnDeviceConnected(object sender, EventArgs e)
	{	
		Connector.DeviceEventArgs deviceEventArgs = (Connector.DeviceEventArgs) e;
		Debug.Log( "New Headset Created: " + deviceEventArgs.Device.PortName );
		if (OnHeadsetConnected != null) {
			OnHeadsetConnected(0); // TODO: meaningful return value?
		}
		deviceEventArgs.Device.DataReceived += this.HandleOnDataReceived;
	}

	// when we receive a headset disconnection request, attempt to disconnect.
	public void disconnect ()
	{
		connector.Disconnect();
		connector.Close();
		if (OnHeadsetDisconnected != null) {
			OnHeadsetDisconnected();
		}
	}

	void HandleOnDeviceValidating(object sender, EventArgs e)
	{
		Debug.Log ("ThinkGearController DeviceValidating: " + e);
	}
	
	void HandleOnDeviceFound(object sender, EventArgs e)
	{
		Debug.Log ("ThinkGearController DeviceFound: " + e);
	}

	void HandleOnDeviceNotFound(object sender, EventArgs e)
	{
		Debug.Log ("ThinkGearController DeviceNotFound: " + e);
	}
	
	void HandleOnDeviceConnectFail(object sender, EventArgs e)
	{
		Debug.Log ("ThinkGearController DeviceConnectFail: " + e);
		if (OnHeadsetConnectionError != null) {
			OnHeadsetConnectionError(0);
		}
	}
	
	void HandleOnDeviceDisconnected(object sender, EventArgs e)
	{
		Debug.Log ("ThinkGearController DeviceDisconnected: " + e);
		if (OnHeadsetDisconnected != null) {
			OnHeadsetDisconnected();
		}
	}
	
	void HandleOnDataReceived(object sender, EventArgs e)
	{
		Debug.Log ("ThinkGearController DataReceived: " + e);
		/* Cast the event sender as a Device object, and e as the Device's DataEventArgs */
		Device d = (Device) sender;
		Device.DataEventArgs de = (Device.DataEventArgs) e;
		/* Create a TGParser to parse the Device's DataRowArray[] */
		TGParser tgParser = new TGParser();
		tgParser.Read( de.DataRowArray );
		/* Loop through parsed data TGParser for its parsed data... */
		for ( int i = 0; i < tgParser.ParsedData.Length; i++ ) {
			// See the Data Types documentation for valid keys such
			// as "Raw", "PoorSignal", "Attention", etc.			
			if( tgParser.ParsedData[i].ContainsKey("Raw") ){
				Debug.Log( "Raw Value:" + tgParser.ParsedData[i]["Raw"] );
			}
			if( tgParser.ParsedData[i].ContainsKey("PoorSignal") ){
				Debug.Log( "PQ Value:" + tgParser.ParsedData[i]["PoorSignal"] );
			}			
			if( tgParser.ParsedData[i].ContainsKey("Attention") ) {
				Debug.Log( "Att Value:" + tgParser.ParsedData[i]["Attention"] );
			}
			if( tgParser.ParsedData[i].ContainsKey("Meditation") ) {
				Debug.Log( "Med Value:" + tgParser.ParsedData[i]["Meditation"] );
			}
		}
		// TODO: change above to report via OnHeadsetDataReceived, see below
	}
	
	// Repeating callback method to retrieve data from the headset
	private int UpdateHeadsetData ()
	{
		int packetCount = 0; //ThinkGear.TG_ReadPackets (handleID, -1);
		if (packetCount > 0) {
			ReportData();
		}
		return packetCount;
	}

	private void ReportData ()
	{
		IDictionary<string, float> values = new Dictionary<string, float> ();
/*		values.Add ("poorSignal", GetDataValue (ThinkGear.DATA_POOR_SIGNAL));  
		values.Add ("attention", GetDataValue (ThinkGear.DATA_ATTENTION));
		values.Add ("meditation", GetDataValue (ThinkGear.DATA_MEDITATION));
		values.Add ("delta", GetDataValue (ThinkGear.DATA_DELTA));
		values.Add ("theta", GetDataValue (ThinkGear.DATA_THETA));
		values.Add ("lowAlpha", GetDataValue (ThinkGear.DATA_ALPHA1));
		values.Add ("highAlpha", GetDataValue (ThinkGear.DATA_ALPHA2));
		values.Add ("lowBeta", GetDataValue (ThinkGear.DATA_BETA1));
		values.Add ("highBeta", GetDataValue (ThinkGear.DATA_BETA2));
		values.Add ("lowGamma", GetDataValue (ThinkGear.DATA_GAMMA1));
		values.Add ("highGamma", GetDataValue (ThinkGear.DATA_GAMMA2));
*/		if (OnHeadsetDataReceived != null) {
			OnHeadsetDataReceived(values);
		}
	}
}
