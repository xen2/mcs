//
// System.Security.Cryptography SHA256Managed Class implementation
//
// Author:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//
// (C) 2001 
// (C) 2004 Novell (http://www.novell.com)
//


using System.Security.Cryptography;

namespace System.Security.Cryptography {
	
	/// <summary>
	/// C# implementation of the SHA1 cryptographic hash function.
	/// LAMESPEC?: Basically the same thing as SHA1Managed except for how its implemented.
	/// </summary>
	public class SHA256Managed : SHA256 {
		private const int BLOCK_SIZE_BYTES =  64;
		private const int HASH_SIZE_BYTES  =  32;
		private const int HASH_SIZE_BITS   = 256;
		[CLSCompliant(false)] private uint[] _H;
		[CLSCompliant(false)] private uint[] K;
		[CLSCompliant(false)] private uint count;
		private byte[] _ProcessingBuffer;   // Used to start data when passed less than a block worth.
		private int _ProcessingBufferCount; // Counts how much data we have stored that still needs processed.
	
		/// <summary>
		/// Creates a new SHA256Managed class.
		/// </summary>
		public SHA256Managed () 
		{
			_H = new uint[8];
			HashSizeValue = HASH_SIZE_BITS;
			_ProcessingBuffer = new byte[BLOCK_SIZE_BYTES];
			Initialize();
		}


		/// <summary>
		/// Internal function handling a subset of the algorithm.
		/// </summary>
		private uint Ch (uint u, uint v, uint w) 
		{
			return (u&v) ^ (~u&w);
		}

		/// <summary>
		/// Internal function handling a subset of the algorithm.
		/// </summary>
		private uint Maj (uint u, uint v, uint w) 
		{
			return (u&v) ^ (u&w) ^ (v&w);
		}

		/// <summary>
		/// Internal function handling a subset of the algorithm.
		/// </summary>
		private uint Ro0 (uint x) 
		{
			return ((x >> 7) | (x << 25))
				^ ((x >> 18) | (x << 14))
				^ (x >> 3);
		}

		/// <summary>
		/// Internal function handling a subset of the algorithm.
		/// </summary>
		private uint Ro1 (uint x) 
		{
			return ((x >> 17) | (x << 15))
				^ ((x >> 19) | (x << 13))
				^ (x >> 10);
		}

		/// <summary>
		/// Internal function handling a subset of the algorithm.
		/// </summary>
		private uint Sig0 (uint x) 
		{
			return ((x >> 2) | (x << 30))
				^ ((x >> 13) | (x << 19))
				^ ((x >> 22) | (x << 10));
		}

		/// <summary>
		/// Internal function handling a subset of the algorithm.
		/// </summary>
		private uint Sig1 (uint x) 
		{
			return ((x >> 6) | (x << 26))
				^ ((x >> 11) | (x << 21))
				^ ((x >> 25) | (x << 7));
		}

		/// <summary>
		/// Drives the hashing function.
		/// </summary>
		/// <param name="rgb">Byte array containing the data to hash.</param>
		/// <param name="start">Where in the input buffer to start.</param>
		/// <param name="size">Size in bytes of the data in the buffer to hash.</param>
		protected override void HashCore (byte[] rgb, int start, int size) 
		{
			int i;
			State = 1;

			if (_ProcessingBufferCount != 0) {
				if (size < (BLOCK_SIZE_BYTES - _ProcessingBufferCount)) {
					System.Buffer.BlockCopy (rgb, start, _ProcessingBuffer, _ProcessingBufferCount, size);
					_ProcessingBufferCount += size;
					return;
				}
				else {
					i = (BLOCK_SIZE_BYTES - _ProcessingBufferCount);
					System.Buffer.BlockCopy (rgb, start, _ProcessingBuffer, _ProcessingBufferCount, i);
					ProcessBlock (_ProcessingBuffer, 0);
					_ProcessingBufferCount = 0;
					start += i;
					size -= i;
				}
			}

			for (i=0; i<size-size%BLOCK_SIZE_BYTES; i += BLOCK_SIZE_BYTES) {
				ProcessBlock (rgb, start+i);
			}

			if (size%BLOCK_SIZE_BYTES != 0) {
				System.Buffer.BlockCopy (rgb, size-size%BLOCK_SIZE_BYTES+start, _ProcessingBuffer, 0, size%BLOCK_SIZE_BYTES);
				_ProcessingBufferCount = size%BLOCK_SIZE_BYTES;
			}
		}
	
