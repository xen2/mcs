//
// System.Security.Cryptography.DESCryptoServiceProvider
//
// Authors:
//	Sergey Chaban (serge@wildwestsoftware.com)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if !NET_2_1 || MONOTOUCH

using System.Runtime.InteropServices;
using Mono.Security.Cryptography;

namespace System.Security.Cryptography {

	// References:
	// a.	FIPS PUB 46-3: Data Encryption Standard
	//	http://csrc.nist.gov/publications/fips/fips46-3/fips46-3.pdf
	
	internal class DESTransform : SymmetricTransform {
	
		internal static readonly int KEY_BIT_SIZE = 64;
		internal static readonly int KEY_BYTE_SIZE = KEY_BIT_SIZE / 8;
		internal static readonly int BLOCK_BIT_SIZE = 64;
		internal static readonly int BLOCK_BYTE_SIZE = BLOCK_BIT_SIZE / 8;
	
		private byte[] keySchedule;
		private byte[] byteBuff;
		private uint[] dwordBuff;
/*	
		// S-boxes from FIPS 46-3, Appendix 1, page 17
		private static readonly byte [] sBoxes = {
			14,  4, 13,  1,  2, 15, 11,  8,  3, 10,  6, 12,  5,  9,  0,  7,
			0, 15,  7,  4, 14,  2, 13,  1, 10,  6, 12, 11,  9,  5,  3,  8,
			4,  1, 14,  8, 13,  6,  2, 11, 15, 12,  9,  7,  3, 10,  5,  0,
			15, 12,  8,  2,  4,  9,  1,  7,  5, 11,  3, 14, 10,  0,  6, 13,
	
			15,  1,  8, 14,  6, 11,  3,  4,  9,  7,  2, 13, 12,  0,  5, 10,
			3, 13,  4,  7, 15,  2,  8, 14, 12,  0,  1, 10,  6,  9, 11,  5,
			0, 14,  7, 11, 10,  4, 13,  1,  5,  8, 12,  6,  9,  3,  2, 15,
			13,  8, 10,  1,  3, 15,  4,  2, 11,  6,  7, 12,  0,  5, 14,  9,
	
			10,  0,  9, 14,  6,  3, 15,  5,  1, 13, 12,  7, 11,  4,  2,  8,
			13,  7,  0,  9,  3,  4,  6, 10,  2,  8,  5, 14, 12, 11, 15,  1,
			13,  6,  4,  9,  8, 15,  3,  0, 11,  1,  2, 12,  5, 10, 14,  7,
			1, 10, 13,  0,  6,  9,  8,  7,  4, 15, 14,  3, 11,  5,  2, 12,
	
			7, 13, 14,  3,  0,  6,  9, 10,  1,  2,  8,  5, 11, 12,  4, 15,
			13,  8, 11,  5,  6, 15,  0,  3,  4,  7,  2, 12,  1, 10, 14,  9,
			10,  6,  9,  0, 12, 11,  7, 13, 15,  1,  3, 14,  5,  2,  8,  4,
			3, 15,  0,  6, 10,  1, 13,  8,  9,  4,  5, 11, 12,  7,  2, 14,
	
			2, 12,  4,  1,  7, 10, 11,  6,  8,  5,  3, 15, 13,  0, 14,  9,
			14, 11,  2, 12,  4,  7, 13,  1,  5,  0, 15, 10,  3,  9,  8,  6,
			4,  2,  1, 11, 10, 13,  7,  8, 15,  9, 12,  5,  6,  3,  0, 14,
			11,  8, 12,  7,  1, 14,  2, 13,  6, 15,  0,  9, 10,  4,  5,  3,
	
			12,  1, 10, 15,  9,  2,  6,  8,  0, 13,  3,  4, 14,  7,  5, 11,
			10, 15,  4,  2,  7, 12,  9,  5,  6,  1, 13, 14,  0, 11,  3,  8,
			9, 14, 15,  5,  2,  8, 12,  3,  7,  0,  4, 10,  1, 13, 11,  6,
			4,  3,  2, 12,  9,  5, 15, 10, 11, 14,  1,  7,  6,  0,  8, 13,
	
			4, 11,  2, 14, 15,  0,  8, 13,  3, 12,  9,  7,  5, 10,  6,  1,
			13,  0, 11,  7,  4,  9,  1, 10, 14,  3,  5, 12,  2, 15,  8,  6,
			1,  4, 11, 13, 12,  3,  7, 14, 10, 15,  6,  8,  0,  5,  9,  2,
			6, 11, 13,  8,  1,  4, 10,  7,  9,  5,  0, 15, 14,  2,  3, 12,
	
			13,  2,  8,  4,  6, 15, 11,  1, 10,  9,  3, 14,  5,  0, 12,  7,
			1, 15, 13,  8, 10,  3,  7,  4, 12,  5,  6, 11,  0, 14,  9,  2,
			7, 11,  4,  1,  9, 12, 14,  2,  0,  6, 10, 13, 15,  3,  5,  8,
			2,  1, 14,  7,  4, 10,  8, 13, 15, 12,  9,  0,  3,  5,  6, 11
		};
	
		// P table from page 15, also in Appendix 1, page 18
		private static readonly byte [] pTab = {	
			16-1,  7-1, 20-1, 21-1,
			29-1, 12-1, 28-1, 17-1,
			1-1, 15-1, 23-1, 26-1,
			5-1, 18-1, 31-1, 10-1,
			2-1,  8-1, 24-1, 14-1,
			32-1, 27-1,  3-1,  9-1,
			19-1, 13-1, 30-1,  6-1,
			22-1, 11-1,  4-1, 25-1
		};
*/	
		// pre-computed result of sBoxes (using pTab)
		private static readonly uint[] spBoxes = { 
			0x00808200, 0x00000000, 0x00008000, 0x00808202, 0x00808002, 0x00008202, 0x00000002, 0x00008000, 
			0x00000200, 0x00808200, 0x00808202, 0x00000200, 0x00800202, 0x00808002, 0x00800000, 0x00000002, 
			0x00000202, 0x00800200, 0x00800200, 0x00008200, 0x00008200, 0x00808000, 0x00808000, 0x00800202, 
			0x00008002, 0x00800002, 0x00800002, 0x00008002, 0x00000000, 0x00000202, 0x00008202, 0x00800000, 
			0x00008000, 0x00808202, 0x00000002, 0x00808000, 0x00808200, 0x00800000, 0x00800000, 0x00000200, 
			0x00808002, 0x00008000, 0x00008200, 0x00800002, 0x00000200, 0x00000002, 0x00800202, 0x00008202, 
			0x00808202, 0x00008002, 0x00808000, 0x00800202, 0x00800002, 0x00000202, 0x00008202, 0x00808200, 
			0x00000202, 0x00800200, 0x00800200, 0x00000000, 0x00008002, 0x00008200, 0x00000000, 0x00808002, 
			0x40084010, 0x40004000, 0x00004000, 0x00084010, 0x00080000, 0x00000010, 0x40080010, 0x40004010, 
			0x40000010, 0x40084010, 0x40084000, 0x40000000, 0x40004000, 0x00080000, 0x00000010, 0x40080010, 
			0x00084000, 0x00080010, 0x40004010, 0x00000000, 0x40000000, 0x00004000, 0x00084010, 0x40080000, 
			0x00080010, 0x40000010, 0x00000000, 0x00084000, 0x00004010, 0x40084000, 0x40080000, 0x00004010, 
			0x00000000, 0x00084010, 0x40080010, 0x00080000, 0x40004010, 0x40080000, 0x40084000, 0x00004000, 
			0x40080000, 0x40004000, 0x00000010, 0x40084010, 0x00084010, 0x00000010, 0x00004000, 0x40000000, 
			0x00004010, 0x40084000, 0x00080000, 0x40000010, 0x00080010, 0x40004010, 0x40000010, 0x00080010, 
			0x00084000, 0x00000000, 0x40004000, 0x00004010, 0x40000000, 0x40080010, 0x40084010, 0x00084000, 
			0x00000104, 0x04010100, 0x00000000, 0x04010004, 0x04000100, 0x00000000, 0x00010104, 0x04000100, 
			0x00010004, 0x04000004, 0x04000004, 0x00010000, 0x04010104, 0x00010004, 0x04010000, 0x00000104, 
			0x04000000, 0x00000004, 0x04010100, 0x00000100, 0x00010100, 0x04010000, 0x04010004, 0x00010104, 
			0x04000104, 0x00010100, 0x00010000, 0x04000104, 0x00000004, 0x04010104, 0x00000100, 0x04000000, 
			0x04010100, 0x04000000, 0x00010004, 0x00000104, 0x00010000, 0x04010100, 0x04000100, 0x00000000, 
			0x00000100, 0x00010004, 0x04010104, 0x04000100, 0x04000004, 0x00000100, 0x00000000, 0x04010004, 
			0x04000104, 0x00010000, 0x04000000, 0x04010104, 0x00000004, 0x00010104, 0x00010100, 0x04000004, 
			0x04010000, 0x04000104, 0x00000104, 0x04010000, 0x00010104, 0x00000004, 0x04010004, 0x00010100, 
			0x80401000, 0x80001040, 0x80001040, 0x00000040, 0x00401040, 0x80400040, 0x80400000, 0x80001000, 
			0x00000000, 0x00401000, 0x00401000, 0x80401040, 0x80000040, 0x00000000, 0x00400040, 0x80400000, 
			0x80000000, 0x00001000, 0x00400000, 0x80401000, 0x00000040, 0x00400000, 0x80001000, 0x00001040, 
			0x80400040, 0x80000000, 0x00001040, 0x00400040, 0x00001000, 0x00401040, 0x80401040, 0x80000040, 
			0x00400040, 0x80400000, 0x00401000, 0x80401040, 0x80000040, 0x00000000, 0x00000000, 0x00401000, 
			0x00001040, 0x00400040, 0x80400040, 0x80000000, 0x80401000, 0x80001040, 0x80001040, 0x00000040, 
			0x80401040, 0x80000040, 0x80000000, 0x00001000, 0x80400000, 0x80001000, 0x00401040, 0x80400040, 
			0x80001000, 0x00001040, 0x00400000, 0x80401000, 0x00000040, 0x00400000, 0x00001000, 0x00401040, 
			0x00000080, 0x01040080, 0x01040000, 0x21000080, 0x00040000, 0x00000080, 0x20000000, 0x01040000, 
			0x20040080, 0x00040000, 0x01000080, 0x20040080, 0x21000080, 0x21040000, 0x00040080, 0x20000000, 
			0x01000000, 0x20040000, 0x20040000, 0x00000000, 0x20000080, 0x21040080, 0x21040080, 0x01000080, 
			0x21040000, 0x20000080, 0x00000000, 0x21000000, 0x01040080, 0x01000000, 0x21000000, 0x00040080, 
			0x00040000, 0x21000080, 0x00000080, 0x01000000, 0x20000000, 0x01040000, 0x21000080, 0x20040080, 
			0x01000080, 0x20000000, 0x21040000, 0x01040080, 0x20040080, 0x00000080, 0x01000000, 0x21040000, 
			0x21040080, 0x00040080, 0x21000000, 0x21040080, 0x01040000, 0x00000000, 0x20040000, 0x21000000, 
			0x00040080, 0x01000080, 0x20000080, 0x00040000, 0x00000000, 0x20040000, 0x01040080, 0x20000080, 
			0x10000008, 0x10200000, 0x00002000, 0x10202008, 0x10200000, 0x00000008, 0x10202008, 0x00200000, 
			0x10002000, 0x00202008, 0x00200000, 0x10000008, 0x00200008, 0x10002000, 0x10000000, 0x00002008, 
			0x00000000, 0x00200008, 0x10002008, 0x00002000, 0x00202000, 0x10002008, 0x00000008, 0x10200008, 
			0x10200008, 0x00000000, 0x00202008, 0x10202000, 0x00002008, 0x00202000, 0x10202000, 0x10000000, 
			0x10002000, 0x00000008, 0x10200008, 0x00202000, 0x10202008, 0x00200000, 0x00002008, 0x10000008, 
			0x00200000, 0x10002000, 0x10000000, 0x00002008, 0x10000008, 0x10202008, 0x00202000, 0x10200000, 
			0x00202008, 0x10202000, 0x00000000, 0x10200008, 0x00000008, 0x00002000, 0x10200000, 0x00202008, 
			0x00002000, 0x00200008, 0x10002008, 0x00000000, 0x10202000, 0x10000000, 0x00200008, 0x10002008, 
			0x00100000, 0x02100001, 0x02000401, 0x00000000, 0x00000400, 0x02000401, 0x00100401, 0x02100400, 
			0x02100401, 0x00100000, 0x00000000, 0x02000001, 0x00000001, 0x02000000, 0x02100001, 0x00000401, 
			0x02000400, 0x00100401, 0x00100001, 0x02000400, 0x02000001, 0x02100000, 0x02100400, 0x00100001, 
			0x02100000, 0x00000400, 0x00000401, 0x02100401, 0x00100400, 0x00000001, 0x02000000, 0x00100400, 
			0x02000000, 0x00100400, 0x00100000, 0x02000401, 0x02000401, 0x02100001, 0x02100001, 0x00000001, 
			0x00100001, 0x02000000, 0x02000400, 0x00100000, 0x02100400, 0x00000401, 0x00100401, 0x02100400, 
			0x00000401, 0x02000001, 0x02100401, 0x02100000, 0x00100400, 0x00000000, 0x00000001, 0x02100401, 
			0x00000000, 0x00100401, 0x02100000, 0x00000400, 0x02000001, 0x02000400, 0x00000400, 0x00100001, 
			0x08000820, 0x00000800, 0x00020000, 0x08020820, 0x08000000, 0x08000820, 0x00000020, 0x08000000, 
			0x00020020, 0x08020000, 0x08020820, 0x00020800, 0x08020800, 0x00020820, 0x00000800, 0x00000020, 
			0x08020000, 0x08000020, 0x08000800, 0x00000820, 0x00020800, 0x00020020, 0x08020020, 0x08020800, 
			0x00000820, 0x00000000, 0x00000000, 0x08020020, 0x08000020, 0x08000800, 0x00020820, 0x00020000, 
			0x00020820, 0x00020000, 0x08020800, 0x00000800, 0x00000020, 0x08020020, 0x00000800, 0x00020820, 
			0x08000800, 0x00000020, 0x08000020, 0x08020000, 0x08020020, 0x08000000, 0x00020000, 0x08000820, 
			0x00000000, 0x08020820, 0x00020020, 0x08000020, 0x08020000, 0x08000800, 0x08000820, 0x00000000, 
			0x08020820, 0x00020800, 0x00020800, 0x00000820, 0x00000820, 0x00020020, 0x08000000, 0x08020800
		};
	
