/* -*- Mode: Csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
//
//
// This class has several problems:
//
//   * No buffering, the specification requires that there is buffering, this
//     matters because a few methods expose strings and chars and the reading
//     is encoding sensitive.   This means that when we do a read of a byte
//     sequence that can not be turned into a full string by the current encoding
//     we should keep a buffer with this data, and read from it on the next
//     iteration.
//
//   * Calls to read_serial from the unmanaged C do not check for errors,
//     like EINTR, that should be retried
//
//   * Calls to the encoder that do not consume all bytes because of partial
//     reads 
//

#if NET_2_0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Runtime.InteropServices;

namespace System.IO.Ports
{
	public class SerialPort : Component
	{
		public const int InfiniteTimeout = -1;
		const int DefaultReadBufferSize = 4096;
		const int DefaultWriteBufferSize = 2048;
		const int DefaultBaudRate = 9600;
		const int DefaultDataBits = 8;
		const Parity DefaultParity = Parity.None;
		const StopBits DefaultStopBits = StopBits.One;

		bool is_open;
		int baud_rate;
		Parity parity;
		StopBits stop_bits;
		Handshake handshake;
		int data_bits;
		bool break_state = false;
		bool dtr_enable = false;
		bool rts_enable = false;
		ISerialStream stream;
		Encoding encoding = Encoding.ASCII;
		string new_line = Environment.NewLine;
		string port_name;
		int read_timeout = InfiniteTimeout;
		int write_timeout = InfiniteTimeout;
		int readBufferSize = DefaultReadBufferSize;
		int writeBufferSize = DefaultWriteBufferSize;
		object error_received = new object ();
		object data_received = new object ();
		object pin_changed = new object ();
		
		static string default_port_name = "ttyS0";

		public SerialPort () : 
			this (GetDefaultPortName (), DefaultBaudRate, DefaultParity, DefaultDataBits, DefaultStopBits)
		{
		}

		public SerialPort (IContainer container) : this ()
		{
			// TODO: What to do here?
		}

		/*
		  IContainer is in 2.0?
		  public SerialPort (IContainer container) {
		  }
		*/

		public SerialPort (string portName) :
			this (portName, DefaultBaudRate, DefaultParity, DefaultDataBits, DefaultStopBits)
		{
		}

		public SerialPort (string portName, int baudRate) :
			this (portName, baudRate, DefaultParity, DefaultDataBits, DefaultStopBits)
		{
		}

		public SerialPort (string portName, int baudRate, Parity parity) :
			this (portName, baudRate, parity, DefaultDataBits, DefaultStopBits)
		{
		}

		public SerialPort (string portName, int baudRate, Parity parity, int dataBits) :
			this (portName, baudRate, parity, dataBits, DefaultStopBits)
		{
		}

		public SerialPort (string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) 
		{
			port_name = portName;
			baud_rate = baudRate;
			data_bits = dataBits;
			stop_bits = stopBits;
			this.parity = parity;
		}

		static string GetDefaultPortName ()
		{
			return default_port_name;
		}

		public Stream BaseStream {
			get {
				if (!is_open)
					throw new InvalidOperationException ();

				return (Stream) stream;
			}
		}

		[DefaultValueAttribute (DefaultBaudRate)]
		public int BaudRate {
			get {
				return baud_rate;
			}
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");
				
				if (is_open)
					stream.SetAttributes (value, parity, data_bits, stop_bits, handshake);
				
				baud_rate = value;
			}
		}

		public bool BreakState {
			get {
				return break_state;
			}
			set {
				CheckOpen ();
				if (value == break_state)
					return; // Do nothing.

				stream.SetBreakState (value);
				break_state = value;
			}
		}

		public int BytesToRead {
			get {
				CheckOpen ();
				return stream.BytesToRead;
			}
		}

		public int BytesToWrite {
			get {
				CheckOpen ();
				return stream.BytesToWrite;
			}
		}

		public bool CDHolding {
			get {
				CheckOpen ();
				return (stream.GetSignals () & SerialSignal.Cd) != 0;
			}
		}

		public bool CtsHolding {
			get {
				CheckOpen ();
				return (stream.GetSignals () & SerialSignal.Cts) != 0;
			}
		}

		[DefaultValueAttribute(DefaultDataBits)]
		public int DataBits {
			get {
				return data_bits;
			}
			set {
				if (value < 5 || value > 8)
					throw new ArgumentOutOfRangeException ("value");

				if (is_open)
					stream.SetAttributes (baud_rate, parity, value, stop_bits, handshake);
				
				data_bits = value;
			}
		}

		[MonoTODO("Not implemented")]
		public bool DiscardNull {
			get {
				CheckOpen ();
				throw new NotImplementedException ();
			}
			set {
				CheckOpen ();
				throw new NotImplementedException ();
			}
		}

		public bool DsrHolding {
			get {
				CheckOpen ();
				return (stream.GetSignals () & SerialSignal.Dsr) != 0;
			}
		}

		[DefaultValueAttribute(false)]
		public bool DtrEnable {
			get {
				return dtr_enable;
			}
			set {
				if (value == dtr_enable)
					return;
				if (is_open)
					stream.SetSignal (SerialSignal.Dtr, value);
				
				dtr_enable = value;
			}
		}

		public Encoding Encoding {
			get {
				return encoding;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				encoding = value;
			}
		}

		[DefaultValueAttribute(Handshake.None)]
		public Handshake Handshake {
			get {
				return handshake;
			}
			set {
				if (value < Handshake.None || value > Handshake.RequestToSendXOnXOff)
					throw new ArgumentOutOfRangeException ("value");

				if (is_open)
					stream.SetAttributes (baud_rate, parity, data_bits, stop_bits, value);
				
				handshake = value;
			}
		}

		public bool IsOpen {
			get {
				return is_open;
			}
		}

		[DefaultValueAttribute("\n")]
		public string NewLine {
			get {
				return new_line;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				
				new_line = value;
			}
		}

		[DefaultValueAttribute(DefaultParity)]
		public Parity Parity {
			get {
				return parity;
			}
			set {
				if (value < Parity.None || value > Parity.Space)
					throw new ArgumentOutOfRangeException ("value");

				if (is_open)
					stream.SetAttributes (baud_rate, value, data_bits, stop_bits, handshake);
				
				parity = value;
			}
		}

		[MonoTODO("Not implemented")]
		public byte ParityReplace {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		
		public string PortName {
			get {
				return port_name;
			}
			set {
				if (is_open)
					throw new InvalidOperationException ("Port name cannot be set while port is open.");
				if (value == null)
					throw new ArgumentNullException ("value");
				if (value.Length == 0 || value.StartsWith ("\\\\"))
					throw new ArgumentException ("value");

				port_name = value;
			}
		}

		[DefaultValueAttribute(DefaultReadBufferSize)]
		public int ReadBufferSize {
			get {
				return readBufferSize;
			}
			set {
				if (is_open)
					throw new InvalidOperationException ();
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");
				if (value <= DefaultReadBufferSize)
					return;

				readBufferSize = value;
			}
		}

		[DefaultValueAttribute(InfiniteTimeout)]
		public int ReadTimeout {
			get {
				return read_timeout;
			}
			set {
				if (value <= 0 && value != InfiniteTimeout)
					throw new ArgumentOutOfRangeException ("value");

				if (is_open)
					stream.ReadTimeout = value;
				
				read_timeout = value;
			}
		}

		[MonoTODO("Not implemented")]
		[DefaultValueAttribute(1)]
		public int ReceivedBytesThreshold {
			get {
				throw new NotImplementedException ();
			}
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");

				throw new NotImplementedException ();
			}
		}

		[DefaultValueAttribute(false)]
		public bool RtsEnable {
			get {
				return rts_enable;
			}
			set {
				if (value == rts_enable)
					return;
				if (is_open)
					stream.SetSignal (SerialSignal.Rts, value);
				
				rts_enable = value;
			}
		}

		[DefaultValueAttribute(DefaultStopBits)]
		public StopBits StopBits {
			get {
				return stop_bits;
			}
			set {
				if (value < StopBits.One || value > StopBits.OnePointFive)
					throw new ArgumentOutOfRangeException ("value");
				
				if (is_open)
					stream.SetAttributes (baud_rate, parity, data_bits, value, handshake);
				
				stop_bits = value;
			}
		}

		[DefaultValueAttribute(DefaultWriteBufferSize)]
		public int WriteBufferSize {
			get {
				return writeBufferSize;
			}
			set {
				if (is_open)
					throw new InvalidOperationException ();
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");
				if (value <= DefaultWriteBufferSize)
					return;

				writeBufferSize = value;
			}
		}

		[DefaultValueAttribute(InfiniteTimeout)]
		public int WriteTimeout {
			get {
				return write_timeout;
			}
			set {
				if (value <= 0 && value != InfiniteTimeout)
					throw new ArgumentOutOfRangeException ("value");

				if (is_open)
					stream.WriteTimeout = value;
				
				write_timeout = value;
			}
		}

		// methods

		public void Close ()
		{
			Dispose (false);
		}

		protected override void Dispose (bool disposing)
		{
			if (!is_open)
				return;
			
			is_open = false;
			stream.Close ();
			stream = null;
		}

		public void DiscardInBuffer ()
		{
			CheckOpen ();
			stream.DiscardInBuffer ();
		}

		public void DiscardOutBuffer ()
		{
			CheckOpen ();
			stream.DiscardOutBuffer ();
		}

		public static string [] GetPortNames ()
		{
			int p = (int) Environment.OSVersion.Platform;
			
			// Are we on Unix?
			if (p == 4 || p == 128){
				string [] ttys = Directory.GetFiles ("/dev/", "tty*");
				List<string> serial_ports = new List<string> ();
				
				foreach (string dev in ttys){
					if (dev.StartsWith ("/dev/ttyS") || dev.StartsWith ("/dev/ttyUSB"))
						serial_ports.Add (dev);
						
				}
				return serial_ports.ToArray ();
			}
			throw new NotImplementedException ("Detection of ports is not implemented for this platform yet.");
		}

		static bool IsWindows {
			get {
				PlatformID id =  Environment.OSVersion.Platform;
				return id == PlatformID.Win32Windows || id == PlatformID.Win32NT; // WinCE not supported
			}
		}

		public void Open ()
		{
			if (is_open)
				throw new InvalidOperationException ("Port is already open");
			
#if !TARGET_JVM
			if (IsWindows) // Use windows kernel32 backend
				stream = new WinSerialStream (port_name, baud_rate, data_bits, parity, stop_bits,
						handshake, read_timeout, write_timeout, readBufferSize, writeBufferSize);
			else // Use standard unix backend
#endif
				stream = new SerialPortStream (port_name, baud_rate, data_bits, parity, stop_bits, dtr_enable,
					rts_enable, handshake, read_timeout, write_timeout, readBufferSize, writeBufferSize);
			
			is_open = true;
		}

		public int Read (byte[] buffer, int offset, int count)
		{
			CheckOpen ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || count < 0)
				throw new ArgumentOutOfRangeException ("offset or count less than zero.");

			if (buffer.Length - offset < count )
				throw new ArgumentException ("offset+count",
							      "The size of the buffer is less than offset + count.");
			
			return stream.Read (buffer, offset, count);
		}

		[Obsolete("Read of char buffers is currently broken")]
		[MonoTODO("This is broken")]
		public int Read (char[] buffer, int offset, int count)
		{
			CheckOpen ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || count < 0)
				throw new ArgumentOutOfRangeException ("offset or count less than zero.");

			if (buffer.Length - offset < count )
				throw new ArgumentException ("offset+count",
							      "The size of the buffer is less than offset + count.");

			// The following code does not work, we nee to reintroduce a buffer stream somewhere
			// for this to work;  In addition the code is broken.
			byte [] bytes = encoding.GetBytes (buffer, offset, count);
			return stream.Read (bytes, 0, bytes.Length);
		}

		internal int read_byte ()
		{
			byte [] buff = new byte [1];
			if (stream.Read (buff, 0, 1) > 0)
				return buff [0];

			return -1;
		}
		
		public int ReadByte ()
		{
			CheckOpen ();
			return read_byte ();
		}

		public int ReadChar ()
		{
			CheckOpen ();
			
			byte [] buffer = new byte [16];
			int i = 0;

			do {
				int b = read_byte ();
				if (b == -1)
					return -1;
				buffer [i++] = (byte) b;
				char [] c = encoding.GetChars (buffer, 0, 1);
				if (c.Length > 0)
					return (int) c [0];
			} while (i < buffer.Length);

			return -1;
		}

		public string ReadExisting ()
		{
			CheckOpen ();
			
			int count = BytesToRead;
			byte [] bytes = new byte [count];
			
			int n = stream.Read (bytes, 0, count);
			return new String (encoding.GetChars (bytes, 0, n));
		}

		public string ReadLine ()
		{
			CheckOpen ();
			List<byte> bytes_read = new List<byte>();
			byte [] buff = new byte [1];
			
			while (true){
				int n = stream.Read (buff, 0, 1);
				if (n == -1 || buff [0] == '\n')
					break;
				bytes_read.Add (buff [0]);
			} 
			return new String (encoding.GetChars (bytes_read.ToArray ()));
		}

		public string ReadTo (string value)
		{
			CheckOpen ();
			if (value == null)
				throw new ArgumentNullException ("value");
			if (value.Length == 0)
				throw new ArgumentException ("value");

			// Turn into byte array, so we can compare
			byte [] byte_value = encoding.GetBytes (value);
			int current = 0;
			List<byte> seen = new List<byte> ();

			while (true){
				int n = read_byte ();
				if (n == -1)
					break;
				seen.Add ((byte)n);
				if (n == byte_value [current]){
					current++;
					if (current == byte_value.Length)
						return encoding.GetString (seen.ToArray (), 0, seen.Count - byte_value.Length);
				} else {
					current = (byte_value [0] == n) ? 1 : 0;
				}
			}
			return encoding.GetString (seen.ToArray ());
		}

		public void Write (string str)
		{
			CheckOpen ();
			if (str == null)
				throw new ArgumentNullException ("str");
			
			byte [] buffer = encoding.GetBytes (str);
			Write (buffer, 0, buffer.Length);
		}

		public void Write (byte [] buffer, int offset, int count)
		{
			CheckOpen ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || count < 0)
				throw new ArgumentOutOfRangeException ();

			if (buffer.Length - offset < count)
				throw new ArgumentException ("offset+count",
							     "The size of the buffer is less than offset + count.");

			stream.Write (buffer, offset, count);
		}

		public void Write (char [] buffer, int offset, int count)
		{
			CheckOpen ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");
			if (count < 0 || count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
			if (count > buffer.Length - offset)
				throw new ArgumentException ("count > buffer.Length - offset");

			byte [] bytes = encoding.GetBytes (buffer, offset, count);
			stream.Write (bytes, 0, bytes.Length);
		}

		public void WriteLine (string str)
		{
			Write (str + new_line);
		}

		void CheckOpen ()
		{
			if (!is_open)
				throw new InvalidOperationException ("Specified port is not open.");
		}

		internal void OnErrorReceived (SerialErrorReceivedEventArgs args)
		{
			SerialErrorReceivedEventHandler handler =
				(SerialErrorReceivedEventHandler) Events [error_received];

			if (handler != null)
				handler (this, args);
		}

		internal void OnDataReceived (SerialDataReceivedEventArgs args)
		{
			SerialDataReceivedEventHandler handler =
				(SerialDataReceivedEventHandler) Events [data_received];

			if (handler != null)
				handler (this, args);
		}
		
		internal void OnDataReceived (SerialPinChangedEventArgs args)
		{
			SerialPinChangedEventHandler handler =
				(SerialPinChangedEventHandler) Events [pin_changed];

			if (handler != null)
				handler (this, args);
		}

		// events
		public event SerialErrorReceivedEventHandler ErrorReceived {
			add { Events.AddHandler (error_received, value); }
			remove { Events.RemoveHandler (error_received, value); }
		}
		
		public event SerialPinChangedEventHandler PinChanged {
			add { Events.AddHandler (pin_changed, value); }
			remove { Events.RemoveHandler (pin_changed, value); }
		}
		
		public event SerialDataReceivedEventHandler DataReceived {
			add { Events.AddHandler (data_received, value); }
			remove { Events.RemoveHandler (data_received, value); }
		}
	}

	public delegate void SerialDataReceivedEventHandler (object sender, SerialDataReceivedEventArgs e);
	public delegate void SerialPinChangedEventHandler (object sender, SerialPinChangedEventArgs e);
	public delegate void SerialErrorReceivedEventHandler (object sender, SerialErrorReceivedEventArgs e);

}

#endif
