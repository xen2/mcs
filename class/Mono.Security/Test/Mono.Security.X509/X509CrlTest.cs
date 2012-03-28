//
// X509CrlTest.cs - NUnit Test Cases for the X509Crl class
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005, 2006 Novell, Inc (http://www.novell.com)
//

using System;
using Mono.Security.X509;

using NUnit.Framework;


namespace MonoTests.Mono.Security.X509 {

	[TestFixture]
	public class X509CrlTest {

		static public byte[] emptyCrl = { 0x30, 0x82, 0x02, 0x08, 0x30, 0x81, 0xF1, 0x02, 0x01, 0x01, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x05, 0x05, 0x00, 0x30, 0x31, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x44, 0x4B, 0x31, 0x0C, 0x30, 0x0A, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x03, 0x54, 0x44, 0x43, 0x31, 0x14, 0x30, 0x12, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x0B, 0x54, 0x44, 0x43, 0x20, 0x4F, 0x43, 0x45, 0x53, 0x20, 0x43, 0x41, 0x17, 0x0D, 0x30, 0x35, 0x30, 0x36, 0x32, 0x38, 0x30, 0x38, 0x30, 0x38, 0x30, 0x33, 0x5A, 0x17, 0x0D, 0x30, 0x35, 0x30, 0x36, 0x32, 0x38, 0x32, 0x31, 
			0x30, 0x38, 0x30, 0x33, 0x5A, 0xA0, 0x81, 0x8B, 0x30, 0x81, 0x88, 0x30, 0x59, 0x06, 0x03, 0x55, 0x1D, 0x1C, 0x01, 0x01, 0xFF, 0x04, 0x4F, 0x30, 0x4D, 0xA0, 0x48, 0xA0, 0x46, 0xA4, 0x44, 0x30, 0x42, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x44, 0x4B, 0x31, 0x0C, 0x30, 0x0A, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x03, 0x54, 0x44, 0x43, 0x31, 0x14, 0x30, 0x12, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x0B, 0x54, 0x44, 0x43, 0x20, 0x4F, 0x43, 0x45, 0x53, 0x20, 0x43, 0x41, 0x31, 0x0F, 0x30, 0x0D, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x06, 0x43, 0x52, 0x4C, 0x37, 0x30, 0x36, 0x81, 
			0x01, 0xFF, 0x30, 0x0A, 0x06, 0x03, 0x55, 0x1D, 0x14, 0x04, 0x03, 0x02, 0x01, 0x01, 0x30, 0x1F, 0x06, 0x03, 0x55, 0x1D, 0x23, 0x04, 0x18, 0x30, 0x16, 0x80, 0x14, 0x60, 0xB5, 0x85, 0xEC, 0x56, 0x64, 0x7E, 0x12, 0x19, 0x27, 0x67, 0x1D, 0x50, 0x15, 0x4B, 0x73, 0xAE, 0x3B, 0xF9, 0x12, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x05, 0x05, 0x00, 0x03, 0x82, 0x01, 0x01, 0x00, 0x7B, 0x45, 0x18, 0xCC, 0x96, 0xBD, 0x82, 0x87, 0x42, 0xA0, 0xAD, 0x82, 0x28, 0x0C, 0x62, 0x2C, 0x25, 0x51, 0x0D, 0x86, 0xC4, 0x07, 0x3C, 0xFA, 0x64, 0x42, 0x38, 0x33, 0x0B, 0x36, 0xE2, 0xC9, 0xAA, 
			0xBA, 0x33, 0x8F, 0x94, 0xBD, 0xCF, 0x58, 0x4F, 0xD9, 0xA5, 0x18, 0xE1, 0x55, 0x79, 0xD5, 0xD1, 0x74, 0x10, 0xE3, 0x32, 0x3F, 0x28, 0x81, 0x2B, 0x56, 0x22, 0x0B, 0x54, 0x19, 0x86, 0xA9, 0xF6, 0x9F, 0x44, 0x25, 0x2F, 0x8E, 0x2E, 0x38, 0xA7, 0x6C, 0x91, 0x75, 0xDD, 0xD5, 0x52, 0x2D, 0x52, 0x7D, 0xA7, 0x9D, 0x6B, 0x3E, 0xA0, 0xD9, 0x64, 0x08, 0xED, 0xC9, 0xC4, 0xA3, 0x4A, 0x4E, 0xE0, 0xBA, 0x96, 0x20, 0x93, 0xF5, 0x2F, 0x2E, 0x5F, 0x58, 0xFB, 0x84, 0x83, 0x31, 0xF0, 0x77, 0x61, 0x22, 0x34, 0x3A, 0x0C, 0x93, 0x0E, 0x20, 0xFE, 0x2A, 0xBB, 0x51, 0xA8, 0x17, 0xCA, 0x40, 0x77, 0xA2, 0x99, 0x19, 0x77, 
			0xBB, 0xAD, 0x82, 0x70, 0xE3, 0xCA, 0x5C, 0xC1, 0xF3, 0xF3, 0x3E, 0x6F, 0x77, 0xF9, 0x0B, 0x30, 0xDF, 0xAD, 0x2E, 0x8F, 0x7B, 0xF9, 0x8D, 0x76, 0xBF, 0xDA, 0x2A, 0x36, 0x59, 0x55, 0x43, 0x4F, 0xEC, 0xE5, 0xB6, 0x95, 0x10, 0xFB, 0xDC, 0x74, 0x13, 0xC3, 0x26, 0x8D, 0x24, 0x65, 0xF1, 0x66, 0x07, 0x7B, 0xAD, 0x5D, 0xC6, 0x77, 0xB9, 0x53, 0x15, 0xF2, 0xBE, 0xFD, 0x2E, 0x66, 0xF7, 0x2F, 0xBD, 0x28, 0xBB, 0x2E, 0x32, 0x8D, 0x72, 0xE9, 0x19, 0x74, 0x79, 0x0D, 0xBE, 0xFF, 0xBA, 0x57, 0xF0, 0x89, 0xE4, 0x27, 0x23, 0x47, 0x46, 0x1A, 0x5A, 0x27, 0xFB, 0x17, 0xF7, 0x24, 0xA4, 0x0E, 0xFD, 0xEC, 0x44, 0xA0, 
			0x37, 0x26, 0xF7, 0xD2, 0x6B, 0xE8, 0xD8, 0x3C, 0xBB, 0x8F, 0x61, 0x87, 0x7D, 0xFF, 0x3A, 0x6F, 0x7F, 0x53, 0x03, 0x2A, 0x11, 0xFA, 0xDC };

		[Test]
		public void EmptyCrlWithExtensions ()
		{
			// bug #75406
			// http://bugzilla.ximian.com/show_bug.cgi?id=75406
			X509Crl crl = new X509Crl (emptyCrl);
			Assert.AreEqual (0, crl.Entries.Count, "Entries.Count");
			Assert.AreEqual (3, crl.Extensions.Count, "Extensions.Count");
		}

		static public byte[] bug78901 = { 0x30, 0x82, 0x02, 0x02, 0x30, 0x81, 0xEB, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x04, 0x05, 0x00, 0x30, 0x81, 0xBB, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x46, 0x52, 0x31, 0x16, 0x30, 0x14, 0x06, 0x03, 0x55, 0x04, 0x08, 0x13, 0x0D, 0x49, 0x6C, 0x65, 0x20, 0x64, 0x65, 0x20, 0x46, 0x72, 0x61, 0x6E, 0x63, 0x65, 0x31, 0x0E, 0x30, 0x0C, 0x06, 0x03, 0x55, 0x04, 0x07, 0x13, 0x05, 0x50, 0x61, 0x72, 0x69, 0x73, 0x31, 0x15, 0x30, 0x13, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x0C, 0x52, 0x65, 0x63, 0x6F, 0x75, 0x76, 0x72, 0x65, 0x6D, 0x65, 0x6E, 0x74, 
			0x31, 0x0E, 0x30, 0x0C, 0x06, 0x03, 0x55, 0x04, 0x0B, 0x13, 0x05, 0x41, 0x43, 0x4F, 0x53, 0x53, 0x31, 0x0E, 0x30, 0x0C, 0x06, 0x03, 0x55, 0x04, 0x0B, 0x13, 0x05, 0x41, 0x43, 0x37, 0x35, 0x30, 0x31, 0x0D, 0x30, 0x0B, 0x06, 0x03, 0x55, 0x04, 0x0B, 0x13, 0x04, 0x53, 0x41, 0x45, 0x4C, 0x31, 0x21, 0x30, 0x1F, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x18, 0x41, 0x43, 0x4F, 0x53, 0x53, 0x20, 0x53, 0x41, 0x45, 0x4C, 0x20, 0x43, 0x45, 0x52, 0x54, 0x49, 0x46, 0x49, 0x43, 0x41, 0x54, 0x49, 0x4F, 0x4E, 0x31, 0x1B, 0x30, 0x19, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x09, 0x01, 0x16, 0x0C, 0x69, 
			0x67, 0x63, 0x40, 0x61, 0x63, 0x6F, 0x73, 0x73, 0x2E, 0x66, 0x72, 0x17, 0x0D, 0x30, 0x36, 0x30, 0x37, 0x31, 0x33, 0x31, 0x35, 0x33, 0x34, 0x31, 0x31, 0x5A, 0x17, 0x0D, 0x30, 0x36, 0x30, 0x38, 0x31, 0x32, 0x31, 0x35, 0x33, 0x34, 0x31, 0x31, 0x5A, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x04, 0x05, 0x00, 0x03, 0x82, 0x01, 0x01, 0x00, 0x89, 0x55, 0xF8, 0x5F, 0x9D, 0x3D, 0x3A, 0x26, 0x82, 0xA8, 0xFC, 0xBF, 0x4F, 0x9A, 0x3C, 0x50, 0x34, 0x30, 0x07, 0x3A, 0x9D, 0x18, 0xE9, 0xDD, 0x5E, 0x81, 0x98, 0x07, 0x5B, 0xE4, 0xAD, 0x05, 0x5A, 0x44, 0x82, 0xA8, 0xB5, 0x1F, 0x1D, 
			0xBB, 0xAC, 0x7B, 0x21, 0xE5, 0x47, 0x2A, 0x19, 0xBE, 0x2E, 0xA7, 0x76, 0x11, 0x1E, 0x43, 0xC5, 0x8F, 0xF5, 0xC1, 0x15, 0x49, 0xD1, 0xB0, 0x20, 0xB9, 0x98, 0x91, 0x4F, 0x3D, 0xBB, 0xD3, 0x35, 0x48, 0xF4, 0x84, 0xA4, 0x13, 0xAC, 0xAE, 0xF4, 0xA4, 0xE4, 0x7D, 0x8D, 0x9C, 0x3E, 0xBF, 0x40, 0xFF, 0x07, 0x7F, 0xE4, 0x28, 0x1E, 0xA6, 0x9A, 0x46, 0x41, 0xD3, 0x7C, 0x90, 0x36, 0x69, 0x55, 0x62, 0x62, 0x04, 0xD2, 0x88, 0x49, 0x08, 0x0D, 0x3C, 0x56, 0x71, 0x8A, 0xC4, 0x3E, 0xB0, 0xB8, 0xD8, 0xC0, 0x04, 0x21, 0xA4, 0x95, 0x8C, 0x6A, 0x61, 0xC0, 0x2E, 0xA8, 0x31, 0x78, 0x04, 0x91, 0x9C, 0x00, 0xF1, 0x33, 
			0x77, 0x50, 0x32, 0x6A, 0xF9, 0xBC, 0x8F, 0x22, 0x82, 0xD1, 0x04, 0x98, 0x3E, 0x43, 0xD4, 0x52, 0x41, 0x07, 0x1C, 0xF9, 0x92, 0x7E, 0x49, 0xD1, 0x48, 0x90, 0x79, 0x7A, 0x1C, 0x05, 0xF4, 0x79, 0x22, 0x6A, 0x99, 0x7F, 0x77, 0xCA, 0x23, 0xFE, 0x6B, 0x27, 0xED, 0xD8, 0x26, 0x08, 0x3F, 0x62, 0x3A, 0xD4, 0x12, 0x8E, 0x5F, 0xAF, 0xB8, 0x11, 0x5B, 0x43, 0x99, 0x77, 0x3A, 0xDE, 0x12, 0x35, 0xDF, 0x7D, 0x33, 0x16, 0xFB, 0xBE, 0x11, 0xBF, 0x84, 0x97, 0x0A, 0x7D, 0x73, 0x55, 0x2A, 0x65, 0x14, 0x8F, 0x8F, 0xD2, 0xCB, 0xF9, 0x3F, 0x6D, 0xA9, 0xE2, 0x54, 0x3B, 0xD7, 0x50, 0xB2, 0xA9, 0xE5, 0xF7, 0xA6, 0x2B, 
			0xCC, 0x52, 0xDE, 0x75, 0xB0, 0x35, 0xF1, 0xDC, 0x8C, 0x35, 0x70, 0x63, 0x31, 0x97, 0x73, 0x8E, 0x40 };

		[Test]
		public void EmptyCrl ()
		{
			// bug #78901
			// http://bugzilla.ximian.com/show_bug.cgi?id=78901
			X509Crl crl = new X509Crl (bug78901);
			Assert.AreEqual (0, crl.Entries.Count, "Entries.Count");
			Assert.AreEqual (0, crl.Extensions.Count, "Extensions.Count");
		}

		static public byte[] basicConstraintsCriticalcAFalseCACRL_crl = { 0x30, 0x82, 0x01, 0x52, 0x30, 0x81, 0xBC, 0x02, 0x01, 0x01, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x05, 0x05, 0x00, 0x30, 0x59, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x55, 0x53, 0x31, 0x1A, 0x30, 0x18, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x11, 0x54, 0x65, 0x73, 0x74, 0x20, 0x43, 0x65, 0x72, 0x74, 0x69, 0x66, 0x69, 0x63, 0x61, 0x74, 0x65, 0x73, 0x31, 0x2E, 0x30, 0x2C, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x25, 0x62, 0x61, 0x73, 0x69, 0x63, 0x43, 0x6F, 0x6E, 0x73, 0x74, 0x72, 0x61, 0x69, 0x6E, 0x74, 0x73, 0x20, 0x43, 0x72, 0x69, 0x74, 0x69, 
			0x63, 0x61, 0x6C, 0x20, 0x63, 0x41, 0x20, 0x46, 0x61, 0x6C, 0x73, 0x65, 0x20, 0x43, 0x41, 0x17, 0x0D, 0x30, 0x31, 0x30, 0x34, 0x31, 0x39, 0x31, 0x34, 0x35, 0x37, 0x32, 0x30, 0x5A, 0x17, 0x0D, 0x31, 0x31, 0x30, 0x34, 0x31, 0x39, 0x31, 0x34, 0x35, 0x37, 0x32, 0x30, 0x5A, 0xA0, 0x2F, 0x30, 0x2D, 0x30, 0x1F, 0x06, 0x03, 0x55, 0x1D, 0x23, 0x04, 0x18, 0x30, 0x16, 0x80, 0x14, 0xC7, 0x47, 0x4F, 0x74, 0x22, 0x82, 0x8D, 0x90, 0x9A, 0x94, 0x98, 0x45, 0xB3, 0xB5, 0x43, 0xC3, 0x75, 0x18, 0x36, 0xCE, 0x30, 0x0A, 0x06, 0x03, 0x55, 0x1D, 0x14, 0x04, 0x03, 0x02, 0x01, 0x01, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 
			0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x05, 0x05, 0x00, 0x03, 0x81, 0x81, 0x00, 0x32, 0xBC, 0x12, 0x1F, 0x84, 0xD0, 0xB6, 0x3E, 0x72, 0xA0, 0xFB, 0xD9, 0x75, 0x99, 0xCA, 0xE5, 0x2A, 0x05, 0x09, 0xE6, 0xC8, 0x27, 0x74, 0x47, 0x1C, 0xDC, 0x0C, 0xD4, 0x9F, 0xBC, 0x9F, 0xB2, 0x62, 0x25, 0xB4, 0x6D, 0x5B, 0xE5, 0x0B, 0xE8, 0x2A, 0x8E, 0x07, 0xEB, 0x3E, 0x6B, 0xC5, 0x1E, 0x9A, 0xD2, 0x14, 0xFD, 0x89, 0x5B, 0xC3, 0x10, 0xBF, 0x19, 0x77, 0x67, 0x0A, 0x33, 0x45, 0x1B, 0xBC, 0x6C, 0xED, 0xAF, 0x84, 0x30, 0x59, 0xFB, 0x7C, 0x71, 0x95, 0x63, 0x60, 0x31, 0x9B, 0x9B, 0x0A, 0xEA, 0x77, 0xF1, 0x70, 0xF1, 0xB9, 
			0x2E, 0xD1, 0xA9, 0x04, 0x42, 0x66, 0x94, 0xB9, 0x54, 0x48, 0xDB, 0x44, 0x56, 0x56, 0x1A, 0x57, 0x5A, 0x01, 0x0E, 0x7C, 0x4D, 0xD7, 0xC0, 0x1F, 0x5C, 0x6F, 0x13, 0xF5, 0xA3, 0x57, 0x88, 0x6A, 0x9A, 0x71, 0xCD, 0xD5, 0xAE, 0xC3, 0x00, 0xB1, 0x28 };

		static public byte[] basicConstraintsCriticalcAFalseCACert_crt = { 0x30, 0x82, 0x02, 0x88, 0x30, 0x82, 0x01, 0xF1, 0xA0, 0x03, 0x02, 0x01, 0x02, 0x02, 0x01, 0x17, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x05, 0x05, 0x00, 0x30, 0x40, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x55, 0x53, 0x31, 0x1A, 0x30, 0x18, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x11, 0x54, 0x65, 0x73, 0x74, 0x20, 0x43, 0x65, 0x72, 0x74, 0x69, 0x66, 0x69, 0x63, 0x61, 0x74, 0x65, 0x73, 0x31, 0x15, 0x30, 0x13, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x0C, 0x54, 0x72, 0x75, 0x73, 0x74,
			0x20, 0x41, 0x6E, 0x63, 0x68, 0x6F, 0x72, 0x30, 0x1E, 0x17, 0x0D, 0x30, 0x31, 0x30, 0x34, 0x31, 0x39, 0x31, 0x34, 0x35, 0x37, 0x32, 0x30, 0x5A, 0x17, 0x0D, 0x31, 0x31, 0x30, 0x34, 0x31, 0x39, 0x31, 0x34, 0x35, 0x37, 0x32, 0x30, 0x5A, 0x30, 0x59, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x55, 0x53, 0x31, 0x1A, 0x30, 0x18, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x11, 0x54, 0x65, 0x73, 0x74, 0x20, 0x43, 0x65, 0x72, 0x74, 0x69, 0x66, 0x69, 0x63, 0x61, 0x74, 0x65, 0x73, 0x31, 0x2E, 0x30, 0x2C, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x25, 0x62, 0x61, 0x73, 0x69, 0x63, 0x43, 0x6F,
			0x6E, 0x73, 0x74, 0x72, 0x61, 0x69, 0x6E, 0x74, 0x73, 0x20, 0x43, 0x72, 0x69, 0x74, 0x69, 0x63, 0x61, 0x6C, 0x20, 0x63, 0x41, 0x20, 0x46, 0x61, 0x6C, 0x73, 0x65, 0x20, 0x43, 0x41, 0x30, 0x81, 0x9F, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00, 0x03, 0x81, 0x8D, 0x00, 0x30, 0x81, 0x89, 0x02, 0x81, 0x81, 0x00, 0xB2, 0x6A, 0x1C, 0xBC, 0x7B, 0x48, 0x87, 0x2D, 0x3A, 0xC6, 0x81, 0x94, 0xF1, 0x18, 0x29, 0x0E, 0xFD, 0x27, 0x04, 0x65, 0xC8, 0x3D, 0x25, 0xD1, 0x1B, 0xA3, 0xE8, 0xCB, 0xFF, 0x88, 0x1B, 0x16, 0xAB, 0xA5, 0xB1, 0xB4, 0xE8, 0xD9, 0x82, 0x99, 0xA1,
			0x7D, 0x62, 0xC4, 0x58, 0x28, 0xC2, 0xA5, 0xDF, 0x97, 0xC8, 0x62, 0x04, 0x52, 0x25, 0x7B, 0x19, 0x40, 0xD6, 0xCD, 0x74, 0x5A, 0x7E, 0x38, 0xFD, 0x2E, 0x9C, 0x47, 0x36, 0x82, 0x45, 0xA7, 0x44, 0x59, 0x1D, 0xE5, 0x46, 0x17, 0x69, 0x46, 0x14, 0xC3, 0xED, 0x63, 0xA1, 0x10, 0x50, 0xAE, 0xFE, 0xBA, 0x58, 0xDC, 0x0C, 0x91, 0x34, 0xA9, 0x16, 0x7C, 0xE6, 0x6F, 0x43, 0xEF, 0xBF, 0x1C, 0x3E, 0xE5, 0x8C, 0xBC, 0x30, 0xCC, 0xEF, 0x7D, 0x84, 0xF0, 0x9F, 0x0E, 0x06, 0x22, 0x02, 0xED, 0x4E, 0xB7, 0xDF, 0xA5, 0x2F, 0x41, 0xE1, 0x5F, 0x02, 0x03, 0x01, 0x00, 0x01, 0xA3, 0x79, 0x30, 0x77, 0x30, 0x1F, 0x06, 0x03,
			0x55, 0x1D, 0x23, 0x04, 0x18, 0x30, 0x16, 0x80, 0x14, 0xFB, 0x6C, 0xD4, 0x2D, 0x81, 0x9E, 0xCA, 0x27, 0x7A, 0x9E, 0x0D, 0xB0, 0x3C, 0xEA, 0x9A, 0xBC, 0x87, 0xFF, 0x49, 0xEA, 0x30, 0x1D, 0x06, 0x03, 0x55, 0x1D, 0x0E, 0x04, 0x16, 0x04, 0x14, 0xC7, 0x47, 0x4F, 0x74, 0x22, 0x82, 0x8D, 0x90, 0x9A, 0x94, 0x98, 0x45, 0xB3, 0xB5, 0x43, 0xC3, 0x75, 0x18, 0x36, 0xCE, 0x30, 0x0E, 0x06, 0x03, 0x55, 0x1D, 0x0F, 0x01, 0x01, 0xFF, 0x04, 0x04, 0x03, 0x02, 0x01, 0x06, 0x30, 0x17, 0x06, 0x03, 0x55, 0x1D, 0x20, 0x04, 0x10, 0x30, 0x0E, 0x30, 0x0C, 0x06, 0x0A, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x02, 0x01, 0x30,
			0x01, 0x30, 0x0C, 0x06, 0x03, 0x55, 0x1D, 0x13, 0x01, 0x01, 0xFF, 0x04, 0x02, 0x30, 0x00, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x05, 0x05, 0x00, 0x03, 0x81, 0x81, 0x00, 0x98, 0x98, 0x92, 0xB6, 0xE4, 0xE3, 0x0E, 0x5B, 0x19, 0x6F, 0x8A, 0x09, 0x58, 0xD3, 0x22, 0x86, 0xBD, 0x0A, 0x43, 0xC1, 0x3C, 0x89, 0xE0, 0x22, 0x66, 0xB0, 0x61, 0x88, 0xFE, 0xBB, 0x52, 0x8E, 0xF7, 0xC5, 0x8A, 0xA1, 0xCF, 0x9A, 0xAD, 0x9C, 0xFD, 0x01, 0xA6, 0xFA, 0x90, 0x1D, 0x28, 0xCA, 0xA4, 0x84, 0x65, 0xCD, 0x16, 0x35, 0x4F, 0x2A, 0x6F, 0x29, 0x2E, 0xAA, 0xC5, 0x6C, 0x09, 0xD6, 0x5D, 0x21,
			0xB9, 0x39, 0xE5, 0xAA, 0xE7, 0x01, 0x0C, 0x72, 0x43, 0x5E, 0xD5, 0x08, 0xE9, 0xF7, 0x56, 0xA0, 0xE6, 0x09, 0x96, 0x11, 0x5C, 0x1D, 0xAB, 0xCE, 0x9E, 0x37, 0x19, 0x24, 0x03, 0xAC, 0x3F, 0xFA, 0x18, 0xE9, 0x07, 0x1A, 0x02, 0x01, 0x3E, 0xF8, 0x73, 0xBE, 0x04, 0xA8, 0xFA, 0x4E, 0x5F, 0xC7, 0x3A, 0x34, 0x69, 0x48, 0xDD, 0x5C, 0x8D, 0xD2, 0x13, 0x83, 0xCD, 0xE3, 0xD2, 0x1C };

		[Test]
		public void basicConstraintsCriticalcAFalseCACRL ()
		{
			X509Crl crl = new X509Crl (basicConstraintsCriticalcAFalseCACRL_crl);
			Assert.AreEqual (0, crl.Entries.Count, "Entries.Count");
			Assert.AreEqual (2, crl.Extensions.Count, "Extensions.Count");
			Assert.IsFalse (crl.IsCurrent, "IsCurrent"); // true till 2011
			Assert.AreEqual ("C=US, O=Test Certificates, CN=basicConstraints Critical cA False CA", crl.IssuerName, "IssuerName");
			Assert.AreEqual (634388218400000000, crl.NextUpdate.ToUniversalTime ().Ticks, "NextUpdate");
			Assert.AreEqual ("32-BC-12-1F-84-D0-B6-3E-72-A0-FB-D9-75-99-CA-E5-2A-05-09-E6-C8-27-74-47-1C-DC-0C-D4-9F-BC-9F-B2-62-25-B4-6D-5B-E5-0B-E8-2A-8E-07-EB-3E-6B-C5-1E-9A-D2-14-FD-89-5B-C3-10-BF-19-77-67-0A-33-45-1B-BC-6C-ED-AF-84-30-59-FB-7C-71-95-63-60-31-9B-9B-0A-EA-77-F1-70-F1-B9-2E-D1-A9-04-42-66-94-B9-54-48-DB-44-56-56-1A-57-5A-01-0E-7C-4D-D7-C0-1F-5C-6F-13-F5-A3-57-88-6A-9A-71-CD-D5-AE-C3-00-B1-28", BitConverter.ToString (crl.Signature), "Signature");
			Assert.AreEqual ("1.2.840.113549.1.1.5", crl.SignatureAlgorithm, "SignatureAlgorithm");
			Assert.AreEqual (631232890400000000, crl.ThisUpdate.ToUniversalTime ().Ticks, "ThisUpdate");
			Assert.AreEqual (2, crl.Version, "Version");

			X509Certificate cert = new X509Certificate (basicConstraintsCriticalcAFalseCACert_crt);
			// certificate has CA set to false
			Assert.IsFalse (crl.VerifySignature (cert), "VerifySignature(cert)");
			Assert.IsTrue (crl.VerifySignature (cert.RSA), "VerifySignature(RSA)");
		}
	}
}