		// Permuted choice 1 table, PC-1, page 19
		// Translated to zero-based format.
		private static readonly byte [] PC1 = {
			57-1, 49-1, 41-1, 33-1, 25-1, 17-1,  9-1,
			1-1, 58-1, 50-1, 42-1, 34-1, 26-1, 18-1,
			10-1,  2-1, 59-1, 51-1, 43-1, 35-1, 27-1,
			19-1, 11-1,  3-1, 60-1, 52-1, 44-1, 36-1,
	
			63-1, 55-1, 47-1, 39-1, 31-1, 23-1, 15-1,
			7-1, 62-1, 54-1, 46-1, 38-1, 30-1, 22-1,
			14-1,  6-1, 61-1, 53-1, 45-1, 37-1, 29-1,
			21-1, 13-1,  5-1, 28-1, 20-1, 12-1,  4-1
		};
	
/*
		private static readonly byte [] leftRot = {
			1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1
		};
*/
		// pre-computed result of leftRot
		private static readonly byte[] leftRotTotal = { 0x01, 0x02, 0x04, 0x06, 0x08, 0x0A, 0x0C, 0x0E, 0x0F, 0x11, 0x13, 0x15, 0x17, 0x19, 0x1B, 0x1C };
	
		// Permuted choice 2 table, PC-2, page 21
		// Translated to zero-based format.
		private static readonly byte [] PC2 = {
			14-1, 17-1, 11-1, 24-1,  1-1,  5-1,
			3-1, 28-1, 15-1,  6-1, 21-1, 10-1,
			23-1, 19-1, 12-1,  4-1, 26-1,  8-1,
			16-1,  7-1, 27-1, 20-1, 13-1,  2-1,
			41-1, 52-1, 31-1, 37-1, 47-1, 55-1,
			30-1, 40-1, 51-1, 45-1, 33-1, 48-1,
			44-1, 49-1, 39-1, 56-1, 34-1, 53-1,
			46-1, 42-1, 50-1, 36-1, 29-1, 32-1
		};
	
/*	
		// Initial permutation IP, page 10.
		// Transposed to 0-based format.
		private static readonly byte [] ipBits = {
			58-1, 50-1, 42-1, 34-1, 26-1, 18-1, 10-1,  2-1,
			60-1, 52-1, 44-1, 36-1, 28-1, 20-1, 12-1,  4-1,
			62-1, 54-1, 46-1, 38-1, 30-1, 22-1, 14-1,  6-1,
			64-1, 56-1, 48-1, 40-1, 32-1, 24-1, 16-1,  8-1,
			57-1, 49-1, 41-1, 33-1, 25-1, 17-1,  9-1,  1-1,
			59-1, 51-1, 43-1, 35-1, 27-1, 19-1, 11-1,  3-1,
			61-1, 53-1, 45-1, 37-1, 29-1, 21-1, 13-1,  5-1,
			63-1, 55-1, 47-1, 39-1, 31-1, 23-1, 15-1,  7-1
		};
	
	
		// Final permutation FP = IP^(-1), page 10.
		// Transposed to 0-based format.
		private static readonly byte [] fpBits = {
			40-1,  8-1, 48-1, 16-1, 56-1, 24-1, 64-1, 32-1,
			39-1,  7-1, 47-1, 15-1, 55-1, 23-1, 63-1, 31-1,
			38-1,  6-1, 46-1, 14-1, 54-1, 22-1, 62-1, 30-1,
			37-1,  5-1, 45-1, 13-1, 53-1, 21-1, 61-1, 29-1,
			36-1,  4-1, 44-1, 12-1, 52-1, 20-1, 60-1, 28-1,
			35-1,  3-1, 43-1, 11-1, 51-1, 19-1, 59-1, 27-1,
			34-1,  2-1, 42-1, 10-1, 50-1, 18-1, 58-1, 26-1,
			33-1,  1-1, 41-1,  9-1, 49-1, 17-1, 57-1, 25-1
		};
*/	
		internal static readonly uint[] ipTab = {
			0x00000000, 0x00000000, 0x00000100, 0x00000000, 0x00000000, 0x00000100, 0x00000100, 0x00000100, 
			0x00000001, 0x00000000, 0x00000101, 0x00000000, 0x00000001, 0x00000100, 0x00000101, 0x00000100, 
			0x00000000, 0x00000001, 0x00000100, 0x00000001, 0x00000000, 0x00000101, 0x00000100, 0x00000101, 
			0x00000001, 0x00000001, 0x00000101, 0x00000001, 0x00000001, 0x00000101, 0x00000101, 0x00000101, 
			0x00000000, 0x00000000, 0x01000000, 0x00000000, 0x00000000, 0x01000000, 0x01000000, 0x01000000, 
			0x00010000, 0x00000000, 0x01010000, 0x00000000, 0x00010000, 0x01000000, 0x01010000, 0x01000000, 
			0x00000000, 0x00010000, 0x01000000, 0x00010000, 0x00000000, 0x01010000, 0x01000000, 0x01010000, 
			0x00010000, 0x00010000, 0x01010000, 0x00010000, 0x00010000, 0x01010000, 0x01010000, 0x01010000, 
			0x00000000, 0x00000000, 0x00000200, 0x00000000, 0x00000000, 0x00000200, 0x00000200, 0x00000200, 
			0x00000002, 0x00000000, 0x00000202, 0x00000000, 0x00000002, 0x00000200, 0x00000202, 0x00000200, 
			0x00000000, 0x00000002, 0x00000200, 0x00000002, 0x00000000, 0x00000202, 0x00000200, 0x00000202, 
			0x00000002, 0x00000002, 0x00000202, 0x00000002, 0x00000002, 0x00000202, 0x00000202, 0x00000202, 
			0x00000000, 0x00000000, 0x02000000, 0x00000000, 0x00000000, 0x02000000, 0x02000000, 0x02000000, 
			0x00020000, 0x00000000, 0x02020000, 0x00000000, 0x00020000, 0x02000000, 0x02020000, 0x02000000, 
			0x00000000, 0x00020000, 0x02000000, 0x00020000, 0x00000000, 0x02020000, 0x02000000, 0x02020000, 
			0x00020000, 0x00020000, 0x02020000, 0x00020000, 0x00020000, 0x02020000, 0x02020000, 0x02020000, 
			0x00000000, 0x00000000, 0x00000400, 0x00000000, 0x00000000, 0x00000400, 0x00000400, 0x00000400, 
			0x00000004, 0x00000000, 0x00000404, 0x00000000, 0x00000004, 0x00000400, 0x00000404, 0x00000400, 
			0x00000000, 0x00000004, 0x00000400, 0x00000004, 0x00000000, 0x00000404, 0x00000400, 0x00000404, 
			0x00000004, 0x00000004, 0x00000404, 0x00000004, 0x00000004, 0x00000404, 0x00000404, 0x00000404, 
			0x00000000, 0x00000000, 0x04000000, 0x00000000, 0x00000000, 0x04000000, 0x04000000, 0x04000000, 
			0x00040000, 0x00000000, 0x04040000, 0x00000000, 0x00040000, 0x04000000, 0x04040000, 0x04000000, 
			0x00000000, 0x00040000, 0x04000000, 0x00040000, 0x00000000, 0x04040000, 0x04000000, 0x04040000, 
			0x00040000, 0x00040000, 0x04040000, 0x00040000, 0x00040000, 0x04040000, 0x04040000, 0x04040000, 
			0x00000000, 0x00000000, 0x00000800, 0x00000000, 0x00000000, 0x00000800, 0x00000800, 0x00000800, 
			0x00000008, 0x00000000, 0x00000808, 0x00000000, 0x00000008, 0x00000800, 0x00000808, 0x00000800, 
			0x00000000, 0x00000008, 0x00000800, 0x00000008, 0x00000000, 0x00000808, 0x00000800, 0x00000808, 
			0x00000008, 0x00000008, 0x00000808, 0x00000008, 0x00000008, 0x00000808, 0x00000808, 0x00000808, 
			0x00000000, 0x00000000, 0x08000000, 0x00000000, 0x00000000, 0x08000000, 0x08000000, 0x08000000, 
			0x00080000, 0x00000000, 0x08080000, 0x00000000, 0x00080000, 0x08000000, 0x08080000, 0x08000000, 
			0x00000000, 0x00080000, 0x08000000, 0x00080000, 0x00000000, 0x08080000, 0x08000000, 0x08080000, 
			0x00080000, 0x00080000, 0x08080000, 0x00080000, 0x00080000, 0x08080000, 0x08080000, 0x08080000, 
			0x00000000, 0x00000000, 0x00001000, 0x00000000, 0x00000000, 0x00001000, 0x00001000, 0x00001000, 
			0x00000010, 0x00000000, 0x00001010, 0x00000000, 0x00000010, 0x00001000, 0x00001010, 0x00001000, 
			0x00000000, 0x00000010, 0x00001000, 0x00000010, 0x00000000, 0x00001010, 0x00001000, 0x00001010, 
			0x00000010, 0x00000010, 0x00001010, 0x00000010, 0x00000010, 0x00001010, 0x00001010, 0x00001010, 
			0x00000000, 0x00000000, 0x10000000, 0x00000000, 0x00000000, 0x10000000, 0x10000000, 0x10000000, 
			0x00100000, 0x00000000, 0x10100000, 0x00000000, 0x00100000, 0x10000000, 0x10100000, 0x10000000, 
			0x00000000, 0x00100000, 0x10000000, 0x00100000, 0x00000000, 0x10100000, 0x10000000, 0x10100000, 
			0x00100000, 0x00100000, 0x10100000, 0x00100000, 0x00100000, 0x10100000, 0x10100000, 0x10100000, 
			0x00000000, 0x00000000, 0x00002000, 0x00000000, 0x00000000, 0x00002000, 0x00002000, 0x00002000, 
			0x00000020, 0x00000000, 0x00002020, 0x00000000, 0x00000020, 0x00002000, 0x00002020, 0x00002000, 
			0x00000000, 0x00000020, 0x00002000, 0x00000020, 0x00000000, 0x00002020, 0x00002000, 0x00002020, 
			0x00000020, 0x00000020, 0x00002020, 0x00000020, 0x00000020, 0x00002020, 0x00002020, 0x00002020, 
			0x00000000, 0x00000000, 0x20000000, 0x00000000, 0x00000000, 0x20000000, 0x20000000, 0x20000000, 
			0x00200000, 0x00000000, 0x20200000, 0x00000000, 0x00200000, 0x20000000, 0x20200000, 0x20000000, 
			0x00000000, 0x00200000, 0x20000000, 0x00200000, 0x00000000, 0x20200000, 0x20000000, 0x20200000, 
			0x00200000, 0x00200000, 0x20200000, 0x00200000, 0x00200000, 0x20200000, 0x20200000, 0x20200000, 
			0x00000000, 0x00000000, 0x00004000, 0x00000000, 0x00000000, 0x00004000, 0x00004000, 0x00004000, 
			0x00000040, 0x00000000, 0x00004040, 0x00000000, 0x00000040, 0x00004000, 0x00004040, 0x00004000, 
			0x00000000, 0x00000040, 0x00004000, 0x00000040, 0x00000000, 0x00004040, 0x00004000, 0x00004040, 
			0x00000040, 0x00000040, 0x00004040, 0x00000040, 0x00000040, 0x00004040, 0x00004040, 0x00004040, 
			0x00000000, 0x00000000, 0x40000000, 0x00000000, 0x00000000, 0x40000000, 0x40000000, 0x40000000, 
			0x00400000, 0x00000000, 0x40400000, 0x00000000, 0x00400000, 0x40000000, 0x40400000, 0x40000000, 
			0x00000000, 0x00400000, 0x40000000, 0x00400000, 0x00000000, 0x40400000, 0x40000000, 0x40400000, 
			0x00400000, 0x00400000, 0x40400000, 0x00400000, 0x00400000, 0x40400000, 0x40400000, 0x40400000, 
			0x00000000, 0x00000000, 0x00008000, 0x00000000, 0x00000000, 0x00008000, 0x00008000, 0x00008000, 
			0x00000080, 0x00000000, 0x00008080, 0x00000000, 0x00000080, 0x00008000, 0x00008080, 0x00008000, 
			0x00000000, 0x00000080, 0x00008000, 0x00000080, 0x00000000, 0x00008080, 0x00008000, 0x00008080, 
			0x00000080, 0x00000080, 0x00008080, 0x00000080, 0x00000080, 0x00008080, 0x00008080, 0x00008080, 
			0x00000000, 0x00000000, 0x80000000, 0x00000000, 0x00000000, 0x80000000, 0x80000000, 0x80000000, 
			0x00800000, 0x00000000, 0x80800000, 0x00000000, 0x00800000, 0x80000000, 0x80800000, 0x80000000, 
			0x00000000, 0x00800000, 0x80000000, 0x00800000, 0x00000000, 0x80800000, 0x80000000, 0x80800000, 
			0x00800000, 0x00800000, 0x80800000, 0x00800000, 0x00800000, 0x80800000, 0x80800000, 0x80800000 
		};

