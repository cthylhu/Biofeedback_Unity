using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

/// <summary>
/// EHealth arduino. Communicates with the hardware directly using the SerialPort C# library.
/// 
/// Exposed values: TODO
/// 
/// Also TODO: remove redundancies in calculation with the IOM code!
/// </summary>
public class EHealthArduino
{
	SerialPort stream;
	private Thread readUpdateThread;
	private bool threadRunning = false;
	public double ECGVoltage;
	public double BreVoltage;
	public double GSRread;
	public  int HRBeatCounter = 0, BreathingCounter_air = 0, AirCounter = 0, ECGCounter = 0, HrvCounter = 0;
	//	HR peak parameters
	public double peakAG = 0;
	public double sample = 0, sample0 = 0;
	public double _attack = 0.9875;
	public double _decay = 0.992;
	public double gain;
	public double sampleAG;
	public double lower_bound = 0.9975;
	public double upper_bound = 0.99;
	public int near_peak = 0;
	//	Breathing peak parameters
	public double peakAGAir = 0;
	public double sampleAir = 0, sampleAir0 = 0;
	public double _attackAir = 0.9875;
	public double _decayAir = 0.992;
	public double gainAir;
	public double sampleAGAir;
	public double lower_boundAir = 0.9975;
	public double upper_boundAir = 0.99;
	public int near_peakAir = 0;
	public int beats = 0, AvgNum = 0;
	public long tc, interval, interval_Air;
	public double timeSum, HrBeat, timeSum_Air, BreathingBeat, Hrv, PulseOxHR;
	public Stopwatch sw, sw_Air;
	public double[] timeBuffer = new double[10];
	public double[] HrvBuffer = new double[10]; // 
	public double[] timeBuffer_Air = new double[5];
	public double MaxValueECG = 0, MaxValueBre = 0;
	public int MaxCounterECG = 0, PeakFlagECG = 0, MaxCounterBre = 0, PeakFlagBre = 0;
	public double[] MaxBufferECG = new double[400];
	public double[] AirBuffer = new double[50];
	public double[] ECGBuffer = new double[50];
	public double[] MaxBufferBre = new double[400];
	public double  SumFlag = 1, SumFlag_air = 1, SumHRVflag = 1;
	public DateTime start;
	public TimeSpan timeDiff;

	public void setup (string portName)
	{
		// Set the port and the baud rate (9600, is standard on most devices)
		stream = new SerialPort (portName, 115200);
		sw = Stopwatch.StartNew ();
		sw_Air = Stopwatch.StartNew ();
	}

	public bool startReadingData ()
	{
		if (readUpdateThread != null) { // should not happen, just return alive status
			return readUpdateThread.IsAlive;
		}
		readUpdateThread = new Thread (this.readAndUpdateData);
		//readUpdateThread.IsBackground = true; // not used as it potentially hides problems
		readUpdateThread.Start ();
		return true;
	}
	
	public bool isReadingData ()
	{
		if (readUpdateThread != null) {
			return readUpdateThread.IsAlive;
		}
		return false;
	}
	
	public void stopReadingData ()
	{
		if (readUpdateThread != null) {
			this.threadRunning = false; // not using Abort as it's unreliable and messes with unity
			readUpdateThread.Join ();
			readUpdateThread = null;
		}
	}
	
