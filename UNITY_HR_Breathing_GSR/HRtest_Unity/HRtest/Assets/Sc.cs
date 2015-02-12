using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
//using System.Threading.Tasks;

/// <summary>
/// Simple file writer that puts data files in a known foder with a known suffix.
/// </summary>
public class FileWriter
{
	static public string dataPath = @"../../Data/";
	static public string dataSuffix = @".txt";

	static public void TxtSaveByStr(string saveName, string txtStr)
	{
		string path = dataPath + saveName + dataSuffix;
		// This text is added only once to the file. 
		if (!File.Exists(path)) 
		{
			// Create a file to write to. 
			using (StreamWriter swrite = File.CreateText(path)) 
			{}
		}		
		// This text is always added, making the file longer over time if it is not deleted. 
		using (StreamWriter swrite = File.AppendText(path)) 
		{
			swrite.WriteLine(txtStr);
		}
	}

}

public class Datafetch
{
	SerialPort stream = new SerialPort("COM4", 115200); //Set the port (com4) and the baud rate (9600, is standard on most devices)
	public double ECGVoltage,BreVoltage,eGSRvalue;
	public int swith;
	public  int HRBeatCounter = 0, BreathingCounter_air = 0, AirCounter = 0, ECGCounter = 0, HrvCounter = 0;
//	HR peak parameters
	public double peakAG = 0;
	public double sample = 0,sample0 = 0;
	public double _attack = 0.9875;
	public double _decay = 0.992;
	public double gain;
	public double sampleAG;
	public double lower_bound = 0.9975;
	public double upper_bound = 0.99;
	public int near_peak = 0;
//	Breathing peak parameters
	public double peakAGAir = 0;
	public double sampleAir = 0,sampleAir0 = 0;
	public double _attackAir = 0.9875;
	public double _decayAir = 0.992;
	public double gainAir;
	public double sampleAGAir;
	public double lower_boundAir = 0.9975;
	public double upper_boundAir = 0.99;
	public int near_peakAir = 0;

	public int beats = 0, AvgNum = 0;
	public long tc,interval,interval_Air;
    public double timeSum, HrBeat, timeSum_Air, BreathingBeat, Hrv, eHRp;
	public Stopwatch sw, sw_Air;
	public double [] timeBuffer = new double[10];	
	public double [] HrvBuffer = new double[10]; // 
	public double [] timeBuffer_Air = new double[5];
    public double MaxValueECG = 0, MaxValueBre = 0;
	public int MaxCounterECG = 0, PeakFlagECG = 0, MaxCounterBre = 0, PeakFlagBre = 0;
	public double [] MaxBufferECG = new double[400];
	public double [] AirBuffer = new double[50];
	public double [] ECGBuffer = new double[50];
	public double [] MaxBufferBre = new double[400];
	public double  SumFlag = 1,SumFlag_air = 1,SumHRVflag = 1;
	public DateTime start;
	public TimeSpan timeDiff;