		internal static readonly uint[] fpTab = { 
			0x00000000, 0x00000000, 0x00000000, 0x00000040, 0x00000000, 0x00004000, 0x00000000, 0x00004040, 
			0x00000000, 0x00400000, 0x00000000, 0x00400040, 0x00000000, 0x00404000, 0x00000000, 0x00404040, 
			0x00000000, 0x40000000, 0x00000000, 0x40000040, 0x00000000, 0x40004000, 0x00000000, 0x40004040, 
			0x00000000, 0x40400000, 0x00000000, 0x40400040, 0x00000000, 0x40404000, 0x00000000, 0x40404040, 
			0x00000000, 0x00000000, 0x00000040, 0x00000000, 0x00004000, 0x00000000, 0x00004040, 0x00000000, 
			0x00400000, 0x00000000, 0x00400040, 0x00000000, 0x00404000, 0x00000000, 0x00404040, 0x00000000, 
			0x40000000, 0x00000000, 0x40000040, 0x00000000, 0x40004000, 0x00000000, 0x40004040, 0x00000000, 
			0x40400000, 0x00000000, 0x40400040, 0x00000000, 0x40404000, 0x00000000, 0x40404040, 0x00000000, 
			0x00000000, 0x00000000, 0x00000000, 0x00000010, 0x00000000, 0x00001000, 0x00000000, 0x00001010, 
			0x00000000, 0x00100000, 0x00000000, 0x00100010, 0x00000000, 0x00101000, 0x00000000, 0x00101010, 
			0x00000000, 0x10000000, 0x00000000, 0x10000010, 0x00000000, 0x10001000, 0x00000000, 0x10001010, 
			0x00000000, 0x10100000, 0x00000000, 0x10100010, 0x00000000, 0x10101000, 0x00000000, 0x10101010, 
			0x00000000, 0x00000000, 0x00000010, 0x00000000, 0x00001000, 0x00000000, 0x00001010, 0x00000000, 
			0x00100000, 0x00000000, 0x00100010, 0x00000000, 0x00101000, 0x00000000, 0x00101010, 0x00000000, 
			0x10000000, 0x00000000, 0x10000010, 0x00000000, 0x10001000, 0x00000000, 0x10001010, 0x00000000, 
			0x10100000, 0x00000000, 0x10100010, 0x00000000, 0x10101000, 0x00000000, 0x10101010, 0x00000000, 
			0x00000000, 0x00000000, 0x00000000, 0x00000004, 0x00000000, 0x00000400, 0x00000000, 0x00000404, 
			0x00000000, 0x00040000, 0x00000000, 0x00040004, 0x00000000, 0x00040400, 0x00000000, 0x00040404, 
			0x00000000, 0x04000000, 0x00000000, 0x04000004, 0x00000000, 0x04000400, 0x00000000, 0x04000404, 
			0x00000000, 0x04040000, 0x00000000, 0x04040004, 0x00000000, 0x04040400, 0x00000000, 0x04040404, 
			0x00000000, 0x00000000, 0x00000004, 0x00000000, 0x00000400, 0x00000000, 0x00000404, 0x00000000, 
			0x00040000, 0x00000000, 0x00040004, 0x00000000, 0x00040400, 0x00000000, 0x00040404, 0x00000000, 
			0x04000000, 0x00000000, 0x04000004, 0x00000000, 0x04000400, 0x00000000, 0x04000404, 0x00000000, 
			0x04040000, 0x00000000, 0x04040004, 0x00000000, 0x04040400, 0x00000000, 0x04040404, 0x00000000, 
			0x00000000, 0x00000000, 0x00000000, 0x00000001, 0x00000000, 0x00000100, 0x00000000, 0x00000101, 
			0x00000000, 0x00010000, 0x00000000, 0x00010001, 0x00000000, 0x00010100, 0x00000000, 0x00010101, 
			0x00000000, 0x01000000, 0x00000000, 0x01000001, 0x00000000, 0x01000100, 0x00000000, 0x01000101, 
			0x00000000, 0x01010000, 0x00000000, 0x01010001, 0x00000000, 0x01010100, 0x00000000, 0x01010101, 
			0x00000000, 0x00000000, 0x00000001, 0x00000000, 0x00000100, 0x00000000, 0x00000101, 0x00000000, 
			0x00010000, 0x00000000, 0x00010001, 0x00000000, 0x00010100, 0x00000000, 0x00010101, 0x00000000, 
			0x01000000, 0x00000000, 0x01000001, 0x00000000, 0x01000100, 0x00000000, 0x01000101, 0x00000000, 
			0x01010000, 0x00000000, 0x01010001, 0x00000000, 0x01010100, 0x00000000, 0x01010101, 0x00000000, 
			0x00000000, 0x00000000, 0x00000000, 0x00000080, 0x00000000, 0x00008000, 0x00000000, 0x00008080, 
			0x00000000, 0x00800000, 0x00000000, 0x00800080, 0x00000000, 0x00808000, 0x00000000, 0x00808080, 
			0x00000000, 0x80000000, 0x00000000, 0x80000080, 0x00000000, 0x80008000, 0x00000000, 0x80008080, 
			0x00000000, 0x80800000, 0x00000000, 0x80800080, 0x00000000, 0x80808000, 0x00000000, 0x80808080, 
			0x00000000, 0x00000000, 0x00000080, 0x00000000, 0x00008000, 0x00000000, 0x00008080, 0x00000000, 
			0x00800000, 0x00000000, 0x00800080, 0x00000000, 0x00808000, 0x00000000, 0x00808080, 0x00000000, 
			0x80000000, 0x00000000, 0x80000080, 0x00000000, 0x80008000, 0x00000000, 0x80008080, 0x00000000, 
			0x80800000, 0x00000000, 0x80800080, 0x00000000, 0x80808000, 0x00000000, 0x80808080, 0x00000000, 
			0x00000000, 0x00000000, 0x00000000, 0x00000020, 0x00000000, 0x00002000, 0x00000000, 0x00002020, 
			0x00000000, 0x00200000, 0x00000000, 0x00200020, 0x00000000, 0x00202000, 0x00000000, 0x00202020, 
			0x00000000, 0x20000000, 0x00000000, 0x20000020, 0x00000000, 0x20002000, 0x00000000, 0x20002020, 
			0x00000000, 0x20200000, 0x00000000, 0x20200020, 0x00000000, 0x20202000, 0x00000000, 0x20202020, 
			0x00000000, 0x00000000, 0x00000020, 0x00000000, 0x00002000, 0x00000000, 0x00002020, 0x00000000, 
			0x00200000, 0x00000000, 0x00200020, 0x00000000, 0x00202000, 0x00000000, 0x00202020, 0x00000000, 
			0x20000000, 0x00000000, 0x20000020, 0x00000000, 0x20002000, 0x00000000, 0x20002020, 0x00000000, 
			0x20200000, 0x00000000, 0x20200020, 0x00000000, 0x20202000, 0x00000000, 0x20202020, 0x00000000, 
			0x00000000, 0x00000000, 0x00000000, 0x00000008, 0x00000000, 0x00000800, 0x00000000, 0x00000808, 
			0x00000000, 0x00080000, 0x00000000, 0x00080008, 0x00000000, 0x00080800, 0x00000000, 0x00080808, 
			0x00000000, 0x08000000, 0x00000000, 0x08000008, 0x00000000, 0x08000800, 0x00000000, 0x08000808, 
			0x00000000, 0x08080000, 0x00000000, 0x08080008, 0x00000000, 0x08080800, 0x00000000, 0x08080808, 
			0x00000000, 0x00000000, 0x00000008, 0x00000000, 0x00000800, 0x00000000, 0x00000808, 0x00000000, 
			0x00080000, 0x00000000, 0x00080008, 0x00000000, 0x00080800, 0x00000000, 0x00080808, 0x00000000, 
			0x08000000, 0x00000000, 0x08000008, 0x00000000, 0x08000800, 0x00000000, 0x08000808, 0x00000000, 
			0x08080000, 0x00000000, 0x08080008, 0x00000000, 0x08080800, 0x00000000, 0x08080808, 0x00000000, 
			0x00000000, 0x00000000, 0x00000000, 0x00000002, 0x00000000, 0x00000200, 0x00000000, 0x00000202, 
			0x00000000, 0x00020000, 0x00000000, 0x00020002, 0x00000000, 0x00020200, 0x00000000, 0x00020202, 
			0x00000000, 0x02000000, 0x00000000, 0x02000002, 0x00000000, 0x02000200, 0x00000000, 0x02000202, 
			0x00000000, 0x02020000, 0x00000000, 0x02020002, 0x00000000, 0x02020200, 0x00000000, 0x02020202, 
			0x00000000, 0x00000000, 0x00000002, 0x00000000, 0x00000200, 0x00000000, 0x00000202, 0x00000000, 
			0x00020000, 0x00000000, 0x00020002, 0x00000000, 0x00020200, 0x00000000, 0x00020202, 0x00000000, 
			0x02000000, 0x00000000, 0x02000002, 0x00000000, 0x02000200, 0x00000000, 0x02000202, 0x00000000, 
			0x02020000, 0x00000000, 0x02020002, 0x00000000, 0x02020200, 0x00000000, 0x02020202, 0x00000000 
			};

/*		static DESTransform ()
		{
			spBoxes = new uint [64 * 8];
	
			int [] pBox = new int [32];
	
			for (int p = 0; p < 32; p++) {
				for (int i = 0; i < 32; i++) {
					if (p == pTab [i]) {
						pBox [p] = i;
						break;
					}
				}
			}
	
			for (int s = 0; s < 8; s++) { // for each S-box
				int sOff = s << 6;
	
				for (int i = 0; i < 64; i++) { // inputs
					uint sp=0;
	
					int indx = (i & 0x20) | ((i & 1) << 4) | ((i >> 1) & 0xF);
	
					for (int j = 0; j < 4; j++) { // for each bit in the output
						if ((sBoxes [sOff + indx] & (8 >> j)) != 0) {
							sp |= (uint) (1 << (31 - pBox [(s << 2) + j]));
						}
					}
	
					spBoxes [sOff + i] = sp;
				}
			}

			leftRotTotal = new byte [leftRot.Length];
	
			for (int i = 0; i < leftRot.Length; i++) {
				int r = 0;
				for (int j = 0; j <= i; r += leftRot [j++]) {
					// no statement (confuse the compiler == warning)
				}
				leftRotTotal [i]  = (byte) r;
			}

			InitPermutationTable (ipBits, out ipTab);
			InitPermutationTable (fpBits, out fpTab);
		}
*/	
		// Default constructor.
		internal DESTransform (SymmetricAlgorithm symmAlgo, bool encryption, byte[] key, byte[] iv) 
			: base (symmAlgo, encryption, iv)
		{
			byte[] clonedKey = null;
#if NET_2_0
			if (key == null) {
				key = GetStrongKey ();
				clonedKey = key; // no need to clone
			}
#endif
			// note: checking (semi-)weak keys also checks valid key length
			if (DES.IsWeakKey (key) || DES.IsSemiWeakKey (key)) {
				string msg = Locale.GetText ("This is a known weak, or semi-weak, key.");
				throw new CryptographicException (msg);
			}

			if (clonedKey == null)
				clonedKey = (byte[]) key.Clone ();

			keySchedule = new byte [KEY_BYTE_SIZE * 16];
			byteBuff = new byte [BLOCK_BYTE_SIZE];
			dwordBuff = new uint [BLOCK_BYTE_SIZE / 4];
			SetKey (clonedKey);
		}

/* Permutation Tables are now precomputed.
		private static void InitPermutationTable (byte[] pBits, out int[] permTab)
		{
			permTab = new int [8*2 * 8*2 * (64/32)];
	
			for (int i = 0; i < 16; i++) {
				for (int j = 0; j < 16; j++) {
					int offs = (i << 5) + (j << 1);
					for (int n = 0; n < 64; n++) {
						int bitNum = (int) pBits [n];
						if ((bitNum >> 2 == i) &&
							0 != (j & (8 >> (bitNum & 3)))) {
							permTab [offs + (n >> (3+2))] |= (int) ((0x80808080 & (0xFF << (n & (3 << 3)))) >> (n & 7));
						}
					}
				}
			}
		}
*/

