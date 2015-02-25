using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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

	private int handleID = -1;
	private int connectStatus = -1;
	private int baudRate = ThinkGear.BAUD_9600;
	private int packetType = ThinkGear.STREAM_PACKETS;

	// connect to a new ThinkGear device for reading if found
	public int setup(string portName = "COM7")
	{
		handleID = ThinkGear.TG_GetNewConnectionId();
		if (handleID < 0) {
			Debug.Log ("Error in setting up the connection object!", this);
			return handleID;
		}
		connectStatus = ThinkGear.TG_Connect (handleID, portName, baudRate, packetType);
		return connectStatus;
	}

	/// start Reading Data and send events to the parameter
	public bool startReadingData()
	{
		if (handleID < 0 || connectStatus < 0) {
			Debug.Log ("Cannot start reading, no connection!", this);
			return false;
		}
		StartCoroutine(readingLoopStarter());
		return true;
	}

	// Attempt to connect to the headset, start as Coroutine to be able to wait a bit
	private IEnumerator readingLoopStarter ()
	{
		// now we need to check that the headset is returning valid data.
		// the headset transmits data every second, so sleep for some 
		// interval longer than that to guarantee data received in the 
		// serial buffer, and retry a few times:
		int retries = 0;
		int packetCount = 0;
		for (retries = 0; retries < 50; ++retries) {
			Debug.Log ("MindWave initial connection try " + retries);
			// we use the Unity-specific yield statement here so that the
			// thread doesn't block everything else while it's sleeping, 
			// like Thread.Sleep() would have.
			yield return new WaitForSeconds (1.0f);
			packetCount = ThinkGear.TG_ReadPackets (handleID, -1);
			// if we received some packets, then the connection attempt was successful
			// notify the GOs in the game
			if (packetCount > 0) {
				// successfully received the first set of data!
				Debug.Log (string.Format("Mindwave got data after trying {0} times! packetCount: {1}", retries, packetCount), this);
				if (OnHeadsetConnected != null) {
					OnHeadsetConnected(packetCount);
				}
				ReportData (); // initial reading
				// now set up a repeating invocation to update the headset data
				InvokeRepeating ("UpdateHeadsetData", 0.0f, 1.0f);
				yield break; // we are done here
			} else if (packetCount < 0) {
				break; // an error code, exit the for loop
			}
		}
		Debug.Log (string.Format("MindWave: No data after trying {0} times! packetCount: {1}", retries, packetCount), this);
		// this is an error condition, report it
		// either we didn't find anything, or there was an error
		if (OnHeadsetConnectionError != null) {
			OnHeadsetConnectionError(packetCount);
		}
		reset ();
	}

	private void reset()
	{
		if (handleID != -1) {
			// free the handle
			ThinkGear.TG_FreeConnection (handleID);
		}
		handleID = -1;
		connectStatus = -1;
	}

	// when we receive a headset disconnection request, attempt to disconnect.
	public void disconnect ()
	{
		CancelInvoke (); // the only one should be UpdateHeadsetData or none at all
		reset ();
		if (OnHeadsetDisconnected != null) {
			OnHeadsetDisconnected();
		}
	}

	// Repeating callback method to retrieve data from the headset
	private int UpdateHeadsetData ()
	{
		int packetCount = ThinkGear.TG_ReadPackets (handleID, -1);
		if (packetCount > 0) {
			ReportData();
		}
		return packetCount;
	}

	private void ReportData ()
	{
		IDictionary<string, float> values = new Dictionary<string, float> ();
		values.Add ("poorSignal", GetDataValue (ThinkGear.DATA_POOR_SIGNAL));  
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
		if (OnHeadsetDataReceived != null) {
			OnHeadsetDataReceived(values);
		}
	}

	// Convenience method to retrieve a value from the headset
	private float GetDataValue (int valueType)
	{
		return ThinkGear.TG_GetValue (handleID, valueType);
	}
}