	public void subThread()
	{
		stream.Open(); //Open the Serial Stream.
		sw = new Stopwatch();
		sw_Air = new Stopwatch();
//		start = DateTime.Now;
		while (swith==1) {
			string value = stream.ReadLine(); 
			string[] vec2 = value.Split(',');
			string firstchar = value.Substring(0, 1);
			if (firstchar.Equals("B"))
			{
				ECGVoltage = double.Parse(vec2[1]);//double.Parse(value.Substring(1));
				BreVoltage = double.Parse(vec2[2]);
				eGSRvalue = double.Parse(vec2[3]);
                eHRp = double.Parse(vec2[4]);           // HR from Pulseoximeter
//Get the ECG max
				MaxBufferECG[MaxCounterECG] = ECGVoltage;
				MaxValueECG = MaxBufferECG.Max();
				MaxCounterECG ++;
				if (MaxCounterECG == 400)
					MaxCounterECG = 0;
				if (ECGVoltage > 0.75*MaxValueECG)
					PeakFlagECG = 1;
				else
					PeakFlagECG = 0;
//Get the breathing max
//				MaxBufferBre[MaxCounterBre] = BreVoltage;
//				MaxValueBre = MaxBufferBre.Max();
//				MaxCounterBre ++;
//				if (MaxCounterBre == 400)
//					MaxCounterBre = 0;
//				if (BreVoltage > 0.75*MaxValueBre)
//					PeakFlagBre = 1;
//				else
//					PeakFlagBre = 0;

// Write ECG raw data 
				string ECG = sample.ToString();
				FileWriter.TxtSaveByStr("ECG", ECG);
// Write HR
				string HR = HrBeat.ToString();
				FileWriter.TxtSaveByStr("HR", HR);
// Write HR from Pulseoximeter
                string eHR = eHRp.ToString();
                FileWriter.TxtSaveByStr("eHRp",  eHR);
// Write HRV
				string HRV = Hrv.ToString();
				FileWriter.TxtSaveByStr("HRV", HRV);
// Write GSR
				string eGSR = eGSRvalue.ToString();
				FileWriter.TxtSaveByStr("eGSR", eGSR);

// Write Breathing raw data
				string Breathing = sampleAir.ToString();
				FileWriter.TxtSaveByStr("Breathing", Breathing);
// Write BR
				string BR = BreathingBeat.ToString();
				FileWriter.TxtSaveByStr("BR", BR);				
			}
// calculate the ECG peak
			sample0 = GetData ();
//			ECGBuffer [ECGCounter] = sample0;
//			ECGCounter ++;
//			if (ECGCounter == 3)
//				ECGCounter = 0;
			sample = sample0;//ECGBuffer.Average();
			if (sample > peakAG)
				peakAG = _attack * sample;
			else
				peakAG = _decay * peakAG;
			
			gain = _attack / peakAG;
			sampleAG = gain * sample;
			if (sampleAG >= lower_bound)
				near_peak = 1;			
			if ((near_peak == 1) && (sampleAG < upper_bound) &&(PeakFlagECG == 1))
			{
				near_peak = 0;
				sw.Stop();
				interval = sw.ElapsedMilliseconds;
				//tc = sw.Elapsed;
				//HRBeatCounterPre = HRBeatCounter;
				if ((sw.ElapsedMilliseconds < 1500)&&(sw.ElapsedMilliseconds > 500))
				{
					string IntervalString = interval.ToString();
					FileWriter.TxtSaveByStr("Interval", IntervalString);
					timeBuffer [HRBeatCounter] = Convert.ToDouble(interval)/1000; 
					HRBeatCounter++;
					if (HRBeatCounter == 10)
					{
						HRBeatCounter = 0;						
					}

				}
				for (int i = 0; i < timeBuffer.Length; i++)
				{
					//timeSum = timeSum + timeBuffer[i];
					SumFlag = timeBuffer[i]*SumFlag;	
					SumHRVflag = HrvBuffer[i]*SumHRVflag;	
				}
				if (SumFlag != 0)
				{
					//HRBeatCounter = 0;
					for (int i = 0; i < 10; i++)
					{
						timeSum = timeSum + timeBuffer[i];
						
					}
					if (timeSum != 0)
					{
						HrBeat = 10 / timeSum * 60;     
						HrvBuffer[HrvCounter] =(double) HrBeat;
						HrvCounter++;
						if (HrvCounter == 10)
							HrvCounter = 0;
						if (SumHRVflag != 0)
						{
							double average = HrvBuffer.Average();
							double sumOfSquaresOfDifferences = HrvBuffer.Select(val => (val - average) * (val - average)).Sum();
							double sd = Math.Sqrt(sumOfSquaresOfDifferences / HrvBuffer.Length);
							Hrv = sd;
						}

					}
					timeSum = 0;				
				}
					SumFlag = 1;
					SumHRVflag = 1;
					//if (ECGCounter)
					sw.Reset();
					sw.Start();
				}

// calculate the Breathing peak
			sampleAir0 = BreVoltage*5;
			AirBuffer [AirCounter] = sampleAir0;
			AirCounter ++;
			if (AirCounter == 50)
				AirCounter = 0;
			sampleAir = AirBuffer.Average();
			if (sampleAir > peakAGAir)
				peakAGAir = _attackAir * sampleAir;
			else
				peakAGAir = _decayAir * peakAGAir;
			
			gainAir = _attackAir / peakAGAir;
			sampleAGAir = gainAir * sampleAir;
			if (sampleAGAir >= lower_boundAir)
				near_peakAir = 1;			
			if ((near_peakAir == 1) && (sampleAGAir < upper_boundAir))//&&(PeakFlagBre == 1)
			{
				near_peakAir = 0;
//				timeDiff = DateTime.Now - start;
				sw_Air.Stop();
				interval_Air = sw_Air.ElapsedMilliseconds;//(long)timeDiff.TotalMilliseconds
				//tc = sw.Elapsed;
				//HRBeatCounterPre = HRBeatCounter;
				if (interval_Air > 1000)
				{
					string IntervalBreString = interval_Air.ToString();
					FileWriter.TxtSaveByStr("IntervalBre", IntervalBreString);
					timeBuffer_Air [BreathingCounter_air] = Convert.ToDouble(interval_Air)/1000; 
					BreathingCounter_air++;
					if (BreathingCounter_air == 5)
					{
						BreathingCounter_air = 0;						
					}
					
				}
				for (int i = 0; i < timeBuffer_Air.Length; i++)
				{
					//timeSum = timeSum + timeBuffer[i];
					SumFlag_air = timeBuffer_Air[i]*SumFlag_air;	
				}
				if (SumFlag_air != 0)
				{
					//HRBeatCounter = 0;
					for (int i = 0; i < 5; i++)
					{
						timeSum_Air = timeSum_Air + timeBuffer_Air[i];
						
					}
					if (timeSum_Air != 0)
					{
						BreathingBeat = 5 / timeSum_Air * 60;                
					}
					timeSum_Air = 0;				
				}
				SumFlag_air = 1;	
				sw_Air.Reset();
				sw_Air.Start();
			}
			start = DateTime.Now;

			stream.BaseStream.Flush();
			}

	}
	