		private uint CipherFunct (uint r, int n)
		{
			uint res = 0;
			byte[] subkey = keySchedule;
			int i = n << 3;
	
			uint rt = (r >> 1) | (r << 31); // ROR32(r)
			res |= spBoxes [0*64 + (((rt >> 26) ^ subkey [i++]) & 0x3F)];
			res |= spBoxes [1*64 + (((rt >> 22) ^ subkey [i++]) & 0x3F)];
			res |= spBoxes [2*64 + (((rt >> 18) ^ subkey [i++]) & 0x3F)];
			res |= spBoxes [3*64 + (((rt >> 14) ^ subkey [i++]) & 0x3F)];
			res |= spBoxes [4*64 + (((rt >> 10) ^ subkey [i++]) & 0x3F)];
			res |= spBoxes [5*64 + (((rt >>  6) ^ subkey [i++]) & 0x3F)];
			res |= spBoxes [6*64 + (((rt >>  2) ^ subkey [i++]) & 0x3F)];
			rt = (r << 1) | (r >> 31); // ROL32(r)
			res |= spBoxes [7*64 + ((rt ^ subkey [i]) & 0x3F)];
			return res;
		}
	
		internal static void Permutation (byte[] input, byte[] output, uint[] permTab, bool preSwap)
		{
			if (preSwap && BitConverter.IsLittleEndian)
				BSwap (input);
	
			int offs1 = (((int)(input [0]) >> 4)) << 1;
			int offs2 = (1 << 5) + ((((int)input [0]) & 0xF) << 1);
	
			uint d1 = permTab [offs1++] | permTab [offs2++];
			uint d2 = permTab [offs1]   | permTab [offs2];
	
			int max = BLOCK_BYTE_SIZE << 1;
			for (int i = 2, indx = 1; i < max; i += 2, indx++) {
				int ii = (int) input [indx];
				offs1 = (i << 5) + ((ii >> 4) << 1);
				offs2 = ((i + 1) << 5) + ((ii & 0xF) << 1);
	
				d1 |= permTab [offs1++] | permTab [offs2++];
				d2 |= permTab [offs1]   | permTab [offs2];
			}
	
			if (preSwap || !BitConverter.IsLittleEndian) {
				output [0] = (byte) (d1);
				output [1] = (byte) (d1 >> 8);
				output [2] = (byte) (d1 >> 16);
				output [3] = (byte) (d1 >> 24);
				output [4] = (byte) (d2);
				output [5] = (byte) (d2 >> 8);
				output [6] = (byte) (d2 >> 16);
				output [7] = (byte) (d2 >> 24);
			}
			else {
				output [0] = (byte) (d1 >> 24);
				output [1] = (byte) (d1 >> 16);
				output [2] = (byte) (d1 >> 8);
				output [3] = (byte) (d1);
				output [4] = (byte) (d2 >> 24);
				output [5] = (byte) (d2 >> 16);
				output [6] = (byte) (d2 >> 8);
				output [7] = (byte) (d2);
			}
		}
	