	private void readAndUpdateData ()
	{
		this.threadRunning = true; // signal that we are running
		stream.Open (); //Open the Serial Stream.
		sw = new Stopwatch ();
		sw_Air = new Stopwatch ();
		sw.Start ();
		sw_Air.Start ();

		while (this.threadRunning) {
			Thread.Sleep (1); // always briefly give up control every loop
			string value = stream.ReadLine (); 
			string[] vec2 = value.Split (',');
			string firstchar = value.Substring (0, 1);
			if (firstchar.Equals ("B")) {
				ECGVoltage = double.Parse (vec2 [1]);     //double.Parse(value.Substring(1));
				BreVoltage = double.Parse (vec2 [2]);
				GSRread = double.Parse (vec2 [3]);
				PulseOxHR = double.Parse (vec2 [4]);           
				//Get the ECG max
				MaxBufferECG [MaxCounterECG] = ECGVoltage;
				MaxValueECG = MaxBufferECG.Max ();
				MaxCounterECG ++;
				if (MaxCounterECG == 400)
					MaxCounterECG = 0;
				if (ECGVoltage > 0.75 * MaxValueECG)
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
				
			}
			// calculate the ECG peak
			sample0 = ECGVoltage;
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
			if ((near_peak == 1) && (sampleAG < upper_bound) && (PeakFlagECG == 1)) {
				near_peak = 0;
				sw.Stop ();
				interval = sw.ElapsedMilliseconds;
				//tc = sw.Elapsed;
				//HRBeatCounterPre = HRBeatCounter;
				if ((sw.ElapsedMilliseconds < 1500) && (sw.ElapsedMilliseconds > 500)) {
					timeBuffer [HRBeatCounter] = Convert.ToDouble (interval) / 1000; 
					HRBeatCounter++;
					if (HRBeatCounter == 10) {
						HRBeatCounter = 0;						
					}
					
				}
				for (int i = 0; i < timeBuffer.Length; i++) {
					//timeSum = timeSum + timeBuffer[i];
					SumFlag = timeBuffer [i] * SumFlag;	
					SumHRVflag = HrvBuffer [i] * SumHRVflag;	
				}
				if (SumFlag != 0) {
					//HRBeatCounter = 0;
					for (int i = 0; i < 10; i++) {
						timeSum = timeSum + timeBuffer [i];
						
					}
					if (timeSum != 0) {
						HrBeat = 10 / timeSum * 60;     
						HrvBuffer [HrvCounter] = (double)HrBeat;
						HrvCounter++;
						if (HrvCounter == 10)
							HrvCounter = 0;
						if (SumHRVflag != 0) {
							double average = HrvBuffer.Average ();
							double sumOfSquaresOfDifferences = HrvBuffer.Select (val => (val - average) * (val - average)).Sum ();
							double sd = Math.Sqrt (sumOfSquaresOfDifferences / HrvBuffer.Length);
							Hrv = sd;
						}
						
					}
					timeSum = 0;				
				}
				SumFlag = 1;
				SumHRVflag = 1;
				//if (ECGCounter)
				sw.Reset ();
				sw.Start ();
			}
			
			// calculate the Breathing peak
			sampleAir0 = BreVoltage * 5;
			AirBuffer [AirCounter] = sampleAir0;
			AirCounter ++;
			if (AirCounter == 50)
				AirCounter = 0;
			sampleAir = AirBuffer.Average ();
			if (sampleAir > peakAGAir)
				peakAGAir = _attackAir * sampleAir;
			else
				peakAGAir = _decayAir * peakAGAir;
			
			gainAir = _attackAir / peakAGAir;
			sampleAGAir = gainAir * sampleAir;
			if (sampleAGAir >= lower_boundAir)
				near_peakAir = 1;			
			if ((near_peakAir == 1) && (sampleAGAir < upper_boundAir)) {//&&(PeakFlagBre == 1)
				near_peakAir = 0;
				//				timeDiff = DateTime.Now - start;
				sw_Air.Stop ();
				interval_Air = sw_Air.ElapsedMilliseconds;//(long)timeDiff.TotalMilliseconds
				//tc = sw.Elapsed;
				//HRBeatCounterPre = HRBeatCounter;
				if (interval_Air > 1000) {
					timeBuffer_Air [BreathingCounter_air] = Convert.ToDouble (interval_Air) / 1000; 
					BreathingCounter_air++;
					if (BreathingCounter_air == 5) {
						BreathingCounter_air = 0;						
					}
					
				}
				for (int i = 0; i < timeBuffer_Air.Length; i++) {
					//timeSum = timeSum + timeBuffer[i];
					SumFlag_air = timeBuffer_Air [i] * SumFlag_air;	
				}
				if (SumFlag_air != 0) {
					//HRBeatCounter = 0;
					for (int i = 0; i < 5; i++) {
						timeSum_Air = timeSum_Air + timeBuffer_Air [i];
						
					}
					if (timeSum_Air != 0) {
						BreathingBeat = 5 / timeSum_Air * 60;                
					}
					timeSum_Air = 0;				
				}
				SumFlag_air = 1;	
				sw_Air.Reset ();
				sw_Air.Start ();
			}
			start = DateTime.Now;
			
			stream.BaseStream.Flush ();
		}
		
	}

};
