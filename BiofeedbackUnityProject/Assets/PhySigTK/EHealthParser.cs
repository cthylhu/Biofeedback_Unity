using UnityEngine;
using System;

namespace PhySigTK
{
	/// <summary>
	/// Parses strings read from an ehealth arduino. A subset of the values read can be used.
	/// The expected format is: ECG0.1|SCL0.5|EMG100
	/// as provided by the arduino script eHealthArduino4PhySigTK.ino
	/// </summary>
	public class EHealthParser
	{
		private TimeStampedFloatList NewEMGValues;
		private TimeStampedFloatList NewSCLValues;
		private TimeStampedFloatList NewECGValues;

		private void InitLists()
		{
			if (NewECGValues == null) {
				NewECGValues = new TimeStampedFloatList();
			}
			if (NewEMGValues == null) {
				NewEMGValues = new TimeStampedFloatList();
			}
			if (NewSCLValues == null) {
				NewSCLValues = new TimeStampedFloatList();
			}
		}

		// format: ECG0.1|SCL0.5|EMG100
		public void ParseValues(TimeStampedValue<string>[] values)
		{
			InitLists();
			foreach (TimeStampedValue<string> val in values) {
				long timeStamp = val.TimeStamp;
				foreach (string datapoint in val.Value.Split('|')) {
					try {
						if (datapoint.StartsWith("EMG")) {
							int emgInt = int.Parse(datapoint.Substring(3));
							// should be an integer smaller than 1024, map to 1-5V float
							float emgFloat = 5.0f * (emgInt / 1024.0f);
							NewEMGValues.Add(new TimeStampedValue<float>(timeStamp, emgFloat));
						} else if (datapoint.StartsWith("ECG")) {
							float ecgFloat = float.Parse(datapoint.Substring(3));
							NewECGValues.Add(new TimeStampedValue<float>(timeStamp, ecgFloat));
						} else if (datapoint.StartsWith("SCL")) {
							float sclFloat = float.Parse(datapoint.Substring(3));
							NewSCLValues.Add(new TimeStampedValue<float>(timeStamp, sclFloat));
						} else {
							Debug.LogWarning("EHealthParser: unknown/truncated datapoint: " + datapoint);
						}
					} catch (FormatException) {
						Debug.LogWarning("EHealthParser: unparseable datapoint: " + datapoint);
					}
				}
				// TODO: ecg values need to be adapted, scr detection, smoothing...

				// interesting values:
				// peakstart, peakend detection for ECG, interval length for ECG based on peak detection, heart rate beat and heart rate variability based on that
				// SCL -> SCR responses on/off signal, SCL baseline change signal?
			}
		}

		public TimeStampedFloatList RetrieveNewEMGValues()
		{
			TimeStampedFloatList values = NewEMGValues;
			NewEMGValues = null;
			return values;
		}

		public TimeStampedFloatList RetrieveNewECGValues()
		{
			TimeStampedFloatList values = NewECGValues;
			NewECGValues = null;
			return values;
		}

		public TimeStampedFloatList RetrieveNewSCLValues()
		{
			TimeStampedFloatList values = NewSCLValues;
			NewSCLValues = null;
			return values;
		}
	}
}