		private static void BSwap (byte [] byteBuff)
		{
			byte t = byteBuff [0];
			byteBuff [0] = byteBuff [3];
			byteBuff [3] = t;
	
			t = byteBuff [1];
			byteBuff [1] = byteBuff [2];
			byteBuff [2] = t;
	
			t = byteBuff [4];
			byteBuff [4] = byteBuff [7];
			byteBuff [7] = t;
	
			t = byteBuff [5];
			byteBuff [5] = byteBuff [6];
			byteBuff [6] = t;
		}
	
		internal void SetKey (byte[] key)
		{
			// NOTE: see Fig. 3, Key schedule calculation, at page 20.
			Array.Clear (keySchedule, 0, keySchedule.Length);
	
			int keyBitSize = PC1.Length;
	
			byte[] keyPC1 = new byte [keyBitSize]; // PC1-permuted key
			byte[] keyRot = new byte [keyBitSize]; // PC1 & rotated
	
			int indx = 0;
	
			foreach (byte bitPos in PC1) {
				keyPC1 [indx++] = (byte)((key [(int)bitPos >> 3] >> (7 ^ (bitPos & 7))) & 1);
			}
	
			int j;
			for (int i = 0; i < KEY_BYTE_SIZE*2; i++) {
				int b = keyBitSize >> 1;
	
				for (j = 0; j < b; j++) {
					int s = j + (int) leftRotTotal [i];
					keyRot [j] = keyPC1 [s < b ? s : s - b];
				}
	
				for (j = b; j < keyBitSize; j++) {
					int s = j + (int) leftRotTotal [i];
					keyRot [j] = keyPC1 [s < keyBitSize ? s : s - b];
				}
	
				int keyOffs = i * KEY_BYTE_SIZE;
	
				j = 0;
				foreach (byte bitPos in PC2) {
					if (keyRot [(int)bitPos] != 0) {
						keySchedule [keyOffs + (j/6)] |= (byte) (0x80 >> ((j % 6) + 2));
					}
					j++;
				}
			}
		}
	