	// This method is used to read data
	public double GetData()
	{
		return (ECGVoltage);
	}
};

public class Sc : MonoBehaviour {
	public GUIText HRText;
	public float scldata;	// scldata indicates the Skin Conductance Level measurement
	public int BPM;
	GUIStyle largeFont,smallFont;
	// Use this for Mindwave initialization
	private int handleID = -1;
	private int baudRate = ThinkGear.BAUD_9600;
	private int packetType = ThinkGear.STREAM_PACKETS;
	public string Portname;
	public float poorSignal = 0;
	public float attention = 0;
	public float meditation = 0;
	public int connectStatus = 0;
	//HR & breathing thread
	public Datafetch obj;
	public Thread mytest;
	//Dll import
    [DllImport("../../drivers/WildDivine_SetAvgNum/lightstone_avg.dll")]
	extern static int lightstone_Initial();				//initialization

    [DllImport("../../drivers/WildDivine_SetAvgNum/lightstone_avg.dll")]
	extern static int lightstone_ReadBPM();				//read heart beat data

    [DllImport("../../drivers/WildDivine_SetAvgNum/lightstone_avg.dll")]
	extern static int lightstone_SetAvgNum(int num);	//set avergae peaks number

    [DllImport("../../drivers/WildDivine_SetAvgNum/lightstone_avg.dll")]
	extern static float lightstone_Readscl();			//read skin Conductance data

    [DllImport("../../drivers/WildDivine_SetAvgNum/lightstone_avg.dll")]
	extern static int lightstone_Exit();
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
		int p = lightstone_Initial();   		//This is the initiaization of Wild Divine sensor
		lightstone_SetAvgNum(10);                // set avergae peaks number
		largeFont = new GUIStyle();
		smallFont = new GUIStyle();
		largeFont.fontSize = 32;
		smallFont.fontSize = 24;
		
