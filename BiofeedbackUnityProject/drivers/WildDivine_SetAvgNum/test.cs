using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class test : MonoBehaviour {
	public GUIText HRText;
	public int heartRate;	//heart beat rate
	public float scldata;	// scldata indicates the Skin Conductance Level measurement

	[DllImport("C:/Users/azhang/Desktop/WildDivine/lightstone_avg.dll")]//
	extern static int lightstone_Initial();				//initialization
	
	[DllImport("C:/Users/azhang/Desktop/WildDivine/lightstone_avg.dll")]
	extern static int lightstone_ReadBPM();				//read heart beat data

	[DllImport("C:/Users/azhang/Desktop/WildDivine/lightstone_avg.dll")]
	extern static int lightstone_SetAvgNum(int num);	//set avergae peaks number

	[DllImport("C:/Users/azhang/Desktop/WildDivine/lightstone_avg.dll")]
	extern static float lightstone_Readscl();			//read skin Conductance data

	[DllImport("C:/Users/azhang/Desktop/WildDivine/lightstone_avg.dll")]
	extern static int lightstone_Exit();
	
	// Use this for initialization
	void Start () {
		Debug.Log("Log Start =======");
		int p = lightstone_Initial();   		//This is the initiaization of Wild Divine sensor
		lightstone_SetAvgNum(10);                // set avergae peaks number
	}
	
	// Update is called once per frame
	void Update () {
		heartRate = lightstone_ReadBPM();       //lightstone_ReadBPM function can directly read the heart beat rate
		scldata = lightstone_Readscl();			//lightstone_Readscl function read Skin Conductance data
		UpdateHeartRateText();
	}
	
	void UpdateHeartRateText() {
		Debug.Log("HR: " + heartRate);
		guiText.text = "Heart Rate: " + heartRate +", Skin Conduct: "+ scldata;
	}

	void OnApplicationQuit()
	{
		Debug.Log("Test Exit");
		lightstone_Exit();						//free the resources of sensor
	}
}