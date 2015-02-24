using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;


public class Sc : MonoBehaviour {
	GUIStyle largeFont,smallFont;
	//HR & breathing thread
	public Datafetch obj;
	public Thread mytest;
	//public static int ECGCounter;

	void Start () {
		// check current directory to ensure relative paths for libraries are correct:
		UnityEngine.Debug.Log("Current Directory: " + Directory.GetCurrentDirectory());
		obj = new Datafetch();
		obj.sw = Stopwatch.StartNew();
		obj.sw_Air = Stopwatch.StartNew();
		obj.sw.Start();
		obj.sw_Air.Start();
		obj.swith = 1;
		mytest = new Thread(obj.subThread);
		//Strat the thread
		mytest.Start();
		largeFont = new GUIStyle();
		smallFont = new GUIStyle();
		largeFont.fontSize = 32;
		smallFont.fontSize = 24;
		
	}

	void OnGUI()
	{
		GUILayout.Label("E-health sensor",largeFont);
		GUILayout.Label("Heart Rate (from ECG): " + (int)(obj.HrBeat),smallFont);
		GUILayout.Label("Heart Interval: " + (int)(obj.interval),smallFont);
		GUILayout.Label("Heart Rate Variability: " + (obj.Hrv),smallFont);
		GUILayout.Label("Breathing Rate: " + (int)(obj.BreathingBeat),smallFont);
		GUILayout.Label("Breathing Interval: " + (int)(obj.interval_Air),smallFont);
		GUILayout.Label("GSR: " + (obj.eGSRvalue),smallFont);
        GUILayout.Label("Heart Rate (from Pulseoximeter): " + (obj.eHRp), smallFont);
	}


	void OnDestroy () {
		obj.swith = 0;
	}
}