        // NeuroSky MindWave
		Portname = "COM5";                                              // MINDWAVE COM PORT
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

	void Awake()
	{
		//Application.targetFrameRate = -1;
	}
	
	// Update is called once per frame
	void Update () 
	{
		UpdateHeartRateText();
		UpdateHeadsetData ();
	}

	void UpdateHeartRateText() {
		//Debug.Log("HR: " + heartRate);
		//GSR from wild divine
		scldata = lightstone_Readscl();
		string GSRString = scldata.ToString("R");
		FileWriter.TxtSaveByStr("GSR", GSRString);
		//BPM from wild divine
		BPM = lightstone_ReadBPM ();
		string BPMString = BPM.ToString();
		FileWriter.TxtSaveByStr("BPM", BPMString);

		//BPM from wild divine
		string SigStrenString = poorSignal.ToString();
		FileWriter.TxtSaveByStr("MindStrength", SigStrenString);

		//BPM from wild divine
		string AttenString = attention.ToString();
		FileWriter.TxtSaveByStr("Atten", AttenString);

		//BPM from wild divine
		string MedString = meditation.ToString();
		FileWriter.TxtSaveByStr("Med", MedString);

		//guiText.text = "Heart Rate: " + (int)(obj.HrBeat)+ " Breathing Rate: " + (int)(obj.BreathingBeat) + " Skin Conduct: "+ scldata;//"Time Interval: " + (int)(obj.interval) + 
	}

	void OnGUI()
	{
//		GUILayout.BeginHorizontal();
//		GUILayout.Space(Screen.width-250);
//		GUILayout.Label(signalIcons[indexSignalIcons]);	
//		GUILayout.EndHorizontal();
//		GUILayout.Space(Screen.width-250);
//		GUILayout.Label(new Rect(650, 650, 300, 50),"Heart Rate:  + (int)(obj.HrBeat)",largeFont);
//		GUILayout.Label(Rect(430,320,500,500),"<color=green><size=100>Win</size></color>");
		GUILayout.Label("E-health sensor",largeFont);
		GUILayout.Label("Heart Rate (from ECG): " + (int)(obj.HrBeat),smallFont);
		GUILayout.Label("Heart Interval: " + (int)(obj.interval),smallFont);
		GUILayout.Label("Heart Rate Variability: " + (obj.Hrv),smallFont);
		GUILayout.Label("Breathing Rate: " + (int)(obj.BreathingBeat),smallFont);
		GUILayout.Label("Breathing Interval: " + (int)(obj.interval_Air),smallFont);
		GUILayout.Label("GSR: " + (obj.eGSRvalue),smallFont);
        GUILayout.Label("Heart Rate (from Pulseoximeter): " + (obj.eHRp), smallFont);
		//Wild divine
		GUILayout.Label("Wild Divine sensor",largeFont);
		GUILayout.Label("Skin Conduct: " + scldata,smallFont);
		GUILayout.Label("Heart beat: " + BPM,smallFont);
		//Mindwave
		GUILayout.Label("Mindwave sensor",largeFont);
		GUILayout.Label("Signal strength: " + poorSignal,smallFont);
		GUILayout.Label("attention: " + attention,smallFont);
		GUILayout.Label("meditation: " + meditation,smallFont);
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
			Value = GetDataValue(ThinkGear.DATA_DELTA) + GetDataValue(ThinkGear.DATA_THETA) + GetDataValue(ThinkGear.DATA_ALPHA1) + GetDataValue(ThinkGear.DATA_ALPHA2);
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

	void OnDestroy () {
		//print("Script was destroyed");
		obj.swith = 0;
		guiText.text = "Over";
		lightstone_Exit();						//free the resources of sensor
		OnHeadsetDisconnectionRequest(); //todo: check if still necessary?
	}
}