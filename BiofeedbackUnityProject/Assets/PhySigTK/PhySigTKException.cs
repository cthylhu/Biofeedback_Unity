using System;
using System.Runtime.Serialization;

namespace PhySigTK
{
	[Serializable()]
	class PhySigTKException : Exception
	{
		public PhySigTKException() : base() { }
		public PhySigTKException(string message) : base(message) { }
		public PhySigTKException(string message, Exception inner) : base(message, inner) { }
		// A constructor is needed for serialization when an
		// exception propagates from a remoting server to the client. 
		protected PhySigTKException(SerializationInfo info, StreamingContext context) { }
	}
}