		// public helper for TripleDES
		public void ProcessBlock (byte[] input, byte[] output) 
		{
			Buffer.BlockCopy (input, 0, dwordBuff, 0, BLOCK_BYTE_SIZE);
	
			if (encrypt) {
				uint d0 = dwordBuff [0];
				uint d1 = dwordBuff [1];
	
				// 16 rounds
				d0 ^= CipherFunct (d1,  0);
				d1 ^= CipherFunct (d0,  1);
				d0 ^= CipherFunct (d1,  2);
				d1 ^= CipherFunct (d0,  3);
				d0 ^= CipherFunct (d1,  4);
				d1 ^= CipherFunct (d0,  5);
				d0 ^= CipherFunct (d1,  6);
				d1 ^= CipherFunct (d0,  7);
				d0 ^= CipherFunct (d1,  8);
				d1 ^= CipherFunct (d0,  9);
				d0 ^= CipherFunct (d1, 10);
				d1 ^= CipherFunct (d0, 11);
				d0 ^= CipherFunct (d1, 12);
				d1 ^= CipherFunct (d0, 13);
				d0 ^= CipherFunct (d1, 14);
				d1 ^= CipherFunct (d0, 15);
	
				dwordBuff [0] = d1;
				dwordBuff [1] = d0;
			}
			else {
				uint d1 = dwordBuff [0];
				uint d0 = dwordBuff [1];
	
				// 16 rounds in reverse order
				d1 ^= CipherFunct (d0, 15);
				d0 ^= CipherFunct (d1, 14);
				d1 ^= CipherFunct (d0, 13);
				d0 ^= CipherFunct (d1, 12);
				d1 ^= CipherFunct (d0, 11);
				d0 ^= CipherFunct (d1, 10);
				d1 ^= CipherFunct (d0,  9);
				d0 ^= CipherFunct (d1,  8);
				d1 ^= CipherFunct (d0,  7);
				d0 ^= CipherFunct (d1,  6);
				d1 ^= CipherFunct (d0,  5);
				d0 ^= CipherFunct (d1,  4);
				d1 ^= CipherFunct (d0,  3);
				d0 ^= CipherFunct (d1,  2);
				d1 ^= CipherFunct (d0,  1);
				d0 ^= CipherFunct (d1,  0);
	
				dwordBuff [0] = d0;
				dwordBuff [1] = d1;
			}
	
			Buffer.BlockCopy (dwordBuff, 0, output, 0, BLOCK_BYTE_SIZE);
		}
	
