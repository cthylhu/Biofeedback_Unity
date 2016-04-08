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
	public Text rawECGText;
	public Text hrBeatText;
	public Text intervalText;
	public Text eHRVText;
	public Text eGSRText;
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
	public int ID_num;
	public int markNumber = 1;
	public double timestamp;
	
	// Called once initally, setting up UI components
	void Awake ()
	{
		statusText.text = "Not connected";
		dataText.text = "N/A yet";
		rawECGText.text = "N/A yet";
		hrBeatText.text = "N/A yet";
		intervalText.text = "N/A yet";
		eHRVText.text = "N/A yet";
		eGSRText.text = "N/A yet";
	}
	
	// Called once when enabled, using this for hardware initialization
	void Start ()
	{
		Debug.Log ("EHealth-Arduino device Initializing");
		ehArd = new EHealthArduino ();
		ehArd.setup (Portname);
		statusText.text = "Initialized "+Portname;
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
			eGSRvalueData = ehArd.GSRread;
			
			rawBreathingData = ehArd.sampleAir;				//unused: breathing sensor
			breathingBeatData = ehArd.BreathingBeat;
			intervalAirData = ehArd.interval_Air; 	
			eHRpData = ehArd.PulseOxHR;						//unused: pulseoximeter

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
		/*dataText.text = string.Format ("hrBeat: \t{0}\ninterval: \t{1}\nhrv: \t{2}\nbreathingBeat: \t{3}\n"
		 	+ "intervalAir: \t{4}\neGSRvalue: \t{5}\neHRp: \t{6}\nrawECG: \t{7}\nrawBreathing: \t{8}",
			hrBeatData, intervalData, hrvData, breathingBeatData, intervalAirData, eGSRvalueData, eHRpData, rawECGData, rawBreathingData);*/
		/*dataText.text = string.Format ("rawECG: \t{0}\nhrBeat: \t{1}\ninterval: \t{2}\nhrv: \t{3}\n"+ "eGSRvalue: \t{4}",
				rawECGData, hrBeatData, intervalData, hrvData, eGSRvalueData);*/
		rawECGText.text = rawECGData.ToString ("R");
		hrBeatText.text = hrBeatData.ToString ("R");
		intervalText.text = intervalData.ToString ("R");
		eHRVText.text = hrvData.ToString ("R");
		eGSRText.text = eGSRvalueData.ToString ("R");
	}
	
	void WriteEHealthArduinoDataFile ()
	{
		if (ID_num == 1) {
			FileWriter.TxtSaveByStr ("EHealth_rawECG", rawECGData.ToString ("R") + "," + timestamp + "," + "0");
			FileWriter.TxtSaveByStr ("EHealth_HRV", hrvData.ToString ("R") + "," + timestamp + "," + "0");
			FileWriter.TxtSaveByStr ("EHealth_hrBeat", hrBeatData.ToString ("R") + "," + timestamp + "," + "0");
			FileWriter.TxtSaveByStr ("EHealth_GSR", eGSRvalueData.ToString ("R") + "," + timestamp + "," + "0");
		} 
		else {
			FileWriter.TxtSaveByStr ("EHealth2_rawECG", rawECGData.ToString ("R") + "," + timestamp + "," + "0");
			FileWriter.TxtSaveByStr ("EHealth2_HRV", hrvData.ToString ("R") + "," + timestamp + "," + "0");
			FileWriter.TxtSaveByStr ("EHealth2_hrBeat", hrBeatData.ToString ("R") + "," + timestamp + "," + "0");
			FileWriter.TxtSaveByStr ("EHealth2_GSR", eGSRvalueData.ToString ("R") + "," + timestamp + "," + "0");
		}
	}
	
	void WriteTimeStamp ()
	{
		if (ID_num == 1) {
			FileWriter.TxtSaveByStr ("EHealth_rawECG", rawECGData.ToString ("R") + "," + timestamp + "," + "1");		// Third column = 1 if there is a mark
			FileWriter.TxtSaveByStr ("EHealth_HRV", hrvData.ToString ("R") + "," + timestamp + "," + "1");
			FileWriter.TxtSaveByStr ("EHealth_hrBeat", hrBeatData.ToString ("R") + "," + timestamp + "," + "1");		
			FileWriter.TxtSaveByStr ("EHealth_GSR", eGSRvalueData.ToString ("R") + "," + timestamp + "," + "1");
		} 
		else {
			FileWriter.TxtSaveByStr ("EHealth2_rawECG", rawECGData.ToString ("R") + "," + timestamp + "," + "1");		// Third column = 1 if there is a mark
			FileWriter.TxtSaveByStr ("EHealth2_HRV", hrvData.ToString ("R") + "," + timestamp + "," + "1");
			FileWriter.TxtSaveByStr ("EHealth2_hrBeat", hrBeatData.ToString ("R") + "," + timestamp + "," + "1");		
			FileWriter.TxtSaveByStr ("EHealth2_GSR", eGSRvalueData.ToString ("R") + "," + timestamp + "," + "1");
		}
	}
	
	void OnDestroy ()
	{
		if (ehArd != null) {
			Debug.Log ("EHealth-Arduino Device Exiting");
			ehArd.stopReadingData ();
		//	ehArd.close (); // free the resources
			ehArd = null;
		}
	}
}
