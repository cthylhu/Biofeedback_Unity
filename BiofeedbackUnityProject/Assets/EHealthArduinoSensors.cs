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
	// port can be set in the inspector of the Panel for now (TODO: auto-discovery)
	// common name on Mac OS: /dev/tty.usbmodemXYZ (where XYZ are numbers)
	public string Portname;
	public int markNumber = 1;
	public double timestamp;
	
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
		ehArd.setup (Portname);
		statusText.text = "Initialized";
//		ehArd.setAvgNum (10); // set avergae peaks number
		ehArd.startReadingData ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (ehArd != null) {
			rawECGData = ehArd.ECGVoltage;
			hrBeatData = ehArd.HrBeat;
			intervalData = ehArd.interval;
			hrvData = ehArd.Hrv;
			//rawBreathingData = ehArd.sampleAir;
			//breathingBeatData = ehArd.BreathingBeat;
			//intervalAirData = ehArd.interval_Air; 	//unused: breathing sensor
			eGSRvalueData = ehArd.GSRread;
			//eHRpData = ehArd.PulseOxHR;					//unused: pulseoximeter
			timestamp = Time.time;
			UpdateEHealthArduinoDataUIText ();
			WriteEHealthArduinoDataFile ();
		}
		
		if (Input.GetKeyDown("space")) {
			WriteTimeStamp ();
			Debug.Log("=====MARK"+markNumber+"=====");
			markNumber++;

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
		FileWriter.TxtSaveByStr ("EHealth_rawECG", rawECGData.ToString ("R")+","+timestamp+","+"0");
		FileWriter.TxtSaveByStr ("EHealth_HRV", hrvData.ToString ("R")+","+timestamp+","+"0");
		FileWriter.TxtSaveByStr ("EHealth_hrBeat", hrBeatData.ToString ("R")+","+timestamp+","+"0");
		FileWriter.TxtSaveByStr ("EHealth_GSR", eGSRvalueData.ToString ("R")+","+timestamp+","+"0");
	}
	
	void WriteTimeStamp ()
	{
		FileWriter.TxtSaveByStr ("EHealth_rawECG", rawECGData.ToString ("R")+","+timestamp+","+"1");
		FileWriter.TxtSaveByStr ("EHealth_HRV", hrvData.ToString ("R")+","+timestamp+","+"1");
		FileWriter.TxtSaveByStr ("EHealth_hrBeat", hrBeatData.ToString ("R")+","+timestamp+","+"1");		// Third column = 1 if there is a mark
		FileWriter.TxtSaveByStr ("EHealth_GSR", eGSRvalueData.ToString ("R")+","+timestamp+","+"1");
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
