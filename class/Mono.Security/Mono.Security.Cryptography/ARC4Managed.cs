//
// ARC4Managed.cs: Alleged RC4(tm) compatible symmetric stream cipher
//	RC4 is a trademark of RSA Security
//

using System;
using System.Globalization;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography {

	// References:
	// a.	Usenet 1994 - RC4 Algorithm revealed
	//	http://www.qrst.de/html/dsds/rc4.htm

	public class ARC4Managed : RC4, ICryptoTransform {

		private byte[] key;
		private byte[] state;
		private byte x;
		private byte y;
		private bool m_disposed;

		public ARC4Managed () : base () 
		{
			state = new byte [256];
			m_disposed = false;
		}

		~ARC4Managed () 
		{
			Dispose (true);
		}
	        
		protected override void Dispose (bool disposing) 
		{
			if (!m_disposed) {
				x = 0;
				y = 0;
				if (key != null) {
					Array.Clear (key, 0, key.Length);
					key = null;
				}
				Array.Clear (state, 0, state.Length);
				state = null;
				GC.SuppressFinalize (this);
				m_disposed = true;
			}
		}

		public override byte[] Key {
			get { return (byte[]) key.Clone (); }
			set { 
				key = (byte[]) value.Clone ();
				KeySetup (key);
			}
		}

		public bool CanReuseTransform {
			get { return false; }
		}

		public override ICryptoTransform CreateEncryptor (byte[] rgbKey, byte[] rgvIV)
		{
			Key = rgbKey;
			return (ICryptoTransform) this;
		}

		public override ICryptoTransform CreateDecryptor (byte[] rgbKey, byte[] rgvIV) 
		{
			Key = rgbKey;
			return CreateEncryptor ();
		}

		public override void GenerateIV () 
		{
			// not used for a stream cipher
			IV = new byte [0];
		}

		public override void GenerateKey () 
		{
			Key = KeyBuilder.Key (KeySizeValue >> 3);
		}

		public bool CanTransformMultipleBlocks {
			get { return true; }
		}

		public int InputBlockSize {
			get { return 1; }
		}

		public int OutputBlockSize {
			get { return 1; }
		}

		private void KeySetup (byte[] key) 
		{
			byte index1 = 0;
			byte index2 = 0;

			for (int counter = 0; counter < 256; counter++)
				state [counter] = (byte) counter;    
			x = 0;
			y = 0;
			for (int counter = 0; counter < 256; counter++) {
				index2 = (byte) (key [index1] + state [counter] + index2);
				// swap byte
				byte tmp = state [counter];
				state [counter] = state [index2];
				state [index2] = tmp;
				index1 = (byte) ((index1 + 1) % key.Length);
			}
		}

		private void CheckInput (byte[] inputBuffer, int inputOffset, int inputCount)
		{
			if (inputBuffer == null)
				throw new ArgumentNullException ("inputBuffer");
			if (inputOffset < 0)
				throw new ArgumentOutOfRangeException ("inputOffset", "< 0");
			if (inputCount < 0)
				throw new ArgumentOutOfRangeException ("inputCount", "< 0");
			// ordered to avoid possible integer overflow
			if (inputOffset > inputBuffer.Length - inputCount)
				throw new ArgumentException ("inputBuffer", Locale.GetText ("Overflow"));
		}

		public int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) 
		{
			CheckInput (inputBuffer, inputOffset, inputCount);
			// check output parameters
			if (outputBuffer == null)
				throw new ArgumentNullException ("outputBuffer");
			if (outputOffset < 0)
				throw new ArgumentOutOfRangeException ("outputOffset", "< 0");
			// ordered to avoid possible integer overflow
			if (outputOffset > outputBuffer.Length - inputCount)
				throw new ArgumentException ("outputBuffer", Locale.GetText ("Overflow"));

			return InternalTransformBlock (inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
		}

		private int InternalTransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) 
		{
			byte xorIndex;
			for (int counter = 0; counter < inputCount; counter ++) {               
				x = (byte) (x + 1);
				y = (byte) (state [x] + y);
				// swap byte
				byte tmp = state [x];
				state [x] = state [y];
				state [y] = tmp;

				xorIndex = (byte) (state [x] + state [y]);
				outputBuffer [outputOffset + counter] = (byte) (inputBuffer [inputOffset + counter] ^ state [xorIndex]);
			}
			return inputCount;
		}

		public byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount) 
		{
			CheckInput (inputBuffer, inputOffset, inputCount);

			byte[] output = new byte [inputCount];
			InternalTransformBlock (inputBuffer, inputOffset, inputCount, output, 0);
			return output;
		}
	}
}