		protected override void ECB (byte[] input, byte[] output) 
		{
			Permutation (input, output, ipTab, false);
			ProcessBlock (output, byteBuff);
			Permutation (byteBuff, output, fpTab, true);
		}

		static internal byte[] GetStrongKey ()
		{
			byte[] key = KeyBuilder.Key (DESTransform.KEY_BYTE_SIZE);
			while (DES.IsWeakKey (key) || DES.IsSemiWeakKey (key))
				key = KeyBuilder.Key (DESTransform.KEY_BYTE_SIZE);
			return key;
		}
	} 
	
#if NET_2_0
	[ComVisible (true)]
#endif
	public sealed class DESCryptoServiceProvider : DES {
	
		public DESCryptoServiceProvider ()
		{
		}
	
		public override ICryptoTransform CreateDecryptor (byte[] rgbKey, byte[] rgbIV) 
		{
			return new DESTransform (this, false, rgbKey, rgbIV);
		}
	
		public override ICryptoTransform CreateEncryptor (byte[] rgbKey, byte[] rgbIV) 
		{
			return new DESTransform (this, true, rgbKey, rgbIV);
		}
	
		public override void GenerateIV () 
		{
			IVValue = KeyBuilder.IV (DESTransform.BLOCK_BYTE_SIZE);
		}
	
		public override void GenerateKey () 
		{
			KeyValue = DESTransform.GetStrongKey ();
		}
	}
}

#endif

