using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// eHealth Arduino sensors. Initializing, and reading from Arduino sensors.
/// Requires ui Text elements to display the readings
/// and uses a FileWriter to write them to text files.
/// </summary>
public class EHealthArduinoSensors : MonoBehaviour
{
	// the class assumes that the following Text properties are not null!
	public Text statusText;
	public Text sclText;
	public Text hrvText;
	public Text qrsText;
	public Text bpmText;
	private float sclData;	// Skin Conductance Level
	private float hrvData;	// Heart Rate Variability
	private bool qrsData;	// in a QRS complex right now? (a heart signal peak)
	private float bpmData;	// Heart beats per minute
	private EHealthArduino ehArd;
	
	// Called once initally, setting up UI components
	void Awake ()
	{
		statusText.text = "Not connected";
		sclText.text = "N/A yet";
		hrvText.text = "N/A yet";
		qrsText.text = "N/A yet";
		bpmText.text = "N/A yet";
	}
	
	// Called once when enabled, using this for hardware initialization
	void Start ()
	{
		Debug.Log ("EHealth-Arduino device Initializing");
		/*		obj = new Datafetch();
		obj.sw = Stopwatch.StartNew();
		obj.sw_Air = Stopwatch.StartNew();
		obj.sw.Start();
		obj.sw_Air.Start();
		obj.swith = 1;
		mytest = new Thread(obj.subThread);
		//Strat the thread
		mytest.Start();
*/
		ehArd = new EHealthArduino ();
//		int deviceCount = ehArd.setup ();
//		statusText.text = string.Format("Initialized ({0} found)", deviceCount);
//		ehArd.setAvgNum (10); // set avergae peaks number
//		ehArd.startReadingData ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (ehArd != null) {
//			sclData = ehArd.getSCL ();
//			hrvData = ehArd.getHRV ();
//			qrsData = ehArd.getQRS ();
//			bpmData = ehArd.getBPM ();
			UpdateEHealthArduinoDataUIText ();
			WriteEHealthArduinoDataFile ();
		}
	}
	/*	void OnGUI()
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
*/	
	void UpdateEHealthArduinoDataUIText ()
	{
		sclText.text = sclData.ToString ("R");
		hrvText.text = hrvData.ToString ("R");
		qrsText.text = qrsData.ToString ();
		bpmText.text = bpmData.ToString ("R");
	}
	
	void WriteEHealthArduinoDataFile ()
	{
		FileWriter.TxtSaveByStr ("EHealthArduino_SCL", sclData.ToString ("R"));
		FileWriter.TxtSaveByStr ("EHealthArduino_HRV", hrvData.ToString ("R"));
		FileWriter.TxtSaveByStr ("EHealthArduino_QRS", qrsData.ToString ());
		FileWriter.TxtSaveByStr ("EHealthArduino_BPM", bpmData.ToString ("R"));
	}
	
	void OnDestroy ()
	{
//		obj.swith = 0;
		if (ehArd != null) {
			Debug.Log ("EHealth-Arduino Device Exiting");
//			ehArd.stopReadingData ();
//			ehArd.close (); // free the resources
			ehArd = null;
		}
	}
}
