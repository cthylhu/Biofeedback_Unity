using UnityEngine;
using UnityEngine.UI;
using PhySigTK;

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
	public Text eSCLText;
    public Grapher sclGraph;
    public Grapher ecgGraph;
	public Grapher emgGraph;
	private double rawECGData;
	private double hrBeatData;
	private double intervalData;
	private double hrvData;
	private double rawBreathingData;
	private double breathingBeatData;
	private double intervalAirData;
	private double eSCLvalueData;
	private double eHRpData;
	// port can be set in the inspector of the Panel for now (TODO: auto-discovery)
	// common name on Mac OS: /dev/tty.usbmodemXYZ (where XYZ are numbers)
	public int ID_num;
	public int markNumber = 1;
	
	// Called once initally, setting up UI components
	void Awake ()
	{
		statusText.text = "Not connected";
		dataText.text = "N/A yet";
		rawECGText.text = "N/A yet";
		hrBeatText.text = "N/A yet";
		intervalText.text = "N/A yet";
		eHRVText.text = "N/A yet";
		eSCLText.text = "N/A yet";
	}
	
	public void SCLUpdated(TimeStampedFloatList values)
	{
		foreach (TimeStampedValue<float> val in values) {
			eSCLvalueData = val.Value;
			if (sclGraph != null) {
				sclGraph.AddPoint(val.TimeStamp, eSCLvalueData);
			}
			//WriteEHealthArduinoDataFile("GSR", eSCLvalueData, val.TimeStamp);
		}
	}

	public void ECGUpdated(TimeStampedFloatList values)
	{
		foreach (TimeStampedValue<float> val in values) {
			rawECGData = val.Value;
			if (ecgGraph != null) {
				ecgGraph.AddPoint(val.TimeStamp, rawECGData);
			}
			//WriteEHealthArduinoDataFile("rawECG", rawECGData, val.TimeStamp);
		}
	}

	public void EMGUpdated(TimeStampedFloatList values)
	{
		foreach (TimeStampedValue<float> val in values) {
			if (emgGraph != null) {
				emgGraph.AddPoint(val.TimeStamp, val.Value);
			}
		}
	}

	void Update ()
	{
		UpdateEHealthArduinoDataUIText ();
        if (Input.GetKeyDown("space")) {
			WriteAllDataFilesWithMark();
            Debug.Log("=====MARK"+markNumber+"=====");
			markNumber++;
		}
	}

	void UpdateEHealthArduinoDataUIText ()
	{
		rawECGText.text = rawECGData.ToString ("R");
		hrBeatText.text = hrBeatData.ToString ("R");
		intervalText.text = intervalData.ToString ("R");
		eHRVText.text = hrvData.ToString ("R");
		eSCLText.text = eSCLvalueData.ToString ("R");
	}

	void WriteAllDataFilesWithMark()
	{
		long timestamp = HiResTiming.CurrentTimeStamp;
		WriteEHealthArduinoDataFile("rawECG", rawECGData, timestamp, "1");
		WriteEHealthArduinoDataFile("GSR", eSCLvalueData, timestamp, "1");
		// TODO: check if those two are still relevant after PhySigTK update:
		WriteEHealthArduinoDataFile("HRV", hrvData, timestamp, "1");
		WriteEHealthArduinoDataFile("hrBeat", hrBeatData, timestamp, "1");
	}

	void WriteEHealthArduinoDataFile (string destination, double data, long timestamp, string mark="0")
	{
		string prefix = "EHealth_";
		if (ID_num != 1) {
			prefix = "EHealth" + ID_num + "_";
		}
		FileWriter.TxtSaveByStr(prefix + destination, data.ToString("R") + "," + timestamp + "," + mark);
	}
	
}
