using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using NeuroSky.ThinkGear;
using NLog;

/**
 * The ThinkGearController class provides an event-based mechanism for scripts
 * in your Unity project to control and receive data from the headset.
 * 
 * Basic methods:
 * setup - sets up the connection and triggers the start of the continuous reading loop
 * disconnect - stops the loop
 * 
 * Events are defined in the class using the C# delegate/event mechanism:
 * All the events will be called from Update() and therefore in the Unity main loop
 * (necessarily with some delay from the originating event in the reading loop),
 * so the event handlers can use unity functions.
 */

public delegate void HeadsetConnected(int packetCount); // Sent when the headset has successfully sent data the first time
public delegate void HeadsetDisconnected(); // Sent when the headset has been disconnected
public delegate void HeadsetDataReceived(IDictionary<string, double> data); // Sent when data is received from the headset
public delegate void HeadsetConnectionError(int packetError); // Sent when a startReadingData attempt failed.

public class ThinkGearController: MonoBehaviour
{
	public event HeadsetConnected OnHeadsetConnected;
	public event HeadsetDisconnected OnHeadsetDisconnected;
	public event HeadsetDataReceived OnHeadsetDataReceived;
	public event HeadsetConnectionError OnHeadsetConnectionError;

	public const string dataLabelsString =
		"PoorSignal Attention Meditation HeartRate EegPowerDelta EegPowerTheta EegPowerAlpha1 EegPowerAlpha2 EegPowerBeta1 EegPowerBeta2 EegPowerGamma1 EegPowerGamma2";
	public static readonly string[] dataLabels = dataLabelsString.Split();

	private Connector connector;
	// internal task queue for communication with unity thread:
	private delegate void Task();
	private Queue<Task> TaskQueue = new Queue<Task>();
	private object _queueLock = new object();
	private const int MAX_TASKS_PER_FRAME = 5;
	private const int MAX_TASKS_IN_QUEUE = 100;

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

	// when we receive a headset disconnection request, attempt to disconnect.
	public void disconnect ()
	{
		connector.Disconnect();
		connector.Close();
		if (OnHeadsetDisconnected != null) {
			OnHeadsetDisconnected();
		}
	}
	
	// Update is called once per frame: get tasks from the queue in main thread
	void Update ()
	{
		lock (_queueLock) {
			if (TaskQueue.Count >= MAX_TASKS_IN_QUEUE) {
				Debug.LogWarning("TGController: TaskQueue full!!!");
			}
			int taskCount = MAX_TASKS_PER_FRAME;
			while (TaskQueue.Count > 0 && taskCount > 0) {
				TaskQueue.Dequeue()();
				taskCount--;
			}
		}
	}
	
	private void ScheduleTask(Task newTask)
	{
		lock (_queueLock) {
			if (TaskQueue.Count < MAX_TASKS_IN_QUEUE) { // warn in eventdispatcher (i.e. Update())
				TaskQueue.Enqueue(newTask);
			}
		}
	}

	// All the handlers below are called from the reading thread, and therefore
	// need to use ScheduleTask to dispatch events to the main unity thread!
	
	void HandleOnDeviceConnected(object sender, EventArgs e)
	{	
		Connector.DeviceEventArgs deviceEventArgs = (Connector.DeviceEventArgs) e;
		Debug.Log( "New Headset Created: " + deviceEventArgs.Device.PortName );
		ScheduleTask(new Task(delegate {
			if (OnHeadsetConnected != null) {
				OnHeadsetConnected(0); // TODO: meaningful return value?
			}
		}));
		// set up event handling for data:
		deviceEventArgs.Device.DataReceived += this.HandleOnDataReceived;
	}

	void HandleOnDeviceValidating(object sender, EventArgs e)
	{
		Connector.ConnectionEventArgs connEventArgs = (Connector.ConnectionEventArgs) e;
		Debug.Log ("ThinkGearController DeviceValidating: " + connEventArgs.Connection);
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
		ScheduleTask(new Task(delegate {
			if (OnHeadsetConnectionError != null) {
				OnHeadsetConnectionError(0);
			}
		}));
	}
	
	void HandleOnDeviceDisconnected(object sender, EventArgs e)
	{
		Debug.Log ("ThinkGearController DeviceDisconnected: " + e);
		ScheduleTask(new Task(delegate {
			if (OnHeadsetDisconnected != null) {
				OnHeadsetDisconnected();
			}
		}));
	}
	
	void HandleOnDataReceived(object sender, EventArgs e)
	{
		Debug.Log ("ThinkGearController DataReceived: " + e);
		/* Cast the event sender as a Device object, and e as the Device's DataEventArgs */
		//Device d = (Device) sender;
		Device.DataEventArgs de = (Device.DataEventArgs) e;
		/* Create a TGParser to parse the Device's DataRowArray[] */
		TGParser tgParser = new TGParser();
		tgParser.Read( de.DataRowArray );
		/* Loop through parsed data TGParser for its parsed data... */
		for ( int i = 0; i < tgParser.ParsedData.Length; i++ ) {
			ReportData(tgParser.ParsedData[i]);
		}
	}

	private void ReportData(IDictionary<string, double> readings)
	{
		IDictionary<string, double> values = new Dictionary<string, double> ();
		values.Add ("Time", readings["Time"]); // always need a timestamp value
		foreach (string key in readings.Keys) {
			if (dataLabels.Contains(key)) {
				values.Add (key, readings[key]);
			} else if (key != "Time" && key != "Raw") {
				Debug.Log ("TGController: Ignoring data key " + key);
			}
		}
		ScheduleTask(new Task(delegate {
			if (OnHeadsetDataReceived != null) {
				OnHeadsetDataReceived(values);
			}
		}));
	}
}
