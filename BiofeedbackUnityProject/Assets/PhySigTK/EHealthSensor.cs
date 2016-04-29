using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;

namespace PhySigTK
{
	/// <summary>
	/// Represents ehealth arduino sensors.
	/// Can start readers and use parsers to prepare the values for use.
	/// UnityEvents are provided to react to updates.
	/// </summary>
	public class EHealthSensor : MonoBehaviour
	{
		public StatusChangeEvent StatusChanged = new StatusChangeEvent();
		public TimeStampedDataEvent SCLUpdated = new TimeStampedDataEvent();
		public TimeStampedDataEvent EMGUpdated = new TimeStampedDataEvent();
		public TimeStampedDataEvent ECGUpdated = new TimeStampedDataEvent();
		private EHealthReader ehReader;
		private EHealthParser ehParser;

		void Start()
		{
			StatusChanged.Invoke("Initializing");
			ehReader = new EHealthReader();
			ehParser = new EHealthParser();
			ehReader.Setup();
			StatusChanged.Invoke("Setup Port " + ehReader.PortName);
			ehReader.StartReadingData();
			StatusChanged.Invoke("Reading Port " + ehReader.PortName);
		}

		void Update()
		{
			TimeStampedValue<string>[] values = ehReader.RetrieveData();
			if (values.Length > 0) {
				ehParser.ParseValues(values);
				// now retrieve the newest timestampedvalues for EMG, SCL, ECG and trigger events
				TimeStampedFloatList emgValues = ehParser.RetrieveNewEMGValues();
				TimeStampedFloatList ecgValues = ehParser.RetrieveNewECGValues();
				TimeStampedFloatList sclValues = ehParser.RetrieveNewSCLValues();
				//DebugLogValues("EMG", emgValues);
				//DebugLogValues("ECG", ecgValues);
				//DebugLogValues("SCL", sclValues);
				EMGUpdated.Invoke(emgValues);
				ECGUpdated.Invoke(ecgValues);
				SCLUpdated.Invoke(sclValues);
			}
		}

		void DebugLogValues(string prefix, TimeStampedFloatList values)
		{
			foreach (TimeStampedValue<float> val in values) {
				Debug.Log(prefix + " " +
					HiResTiming.getDateTimeForTimestamp(val.TimeStamp).ToString("o")
					+ " " + val.Value);
			}
		}

		void OnDisable()
		{
			if (ehReader != null) {
				if (ehReader.IsReadingData()) {
					ehReader.StopReadingData();
					StatusChanged.Invoke("Stopped Reading Port " + ehReader.PortName);
				}
			}
		}

		void OnEnable()
		{
			if (ehReader != null) {
				ehReader.StartReadingData();
				StatusChanged.Invoke("Restarted Reading Port " + ehReader.PortName);
			}
		}
	}
}
