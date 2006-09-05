//
// IBuiltInEvidenceTest.cs - NUnit Test Cases for IBuiltInEvidence
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;
using System;
using System.Reflection;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	// IBuiltInEvidence is internal but we can test it using reflection.

	[TestFixture]
	[Ignore ("not sure if we gonna need this and, if we do, if we must be compatible with it")]
	public class IBuiltInEvidenceTest {

		static byte [] msSpCert = { 0x30, 0x82, 0x05, 0x0F, 0x30, 0x82, 0x03, 0xF7, 0xA0, 0x03, 0x02, 0x01, 0x02, 0x02, 0x0A, 0x61, 0x07, 0x11, 0x43, 0x00, 0x00, 0x00, 0x00, 0x00, 0x34, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x05, 0x05, 0x00, 0x30, 0x81, 0xA6, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x55, 0x53, 0x31, 0x13, 0x30, 0x11, 0x06, 0x03, 0x55, 0x04, 0x08, 0x13, 0x0A, 0x57, 0x61, 0x73, 0x68, 0x69, 0x6E, 0x67, 0x74, 0x6F, 0x6E, 0x31, 0x10, 0x30, 0x0E, 0x06, 0x03, 0x55, 0x04, 0x07, 0x13, 0x07, 0x52, 0x65, 0x64, 0x6D, 0x6F, 0x6E, 0x64, 0x31, 0x1E, 0x30, 0x1C, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x15, 0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0x20, 0x43, 0x6F, 0x72, 0x70, 0x6F, 0x72, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x31, 0x2B, 0x30, 0x29, 0x06, 0x03, 0x55, 0x04, 0x0B, 0x13, 0x22, 0x43, 0x6F, 0x70, 0x79, 0x72, 0x69, 0x67, 0x68, 0x74, 0x20, 0x28, 0x63, 0x29, 0x20, 0x32, 0x30, 0x30, 0x30, 0x20, 0x4D, 0x69, 0x63, 0x72,
			0x6F, 0x73, 0x6F, 0x66, 0x74, 0x20, 0x43, 0x6F, 0x72, 0x70, 0x2E, 0x31, 0x23, 0x30, 0x21, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x1A, 0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0x20, 0x43, 0x6F, 0x64, 0x65, 0x20, 0x53, 0x69, 0x67, 0x6E, 0x69, 0x6E, 0x67, 0x20, 0x50, 0x43, 0x41, 0x30, 0x1E, 0x17, 0x0D, 0x30, 0x32, 0x30, 0x35, 0x32, 0x35, 0x30, 0x30, 0x35, 0x35, 0x34, 0x38, 0x5A, 0x17, 0x0D, 0x30, 0x33, 0x31, 0x31, 0x32, 0x35, 0x30, 0x31, 0x30, 0x35, 0x34, 0x38, 0x5A, 0x30, 0x81, 0xA1, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x55, 0x53, 0x31, 0x13, 0x30, 0x11, 0x06, 0x03, 0x55, 0x04, 0x08, 0x13, 0x0A, 0x57, 0x61, 0x73, 0x68, 0x69, 0x6E, 0x67, 0x74, 0x6F, 0x6E, 0x31, 0x10, 0x30, 0x0E, 0x06, 0x03, 0x55, 0x04, 0x07, 0x13, 0x07, 0x52, 0x65, 0x64, 0x6D, 0x6F, 0x6E, 0x64, 0x31, 0x1E, 0x30, 0x1C, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x15, 0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0x20, 0x43, 0x6F, 0x72, 0x70, 0x6F, 0x72, 0x61,
			0x74, 0x69, 0x6F, 0x6E, 0x31, 0x2B, 0x30, 0x29, 0x06, 0x03, 0x55, 0x04, 0x0B, 0x13, 0x22, 0x43, 0x6F, 0x70, 0x79, 0x72, 0x69, 0x67, 0x68, 0x74, 0x20, 0x28, 0x63, 0x29, 0x20, 0x32, 0x30, 0x30, 0x32, 0x20, 0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0x20, 0x43, 0x6F, 0x72, 0x70, 0x2E, 0x31, 0x1E, 0x30, 0x1C, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x15, 0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0x20, 0x43, 0x6F, 0x72, 0x70, 0x6F, 0x72, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x30, 0x82, 0x01, 0x22, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00, 0x03, 0x82, 0x01, 0x0F, 0x00, 0x30, 0x82, 0x01, 0x0A, 0x02, 0x82, 0x01, 0x01, 0x00, 0xAA, 0x99, 0xBD, 0x39, 0xA8, 0x18, 0x27, 0xF4, 0x2B, 0x3D, 0x0B, 0x4C, 0x3F, 0x7C, 0x77, 0x2E, 0xA7, 0xCB, 0xB5, 0xD1, 0x8C, 0x0D, 0xC2, 0x3A, 0x74, 0xD7, 0x93, 0xB5, 0xE0, 0xA0, 0x4B, 0x3F, 0x59, 0x5E, 0xCE, 0x45, 0x4F, 0x9A, 0x79, 0x29, 0xF1, 0x49, 0xCC, 0x1A, 0x47, 0xEE, 0x55, 0xC2, 0x08,
			0x3E, 0x12, 0x20, 0xF8, 0x55, 0xF2, 0xEE, 0x5F, 0xD3, 0xE0, 0xCA, 0x96, 0xBC, 0x30, 0xDE, 0xFE, 0x58, 0xC8, 0x27, 0x32, 0xD0, 0x85, 0x54, 0xE8, 0xF0, 0x91, 0x10, 0xBB, 0xF3, 0x2B, 0xBE, 0x19, 0xE5, 0x03, 0x9B, 0x0B, 0x86, 0x1D, 0xF3, 0xB0, 0x39, 0x8C, 0xB8, 0xFD, 0x0B, 0x1D, 0x3C, 0x73, 0x26, 0xAC, 0x57, 0x2B, 0xCA, 0x29, 0xA2, 0x15, 0x90, 0x82, 0x15, 0xE2, 0x77, 0xA3, 0x40, 0x52, 0x03, 0x8B, 0x9D, 0xC2, 0x70, 0xBA, 0x1F, 0xE9, 0x34, 0xF6, 0xF3, 0x35, 0x92, 0x4E, 0x55, 0x83, 0xF8, 0xDA, 0x30, 0xB6, 0x20, 0xDE, 0x57, 0x06, 0xB5, 0x5A, 0x42, 0x06, 0xDE, 0x59, 0xCB, 0xF2, 0xDF, 0xA6, 0xBD, 0x15, 0x47, 0x71, 0x19, 0x25, 0x23, 0xD2, 0xCB, 0x6F, 0x9B, 0x19, 0x79, 0xDF, 0x6A, 0x5B, 0xF1, 0x76, 0x05, 0x79, 0x29, 0xFC, 0xC3, 0x56, 0xCA, 0x8F, 0x44, 0x08, 0x85, 0x55, 0x8A, 0xCB, 0xC8, 0x0F, 0x46, 0x4B, 0x55, 0xCB, 0x8C, 0x96, 0x77, 0x4A, 0x87, 0xE8, 0xA9, 0x41, 0x06, 0xC7, 0xFF, 0x0D, 0xE9, 0x68, 0x57, 0x63, 0x72, 0xC3, 0x69, 0x57, 0xB4, 0x43, 0xCF, 0x32, 0x3A, 0x30, 0xDC,
			0x1B, 0xE9, 0xD5, 0x43, 0x26, 0x2A, 0x79, 0xFE, 0x95, 0xDB, 0x22, 0x67, 0x24, 0xC9, 0x2F, 0xD0, 0x34, 0xE3, 0xE6, 0xFB, 0x51, 0x49, 0x86, 0xB8, 0x3C, 0xD0, 0x25, 0x5F, 0xD6, 0xEC, 0x9E, 0x03, 0x61, 0x87, 0xA9, 0x68, 0x40, 0xC7, 0xF8, 0xE2, 0x03, 0xE6, 0xCF, 0x05, 0x02, 0x03, 0x01, 0x00, 0x01, 0xA3, 0x82, 0x01, 0x40, 0x30, 0x82, 0x01, 0x3C, 0x30, 0x0E, 0x06, 0x03, 0x55, 0x1D, 0x0F, 0x01, 0x01, 0xFF, 0x04, 0x04, 0x03, 0x02, 0x06, 0xC0, 0x30, 0x13, 0x06, 0x03, 0x55, 0x1D, 0x25, 0x04, 0x0C, 0x30, 0x0A, 0x06, 0x08, 0x2B, 0x06, 0x01, 0x05, 0x05, 0x07, 0x03, 0x03, 0x30, 0x1D, 0x06, 0x03, 0x55, 0x1D, 0x0E, 0x04, 0x16, 0x04, 0x14, 0x6B, 0xC8, 0xC6, 0x51, 0x20, 0xF0, 0xB4, 0x2F, 0xD3, 0xA0, 0xB6, 0xAE, 0x7F, 0x5E, 0x26, 0xB2, 0xB8, 0x87, 0x52, 0x29, 0x30, 0x81, 0xA9, 0x06, 0x03, 0x55, 0x1D, 0x23, 0x04, 0x81, 0xA1, 0x30, 0x81, 0x9E, 0x80, 0x14, 0x29, 0x5C, 0xB9, 0x1B, 0xB6, 0xCD, 0x33, 0xEE, 0xBB, 0x9E, 0x59, 0x7D, 0xF7, 0xE5, 0xCA, 0x2E, 0xC4, 0x0D, 0x34, 0x28, 0xA1, 0x74,
			0xA4, 0x72, 0x30, 0x70, 0x31, 0x2B, 0x30, 0x29, 0x06, 0x03, 0x55, 0x04, 0x0B, 0x13, 0x22, 0x43, 0x6F, 0x70, 0x79, 0x72, 0x69, 0x67, 0x68, 0x74, 0x20, 0x28, 0x63, 0x29, 0x20, 0x31, 0x39, 0x39, 0x37, 0x20, 0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0x20, 0x43, 0x6F, 0x72, 0x70, 0x2E, 0x31, 0x1E, 0x30, 0x1C, 0x06, 0x03, 0x55, 0x04, 0x0B, 0x13, 0x15, 0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0x20, 0x43, 0x6F, 0x72, 0x70, 0x6F, 0x72, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x31, 0x21, 0x30, 0x1F, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x18, 0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0x20, 0x52, 0x6F, 0x6F, 0x74, 0x20, 0x41, 0x75, 0x74, 0x68, 0x6F, 0x72, 0x69, 0x74, 0x79, 0x82, 0x10, 0x6A, 0x0B, 0x99, 0x4F, 0xC0, 0x00, 0xDE, 0xAA, 0x11, 0xD4, 0xD8, 0x40, 0x9A, 0xA8, 0xBE, 0xE6, 0x30, 0x4A, 0x06, 0x03, 0x55, 0x1D, 0x1F, 0x04, 0x43, 0x30, 0x41, 0x30, 0x3F, 0xA0, 0x3D, 0xA0, 0x3B, 0x86, 0x39, 0x68, 0x74, 0x74, 0x70, 0x3A, 0x2F, 0x2F, 0x63, 0x72, 0x6C,
			0x2E, 0x6D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0x2E, 0x63, 0x6F, 0x6D, 0x2F, 0x70, 0x6B, 0x69, 0x2F, 0x63, 0x72, 0x6C, 0x2F, 0x70, 0x72, 0x6F, 0x64, 0x75, 0x63, 0x74, 0x73, 0x2F, 0x43, 0x6F, 0x64, 0x65, 0x53, 0x69, 0x67, 0x6E, 0x50, 0x43, 0x41, 0x2E, 0x63, 0x72, 0x6C, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x05, 0x05, 0x00, 0x03, 0x82, 0x01, 0x01, 0x00, 0x35, 0x23, 0xFD, 0x13, 0x54, 0xFC, 0xE9, 0xDC, 0xF0, 0xDD, 0x0C, 0x14, 0x7A, 0xFA, 0xA7, 0xB3, 0xCE, 0xFD, 0xA7, 0x3A, 0xC8, 0xBA, 0xE5, 0xE7, 0xF6, 0x03, 0xFB, 0x53, 0xDB, 0xA7, 0x99, 0xA9, 0xA0, 0x9B, 0x36, 0x9C, 0x03, 0xEB, 0x82, 0x47, 0x1C, 0x21, 0xBD, 0x14, 0xCB, 0xE7, 0x67, 0x40, 0x09, 0xC7, 0x16, 0x91, 0x02, 0x55, 0xCE, 0x43, 0x42, 0xB4, 0xCD, 0x1B, 0x5D, 0xB0, 0xF3, 0x32, 0x04, 0x3D, 0x12, 0xE5, 0x1D, 0xA7, 0x07, 0xA7, 0x8F, 0xA3, 0x7E, 0x45, 0x55, 0x76, 0x1B, 0x96, 0x95, 0x91, 0x69, 0xF0, 0xDD, 0x38, 0xF3, 0x48, 0x89, 0xEF, 0x70, 0x40, 0xB7, 0xDB, 0xB5, 0x55,
			0x80, 0xC0, 0x03, 0xC4, 0x2E, 0xB6, 0x28, 0xDC, 0x0A, 0x82, 0x0E, 0xC7, 0x43, 0xE3, 0x7A, 0x48, 0x5D, 0xB8, 0x06, 0x89, 0x92, 0x40, 0x6C, 0x6E, 0xC5, 0xDC, 0xF8, 0x9A, 0xEF, 0x0B, 0xBE, 0x21, 0x0A, 0x8C, 0x2F, 0x3A, 0xB5, 0xED, 0xA7, 0xCE, 0x71, 0x87, 0x68, 0x23, 0xE1, 0xB3, 0xE4, 0x18, 0x7D, 0xB8, 0x47, 0x01, 0xA5, 0x2B, 0xC4, 0x58, 0xCB, 0xB2, 0x89, 0x6C, 0x5F, 0xFD, 0xD3, 0x2C, 0xC4, 0x6F, 0xB8, 0x23, 0xB2, 0x0D, 0xFF, 0x3C, 0xF2, 0x11, 0x45, 0x74, 0xF2, 0x09, 0x06, 0x99, 0x18, 0xDD, 0x6F, 0xC0, 0x86, 0x01, 0x18, 0x12, 0x1D, 0x2B, 0x16, 0xAF, 0x56, 0xEF, 0x65, 0x33, 0xA1, 0xEA, 0x67, 0x4E, 0xF4, 0x4B, 0x82, 0xAB, 0xE9, 0x0F, 0xDC, 0x01, 0xFA, 0xDF, 0x60, 0x7F, 0x66, 0x47, 0x5D, 0xCB, 0x2C, 0x70, 0xCC, 0x7B, 0x4E, 0xD9, 0x06, 0xB8, 0x6E, 0x8C, 0x0C, 0xFE, 0x62, 0x1E, 0x42, 0xF9, 0x93, 0x7C, 0xA2, 0xAB, 0x0A, 0x9E, 0xD0, 0x23, 0x10, 0xAE, 0x4D, 0x7B, 0x27, 0x91, 0x6F, 0x26, 0xBE, 0x68, 0xFA, 0xA6, 0x3F, 0x9F, 0x23, 0xEB, 0xC8, 0x9D, 0xBB, 0x87 };

		private int GetRequiredSize (object evidence, bool verbose)
		{
			try {
				Type t = evidence.GetType ();
				object[] args = new object [1] { verbose };
				int result = (int)t.InvokeMember ("System.Security.Policy.IBuiltInEvidence.GetRequiredSize", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, evidence, args);
				return result;
			}
			catch (TargetInvocationException tie) {
				throw tie.InnerException;
			}
		}

		private int InitFromBuffer (object evidence, char[] buffer, int position)
		{
			try {
				Type t = evidence.GetType ();
				object [] args = new object [2] { buffer, position };
				int result = (int)t.InvokeMember ("System.Security.Policy.IBuiltInEvidence.InitFromBuffer", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, evidence, args);
				return result;
			}
			catch (TargetInvocationException tie) {
				throw tie.InnerException;
			}
		}

		private int OutputToBuffer (object evidence, char[] buffer, int position, bool verbose)
		{
			try {
				Type t = evidence.GetType ();
				object [] args = new object [3] { buffer, position, verbose };
				int result = (int)t.InvokeMember ("System.Security.Policy.IBuiltInEvidence.OutputToBuffer", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, evidence, args);
				return result;
			}
			catch (TargetInvocationException tie) {
				throw tie.InnerException;
			}
		}


		[Test]
		public void ApplicationDirectory_GetRequiredSize ()
		{
			ApplicationDirectory ad = new ApplicationDirectory ("file://MONO");
			Assert.AreEqual (14, GetRequiredSize (ad, true), "GetRequiredSize-true");
			Assert.AreEqual (12, GetRequiredSize (ad, false), "GetRequiredSize-false");
			ad = new ApplicationDirectory ("file://NOVELL/mono");
			Assert.AreEqual (21, GetRequiredSize (ad, true), "GetRequiredSize-true-2");
			Assert.AreEqual (19, GetRequiredSize (ad, false), "GetRequiredSize-false-2");
		}
#if NET_2_0
		[Test]
		public void GacInstalled_GetRequiredSize ()
		{
			GacInstalled g = new GacInstalled ();
			Assert.AreEqual (1, GetRequiredSize (g, true), "GetRequiredSize-true");
			Assert.AreEqual (1, GetRequiredSize (g, false), "GetRequiredSize-false");
		}

		[Test]
		public void GacInstalled_InitFromBuffer ()
		{
			char[] buffer = new char [2] { '\t', '\t' };
			GacInstalled g = new GacInstalled ();
			Assert.AreEqual (0, InitFromBuffer (g, buffer, 0), "InitFromBuffer-1");
			Assert.AreEqual (1, InitFromBuffer (g, buffer, 1), "InitFromBuffer-2");
		}

		[Test]
		public void GacInstalled_InitFromBuffer_BadData ()
		{
			char [] buffer = new char [1] { '\r' };
			GacInstalled g = new GacInstalled ();
			Assert.AreEqual (0, InitFromBuffer (g, buffer, 0), "InitFromBuffer");
		}

		[Test]
		public void GacInstalled_InitFromBuffer_Null ()
		{
			GacInstalled g = new GacInstalled ();
			Assert.AreEqual (0, InitFromBuffer (g, null, 0), "InitFromBuffer");
		}

		[Test]
		public void GacInstalled_InitFromBuffer_OutOfRange ()
		{
			char [] buffer = new char [1] { '\t' };
			GacInstalled g = new GacInstalled ();
			Assert.AreEqual (4, InitFromBuffer (g, buffer, 4), "InitFromBuffer");
		}

		[Test]
		public void GacInstalled_OutputToBuffer ()
		{
			char[] buffer = new char [2];
			GacInstalled g = new GacInstalled ();
			Assert.AreEqual (1, OutputToBuffer (g, buffer, 0, false), "OutputToBuffer-false");
			Assert.AreEqual ('\t', buffer [0], "Buffer-false");
			Assert.AreEqual (2, OutputToBuffer (g, buffer, 1, true), "OutputToBuffer-true");
			Assert.AreEqual ('\t', buffer [1], "Buffer-true");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void GacInstalled_OutputToBuffer_Null ()
		{
			GacInstalled g = new GacInstalled ();
			OutputToBuffer (g, null, 0, false);
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void GacInstalled_OutputToBuffer_OutOfRange ()
		{
			char[] buffer = new char [1] { '\t' };
			GacInstalled g = new GacInstalled ();
			OutputToBuffer (g, buffer, 1, false);
		}
#endif
		[Test]
		public void Hash_GetRequiredSize ()
		{
			Hash h = new Hash (Assembly.GetExecutingAssembly ());
			Assert.AreEqual (5, GetRequiredSize (h, true), "GetRequiredSize-true");
			Assert.AreEqual (0, GetRequiredSize (h, false), "GetRequiredSize-false");
		}

		[Test]
		public void PermissionRequestEvidence_GetRequiredSize ()
		{
			PermissionRequestEvidence pre = new PermissionRequestEvidence (null, null, null);
			Assert.AreEqual (3, GetRequiredSize (pre, true), "(null,null,null).GetRequiredSize-true");
			Assert.AreEqual (1, GetRequiredSize (pre, false), "(null,null,null).GetRequiredSize-false");

			PermissionSet ps = new PermissionSet (PermissionState.None);
			pre = new PermissionRequestEvidence (ps, null, null);
			Assert.AreEqual (75, GetRequiredSize (pre, true), "(none,null,null).GetRequiredSize-true");
			Assert.AreEqual (70, GetRequiredSize (pre, false), "(none,null,null).GetRequiredSize-false");

			pre = new PermissionRequestEvidence (ps, ps, null);
			Assert.AreEqual (147, GetRequiredSize (pre, true), "(none,none,null).GetRequiredSize-true");
			Assert.AreEqual (139, GetRequiredSize (pre, false), "(none,none,null).GetRequiredSize-false");

			pre = new PermissionRequestEvidence (ps, ps, ps);
			Assert.AreEqual (219, GetRequiredSize (pre, true), "(none,none,none).GetRequiredSize-true");
			Assert.AreEqual (208, GetRequiredSize (pre, false), "(none,none,none).GetRequiredSize-false");

			ps = new PermissionSet (PermissionState.Unrestricted);
			pre = new PermissionRequestEvidence (ps, ps, ps);
			Assert.AreEqual (282, GetRequiredSize (pre, true), "(unrestricted,unrestricted,unrestricted).GetRequiredSize-true");
			Assert.AreEqual (271, GetRequiredSize (pre, false), "(unrestricted,unrestricted,unrestricted).GetRequiredSize-false");
		}

		[Test]
		public void Publisher_GetRequiredSize ()
		{
			X509Certificate x509 = new X509Certificate (msSpCert);
			Publisher p = new Publisher (x509);
			Assert.AreEqual (653, GetRequiredSize (p, true), "GetRequiredSize-true");
			Assert.AreEqual (651, GetRequiredSize (p, false), "GetRequiredSize-false");
		}

		[Test]
		public void Site_GetRequiredSize ()
		{
			Site s = new Site ("www.mono-project.com");
			Assert.AreEqual (23, GetRequiredSize (s, true), "GetRequiredSize-true");
			Assert.AreEqual (21, GetRequiredSize (s, false), "GetRequiredSize-false");
			s = new Site ("www.go-mono.com");
			Assert.AreEqual (18, GetRequiredSize (s, true), "GetRequiredSize-true-2");
			Assert.AreEqual (16, GetRequiredSize (s, false), "GetRequiredSize-false-2");
		}

		[Test]
		public void StrongName_GetRequiredSize ()
		{
			byte[] pk = { 0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00, 0x06, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x31, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x3D, 0xBD, 0x72, 0x08, 0xC6, 0x2B, 0x0E, 0xA8, 0xC1, 0xC0, 0x58, 0x07, 0x2B, 0x63, 0x5F, 0x7C, 0x9A, 0xBD, 0xCB, 0x22, 0xDB, 0x20, 0xB2, 0xA9, 0xDA, 0xDA, 0xEF, 0xE8, 0x00, 0x64, 0x2F, 0x5D, 0x8D, 0xEB, 0x78, 0x02, 0xF7, 0xA5, 0x36, 0x77, 0x28, 0xD7, 0x55, 0x8D, 0x14, 0x68, 0xDB, 0xEB, 0x24, 0x09, 0xD0, 0x2B, 0x13, 0x1B, 0x92, 0x6E, 0x2E, 0x59, 0x54, 0x4A, 0xAC, 0x18, 0xCF, 0xC9, 0x09, 0x02, 0x3F, 0x4F, 0xA8, 0x3E, 0x94, 0x00, 0x1F, 0xC2, 0xF1, 0x1A, 0x27, 0x47, 0x7D, 0x10, 0x84, 0xF5, 0x14, 0xB8, 0x61, 0x62, 0x1A, 0x0C, 0x66, 0xAB, 0xD2, 0x4C, 0x4B, 0x9F, 0xC9, 0x0F, 0x3C, 0xD8, 0x92, 0x0F, 0xF5, 0xFF, 0xCE, 0xD7, 0x6E, 0x5C, 0x6F, 0xB1, 0xF5, 0x7D, 0xD3, 0x56, 0xF9, 0x67, 0x27, 0xA4, 0xA5, 0x48, 0x5B, 0x07, 0x93, 0x44, 0x00, 0x4A, 0xF8, 0xFF, 0xA4, 0xCB };
			StrongNamePublicKeyBlob snpkb = new StrongNamePublicKeyBlob (pk);
			StrongName sn = new StrongName (snpkb, "mono", new Version ());

			Assert.AreEqual (97, GetRequiredSize (sn, true), "GetRequiredSize-true");
			Assert.AreEqual (93, GetRequiredSize (sn, false), "GetRequiredSize-false");
		}

		[Test]
		public void Url_GetRequiredSize ()
		{
			Url u = new Url ("http://www.mono-project.com/");
			Assert.AreEqual (31, GetRequiredSize (u, true), "GetRequiredSize-true");
			Assert.AreEqual (29, GetRequiredSize (u, false), "GetRequiredSize-false");
			u = new Url ("http://www.go-mono.com/");
			Assert.AreEqual (26, GetRequiredSize (u, true), "GetRequiredSize-true-2");
			Assert.AreEqual (24, GetRequiredSize (u, false), "GetRequiredSize-false-2");
		}

		[Test]
		public void Zone_GetRequiredSize ()
		{
			Zone z = new Zone (SecurityZone.MyComputer);
			Assert.AreEqual (3, GetRequiredSize (z, true), "GetRequiredSize-true");
			Assert.AreEqual (3, GetRequiredSize (z, false), "GetRequiredSize-false");
		}
	}
}
