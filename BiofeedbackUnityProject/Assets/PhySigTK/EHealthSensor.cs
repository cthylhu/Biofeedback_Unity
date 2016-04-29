using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;

namespace PhySigTK
{
	[Serializable]
	public class TimeStampedDataEvent : UnityEvent<TimeStampedValue<float>[]> { }
	[Serializable]
	public class StatusChangeEvent : UnityEvent<string> { }

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
			StatusChanged.Invoke("EHealthSensor initializing");
			ehReader = new EHealthReader();
			ehParser = new EHealthParser();
			ehReader.Setup();
			StatusChanged.Invoke("EHealthSensor Setup Port " + ehReader.PortName);
			ehReader.StartReadingData();
			StatusChanged.Invoke("EHealthSensor Reading Port " + ehReader.PortName);
		}

		void Update()
		{
			TimeStampedValue<string>[] values = ehReader.RetrieveData();
			if (values.Length > 0) {
				ehParser.ParseValues(values);
				// now retrieve the newest timestampedvalues for EMG, SCL, ECG and trigger events
				TimeStampedValue<float>[] emgValues = ehParser.RetrieveNewEMGValues();
				TimeStampedValue<float>[] ecgValues = ehParser.RetrieveNewECGValues();
				TimeStampedValue<float>[] sclValues = ehParser.RetrieveNewSCLValues();
				DebugLogValues("EMG", emgValues);
				DebugLogValues("ECG", ecgValues);
				DebugLogValues("SCL", sclValues);
				EMGUpdated.Invoke(emgValues);
				ECGUpdated.Invoke(ecgValues);
				SCLUpdated.Invoke(sclValues);
			}
		}

		void DebugLogValues(string prefix, TimeStampedValue<float>[] values)
		{
			foreach (TimeStampedValue<float> val in values) {
				Debug.Log(prefix + " " +
					HiResTiming.getDateTimeForTimestamp(val.TimeStamp).ToString("o")
					+ " " + val.Value);
			}
		}

		void OnDestroy()
		{
			if (ehReader != null) {
				ehReader.StopReadingData();
				StatusChanged.Invoke("Stopped Reading Port " + ehReader.PortName);
			}
		}
	}
}