		/// <summary>
		/// This finalizes the hash.  Takes the data from the chaining variables and returns it.
		/// </summary>
		protected override byte[] HashFinal () 
		{
			byte[] hash = new byte[32];
			int i, j;

			ProcessFinalBlock(_ProcessingBuffer, 0, _ProcessingBufferCount);

			for (i=0; i<8; i++) {
				for (j=0; j<4; j++) {
					hash[i*4+j] = (byte)(_H[i] >> (24-j*8));
				}
			}

			State = 0;
			return hash;
		}

		/// <summary>
		/// Resets the class after use.  Called automatically after hashing is done.
		/// </summary>
		public override void Initialize () 
		{
			count = 0;
			_ProcessingBufferCount = 0;
		
			_H[0] = 0x6A09E667;
			_H[1] = 0xBB67AE85;
			_H[2] = 0x3C6EF372;
			_H[3] = 0xA54FF53A;
			_H[4] = 0x510E527F;
			_H[5] = 0x9B05688C;
			_H[6] = 0x1F83D9AB;
			_H[7] = 0x5BE0CD19;
		}

		/// <summary>
		/// This is the meat of the hash function.  It is what processes each block one at a time.
		/// </summary>
		/// <param name="inputBuffer">Byte array to process data from.</param>
		/// <param name="inputOffset">Where in the byte array to start processing.</param>
		private void ProcessBlock (byte[] inputBuffer, int inputOffset) 
		{
			uint a, b, c, d, e, f, g, h;
			uint t1, t2;
			int i;
			uint[] buff;
		
			count += BLOCK_SIZE_BYTES;
		
			buff = new uint[64];

			for (i=0; i<16; i++) {
				buff[i] = ((uint)(inputBuffer[inputOffset+4*i]) << 24)
					| ((uint)(inputBuffer[inputOffset+4*i+1]) << 16)
					| ((uint)(inputBuffer[inputOffset+4*i+2]) <<  8)
					| ((uint)(inputBuffer[inputOffset+4*i+3]));
			}

		
			for (i=16; i<64; i++) {
				buff[i] = Ro1(buff[i-2]) + buff[i-7] + Ro0(buff[i-15]) + buff[i-16];
			}

			a = _H[0];
			b = _H[1];
			c = _H[2];
			d = _H[3];
			e = _H[4];
			f = _H[5];
			g = _H[6];
			h = _H[7];

			for (i=0; i<64; i++) {
				t1 = h + Sig1(e) + Ch(e,f,g) + SHAConstants.K1 [i] + buff[i];
				t2 = Sig0(a) + Maj(a,b,c);
				h = g;
				g = f;
				f = e;
				e = d + t1;
				d = c;
				c = b;
				b = a;
				a = t1 + t2;
			}

			_H[0] += a;
			_H[1] += b;
			_H[2] += c;
			_H[3] += d;
			_H[4] += e;
			_H[5] += f;
			_H[6] += g;
			_H[7] += h;
		}
	
		/// <summary>
		/// Pads and then processes the final block.
		/// Non-standard.
		/// </summary>
		/// <param name="inputBuffer">Buffer to grab data from.</param>
		/// <param name="inputOffset">Position in buffer in bytes to get data from.</param>
		/// <param name="inputCount">How much data in bytes in the buffer to use.</param>
		private void ProcessFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount) 
		{
			byte[] fooBuffer;
			int paddingSize;
			int i;
			uint size;

			paddingSize = (int)(56 - (inputCount + count) % BLOCK_SIZE_BYTES);

			if (paddingSize < 1)
				paddingSize += BLOCK_SIZE_BYTES;

			fooBuffer = new byte[inputCount+paddingSize+8];

			for (i=0; i<inputCount; i++) {
				fooBuffer[i] = inputBuffer[i+inputOffset];
			}

			fooBuffer[inputCount] = 0x80;
			for (i=inputCount+1; i<inputCount+paddingSize; i++) {
				fooBuffer[i] = 0x00;
			}

			size = (uint)(count+inputCount);
			size *= 8;

			fooBuffer[inputCount+paddingSize]   = 0x00;
			fooBuffer[inputCount+paddingSize+1] = 0x00;
			fooBuffer[inputCount+paddingSize+2] = 0x00;
			fooBuffer[inputCount+paddingSize+3] = 0x00;

			fooBuffer[inputCount+paddingSize+4] = (byte)((size) >> 24);
			fooBuffer[inputCount+paddingSize+5] = (byte)((size) >> 16);
			fooBuffer[inputCount+paddingSize+6] = (byte)((size) >>  8);
			fooBuffer[inputCount+paddingSize+7] = (byte)((size) >>  0);

			ProcessBlock(fooBuffer, 0);

			if (inputCount+paddingSize+8 == 128) {
				ProcessBlock(fooBuffer, 64);
			}
		}
	}
}

