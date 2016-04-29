using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace PhySigTK
{
	/// <summary>
	/// EHealth Reader. Reads from Arduino hardware directly using the SerialPort C# library.
	/// Reading is done in a thread and values are kept with TimeStamps in a FIFO queue
	/// with a maximum length. Calculations are done lazily on access if necessary.
	/// </summary>
	public class EHealthReader : IDisposable
	{
		private static ICollection<string> UsedPorts = new List<string>();

		private SerialPort Stream = new SerialPort();
		public string PortName {
			get { return Stream.PortName; }
		}
		private Thread ReadingThread;
		private bool ThreadRunning = false;
		private LockedQueue<TimeStampedValue<string>> SerialLineQueue = new LockedQueue<TimeStampedValue<string>>();

		public void Setup(string portHint = "")
		{
			string[] availablePorts = SerialPort.GetPortNames();
			if (availablePorts.Length < 1) {
				throw new PhySigTKException("EHealthReader: No serial ports found");
			}
			IEnumerable<string> portsLeft = availablePorts.Except(UsedPorts);
			string portName = portHint;
			if (! portsLeft.Contains(portName)) {
				portName = portsLeft.FirstOrDefault();
			}
			if (portName == null) {
				throw new PhySigTKException("EHealthReader: No unused ports left (using "
					+ string.Join(", ", UsedPorts.ToArray()) + ")");
			}
			UsedPorts.Add(portName);
			// Set the port and the baud rate (9600, is standard on most devices)
			Stream.PortName = portName;
			Stream.BaudRate = 115200;
			Stream.ReadTimeout = 2000;
		}

		public bool StartReadingData()
		{
			if (ReadingThread != null) { // should not happen, just return alive status
				return ReadingThread.IsAlive;
			}
			ReadingThread = new Thread(this.ReadAndUpdateData);
			//ReadingThread.IsBackground = true; // not used as it potentially hides problems
			ReadingThread.Start();
			return true;
		}

		public bool IsReadingData()
		{
			if (ReadingThread != null) {
				return ReadingThread.IsAlive;
			}
			return false;
		}

		public void StopReadingData()
		{
			if (ReadingThread != null) {
				this.ThreadRunning = false; // not using Abort as it's unreliable and messes with unity
				ReadingThread.Join();
				ReadingThread = null;
				UsedPorts.Remove(Stream.PortName);
			}
		}

		public void Dispose()
		{
			StopReadingData();
		}

		private void ReadAndUpdateData()
		{
			this.ThreadRunning = true; // signal that we are running
			Stream.Open();
			while (this.ThreadRunning) {
				Thread.Sleep(1); // always briefly give up control every loop
				try {
					string value = Stream.ReadLine();
					SerialLineQueue.Enqueue(new TimeStampedValue<string>(
						HiResTiming.CurrentTimeStamp, value));
				} catch (TimeoutException) { }
			}
			Stream.Close();
		}

		public TimeStampedValue<string>[] RetrieveData()
		{
			return SerialLineQueue.DequeueAll();
		}
	}
}
