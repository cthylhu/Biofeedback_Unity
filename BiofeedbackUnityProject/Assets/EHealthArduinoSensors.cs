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
	public Text dataText;
	private double rawECGData;
	private double hrBeatData;
	private double intervalData;
	private double hrvData;
	private double rawBreathingData;
	private double breathingBeatData;
	private double intervalAirData;
	private double eGSRvalueData;
	private double eHRpData;
	private EHealthArduino ehArd;
	
	// Called once initally, setting up UI components
	void Awake ()
	{
		statusText.text = "Not connected";
		dataText.text = "N/A yet";
	}
	
	// Called once when enabled, using this for hardware initialization
	void Start ()
	{
		Debug.Log ("EHealth-Arduino device Initializing");
		ehArd = new EHealthArduino ();
		ehArd.setup ();
		statusText.text = "Initialized";
//		ehArd.setAvgNum (10); // set avergae peaks number
		ehArd.startReadingData ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (ehArd != null) {
			rawECGData = ehArd.sample;
			hrBeatData = ehArd.HrBeat;
			intervalData = ehArd.interval;
			hrvData = ehArd.Hrv;
			rawBreathingData = ehArd.sampleAir;
			breathingBeatData = ehArd.BreathingBeat;
			intervalAirData = ehArd.interval_Air;
			eGSRvalueData = ehArd.eGSRvalue;
			eHRpData = ehArd.eHRp;
			UpdateEHealthArduinoDataUIText ();
			WriteEHealthArduinoDataFile ();
		}
	}

	void UpdateEHealthArduinoDataUIText ()
	{
		dataText.text = string.Format ("hrBeat: \t{0}\ninterval: \t{1}\nhrv: \t{2}\nbreathingBeat: \t{3}\n"
		                               + "intervalAir: \t{4}\neGSRvalue: \t{5}\neHRp: \t{6}\nrawECG: \t{7}\nrawBreathing: \t{8}",
		    hrBeatData, intervalData, hrvData, breathingBeatData, intervalAirData, eGSRvalueData, eHRpData, rawECGData, rawBreathingData);
	}
	
	void WriteEHealthArduinoDataFile ()
	{
		//TODO
//		FileWriter.TxtSaveByStr ("EHealthArduino_SCL", sclData.ToString ("R"));
//		FileWriter.TxtSaveByStr ("EHealthArduino_HRV", hrvData.ToString ("R"));
//		FileWriter.TxtSaveByStr ("EHealthArduino_QRS", qrsData.ToString ());
//		FileWriter.TxtSaveByStr ("EHealthArduino_BPM", bpmData.ToString ("R"));
	}
	
	void OnDestroy ()
	{
		if (ehArd != null) {
			Debug.Log ("EHealth-Arduino Device Exiting");
			ehArd.stopReadingData ();
//			ehArd.close (); // free the resources
			ehArd = null;
		}
	}
}
